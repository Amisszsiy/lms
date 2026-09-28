using Application.Common;
using Application.Errors;
using Application.Persistance;
using Application.Services;
using AutoMapper;
using Domain.Common;
using Domain.Entities;
using Domain.ValueObjects;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users
{
    public class UserService
    {
        private const string _includes = nameof(User.UserRoles);

        private readonly IUserRepository _repository;
        private readonly IRoleRepository _roleRepository;
        private readonly ICredentialRepository _credentialRepository;
        private readonly IMapper _mapper;
        private readonly RegisterValidator _registerValidator;
        private readonly LoginValidator _loginValidator;
        private readonly UpdateProfileValidator _updateProfileValidator;
        private readonly UpdateUserValidator _updateUserValidator;
        private readonly SearchUsersValidator _searchValidator;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ITokenService _tokenService;

        public UserService(IUserRepository repository,
            IRoleRepository roleRepository,
            ICredentialRepository credentialRepository,
            IMapper mapper,
            RegisterValidator registerValidator,
            LoginValidator loginValidator,
            UpdateProfileValidator updateProfileValidator,
            UpdateUserValidator updateUserValidator,
            SearchUsersValidator searchValidator,
            IPasswordHasher<User> passwordHasher,
            ITokenService tokenService)
        {
            _searchValidator = searchValidator;
            _repository = repository;
            _roleRepository = roleRepository;
            _credentialRepository = credentialRepository;
            _mapper = mapper;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _updateProfileValidator = updateProfileValidator;
            _updateUserValidator = updateUserValidator;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public async Task<Result<UserResponse>> GetUser(Guid id, CancellationToken cancellationToken = default)
        {
            User user = await _repository.GetAsync(u => u.Id == id, _includes, cancellationToken: cancellationToken);
            if (user is null) return Result<UserResponse>.Failure(RepositoryErrors.EntityNotFound);

            return Result<UserResponse>.Success(_mapper.Map<UserResponse>(user));
        }

        public async Task<Result<PagedResponse<UserResponse>>> SearchUsers(SearchUsersRequest request, CancellationToken cancellationToken = default)
        {
            var validateResult = await _searchValidator.ValidateAsync(request, cancellationToken);
            if (!validateResult.IsValid) return ValidationResult<PagedResponse<UserResponse>>.From(validateResult.Errors);

            // Email is stored through a value converter, so it can only be matched exactly, not by fragment.
            string search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();
            Result<EmailAddress> emailResult = EmailAddress.Create(search);
            EmailAddress email = emailResult.IsSuccess ? emailResult.Value : null;
            bool? isActive = request.IsActive;

            Expression<Func<User, bool>> filter = u =>
                (isActive == null || u.IsActive == isActive) &&
                (email != null ? u.Email == email : search == null || u.FirstName.Contains(search) || u.LastName.Contains(search));

            int totalCount = await _repository.CountAsync(filter, cancellationToken);
            IEnumerable<User> users = await _repository.GetAllAsync(filter, _includes, request.PageSize, request.PageNo,
                orderBy: q => q.OrderBy(u => u.FirstName).ThenBy(u => u.LastName).ThenBy(u => u.Id), tracked: false, cancellationToken: cancellationToken);

            return Result<PagedResponse<UserResponse>>.Success(new PagedResponse<UserResponse>
            {
                Items = _mapper.Map<List<UserResponse>>(users),
                PageNo = request.PageNo,
                PageSize = request.PageSize,
                TotalCount = totalCount
            });
        }

        public async Task<Result<UserResponse>> Register(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            var validateResult = await _registerValidator.ValidateAsync(request, cancellationToken);
            if (!validateResult.IsValid) return ValidationResult<UserResponse>.From(validateResult.Errors);

            Result<EmailAddress> emailResult = EmailAddress.Create(request.Email);
            if (emailResult.IsFailure) return Result<UserResponse>.Failure(emailResult.Error);

            if (await _repository.EmailExists(emailResult.Value, cancellationToken: cancellationToken)) return Result<UserResponse>.Failure(UserServiceErrors.EmailAlreadyExists);

            if (await _credentialRepository.UsernameExists(request.Username, cancellationToken)) return Result<UserResponse>.Failure(UserServiceErrors.UsernameAlreadyExists);

            int? normalUserRoleId = await _roleRepository.GetIdByName(RoleNames.NormalUser, cancellationToken);
            if (normalUserRoleId is null) return Result<UserResponse>.Failure(UserServiceErrors.DefaultRoleNotFound);

            Result<User> createResult = User.Create(request.FirstName, request.LastName, emailResult.Value, request.Tel);
            if (createResult.IsFailure) return Result<UserResponse>.Failure(createResult.Error);

            User user = createResult.Value;
            // Default active after register
            user.SetActive(true);

            Result roleResult = user.AssignRole(normalUserRoleId.Value);
            if (roleResult.IsFailure) return Result<UserResponse>.Failure(roleResult.Error);

            Result<Credential> credentialResult = Credential.Create(request.Username, _passwordHasher.HashPassword(user, request.Password), user);
            if (credentialResult.IsFailure) return Result<UserResponse>.Failure(credentialResult.Error);

            await _repository.AddAsync(user, cancellationToken);
            await _credentialRepository.AddAsync(credentialResult.Value, cancellationToken);
            await _repository.CommitAsync(cancellationToken);

            return Result<UserResponse>.Success(_mapper.Map<UserResponse>(user));
        }

        // Returns a JWT. Unknown username and wrong password share one error so usernames can't be probed.
        public async Task<Result<string>> Login(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var validateResult = await _loginValidator.ValidateAsync(request, cancellationToken);
            if (!validateResult.IsValid) return ValidationResult<string>.From(validateResult.Errors);

            string username = request.Username.Trim();
            Credential credential = await _credentialRepository.GetAsync(c => c.Username == username, $"{nameof(Credential.User)}.{nameof(User.UserRoles)}", cancellationToken: cancellationToken);
            if (credential is null) return Result<string>.Failure(UserServiceErrors.InvalidCredentials);

            PasswordVerificationResult verifyResult = _passwordHasher.VerifyHashedPassword(credential.User, credential.PasswordHash, request.Password);
            if (verifyResult == PasswordVerificationResult.Failed) return Result<string>.Failure(UserServiceErrors.InvalidCredentials);

            // Checked after the password so account state is only revealed to the owner.
            if (!credential.User.IsActive) return Result<string>.Failure(UserServiceErrors.UserInactive);

            List<string> roleNames = await _roleRepository.GetNamesByIds(credential.User.UserRoles.Select(r => r.RoleId).ToList(), cancellationToken);

            return Result<string>.Success(_tokenService.Create(credential.User, roleNames));
        }

        public async Task<Result<UserResponse>> UpdateProfile(Guid id, UpdateProfileRequest request, CancellationToken cancellationToken = default)
        {
            var validateResult = await _updateProfileValidator.ValidateAsync(request, cancellationToken);
            if (!validateResult.IsValid) return ValidationResult<UserResponse>.From(validateResult.Errors);

            Result<EmailAddress> emailResult = EmailAddress.Create(request.Email);
            if (emailResult.IsFailure) return Result<UserResponse>.Failure(emailResult.Error);

            User user = await _repository.GetAsync(u => u.Id == id, _includes, tracked: true, cancellationToken: cancellationToken);
            if (user is null) return Result<UserResponse>.Failure(RepositoryErrors.EntityNotFound);

            if (await _repository.EmailExists(emailResult.Value, user.Id, cancellationToken)) return Result<UserResponse>.Failure(UserServiceErrors.EmailAlreadyExists);

            ApplyProfile(user, request, emailResult.Value);

            _repository.Update(user);
            await _repository.CommitAsync(cancellationToken);

            return Result<UserResponse>.Success(_mapper.Map<UserResponse>(user));
        }

        public async Task<Result<UserResponse>> UpdateUser(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
        {
            var validateResult = await _updateUserValidator.ValidateAsync(request, cancellationToken);
            if (!validateResult.IsValid) return ValidationResult<UserResponse>.From(validateResult.Errors);

            Result<EmailAddress> emailResult = EmailAddress.Create(request.Email);
            if (emailResult.IsFailure) return Result<UserResponse>.Failure(emailResult.Error);

            User user = await _repository.GetAsync(u => u.Id == id, _includes, tracked: true, cancellationToken: cancellationToken);
            if (user is null) return Result<UserResponse>.Failure(RepositoryErrors.EntityNotFound);

            if (await _repository.EmailExists(emailResult.Value, user.Id, cancellationToken)) return Result<UserResponse>.Failure(UserServiceErrors.EmailAlreadyExists);

            if (!await _roleRepository.AllExist(request.RoleIds.Distinct().ToList(), cancellationToken)) return Result<UserResponse>.Failure(UserServiceErrors.RoleNotFound);

            ApplyProfile(user, request, emailResult.Value);

            Result rolesResult = SyncRoles(user, request.RoleIds);
            if (rolesResult.IsFailure) return Result<UserResponse>.Failure(rolesResult.Error);

            _repository.Update(user);
            await _repository.CommitAsync(cancellationToken);

            return Result<UserResponse>.Success(_mapper.Map<UserResponse>(user));
        }

        public async Task<Result> ActivateUser(Guid id, CancellationToken cancellationToken = default)
        {
            User user = await _repository.GetAsync(u => u.Id == id, tracked: true, cancellationToken: cancellationToken);
            if (user is null) return Result.Failure(RepositoryErrors.EntityNotFound);

            user.SetActive(true);

            _repository.Update(user);
            await _repository.CommitAsync(cancellationToken);

            return Result.Success();
        }

        // Soft delete: user is deactivated for remaining borrowing history.
        public async Task<Result> DeleteUser(Guid id, CancellationToken cancellationToken = default)
        {
            User user = await _repository.GetAsync(u => u.Id == id, tracked: true, cancellationToken: cancellationToken);
            if (user is null) return Result.Failure(RepositoryErrors.EntityNotFound);

            if (await _repository.HasOpenBorrowings(user.Id, cancellationToken)) return Result.Failure(UserServiceErrors.HasOpenBorrowings);

            user.SetActive(false);

            _repository.Update(user);
            await _repository.CommitAsync(cancellationToken);

            return Result.Success();
        }

        private static void ApplyProfile(User user, IUserProfileRequest request, EmailAddress email)
        {
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = email;
            user.Tel = request.Tel;
        }

        private static Result SyncRoles(User user, IEnumerable<int> requestedRoleIds)
        {
            HashSet<int> roleIds = requestedRoleIds.ToHashSet();
            foreach (int roleId in user.UserRoles.Select(r => r.RoleId).Where(id => !roleIds.Contains(id)).ToList())
            {
                Result result = user.RemoveRole(roleId);
                if (result.IsFailure) return result;
            }
            foreach (int roleId in roleIds.Where(id => user.UserRoles.All(r => r.RoleId != id)).ToList())
            {
                Result result = user.AssignRole(roleId);
                if (result.IsFailure) return result;
            }

            return Result.Success();
        }
    }
}
