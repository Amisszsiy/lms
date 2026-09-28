using Application.Common;
using Application.Errors;
using Application.Persistance;
using AutoMapper;
using Domain.Common;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Authors
{
    public class AuthorService
    {
        private const string _includes = nameof(Author.PenNames);

        private readonly IAuthorRepository _repository;
        private readonly IMapper _mapper;
        private readonly AuthorValidator _validator;
        private readonly SearchAuthorsValidator _searchValidator;
        public AuthorService(IAuthorRepository repository, IMapper mapper, AuthorValidator validator, SearchAuthorsValidator searchValidator)
        {
            _repository = repository;
            _mapper = mapper;
            _validator = validator;
            _searchValidator = searchValidator;
        }

        public async Task<Result<AuthorResponse>> GetAuthor(int id, CancellationToken cancellationToken = default)
        {
            Author author = await _repository.GetAsync(a => a.Id == id, _includes, cancellationToken: cancellationToken);
            if (author is null) return Result<AuthorResponse>.Failure(RepositoryErrors.EntityNotFound);

            return Result<AuthorResponse>.Success(_mapper.Map<AuthorResponse>(author));
        }

        public async Task<Result<PagedResponse<AuthorResponse>>> SearchAuthors(SearchAuthorsRequest request, CancellationToken cancellationToken = default)
        {
            var validateResult = await _searchValidator.ValidateAsync(request, cancellationToken);
            if (!validateResult.IsValid) return ValidationResult<PagedResponse<AuthorResponse>>.From(validateResult.Errors);

            string search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();

            Expression<Func<Author, bool>> filter = a =>
                search == null ||
                a.FirstName.Contains(search) ||
                a.LastName.Contains(search) ||
                a.PenNames.Any(p => p.Name.Contains(search));

            int totalCount = await _repository.CountAsync(filter, cancellationToken);
            IEnumerable<Author> authors = await _repository.GetAllAsync(filter, _includes, request.PageSize, request.PageNo,
                orderBy: q => q.OrderBy(a => a.FirstName).ThenBy(a => a.LastName).ThenBy(a => a.Id), tracked: false, cancellationToken: cancellationToken);

            return Result<PagedResponse<AuthorResponse>>.Success(new PagedResponse<AuthorResponse>
            {
                Items = _mapper.Map<List<AuthorResponse>>(authors),
                PageNo = request.PageNo,
                PageSize = request.PageSize,
                TotalCount = totalCount
            });
        }

        public async Task<Result<AuthorResponse>> AddAuthor(SaveAuthorRequest request, CancellationToken cancellationToken = default)
        {
            var validateResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validateResult.IsValid) return ValidationResult<AuthorResponse>.From(validateResult.Errors);

            Result<Author> createResult = Author.Create(request.FirstName, request.LastName);
            if (createResult.IsFailure) return Result<AuthorResponse>.Failure(createResult.Error);

            Author author = createResult.Value;

            Result penNameResult = SyncPenNames(author, request.PenNames);
            if (penNameResult.IsFailure) return Result<AuthorResponse>.Failure(penNameResult.Error);

            await _repository.AddAsync(author, cancellationToken);
            await _repository.CommitAsync(cancellationToken);

            return Result<AuthorResponse>.Success(_mapper.Map<AuthorResponse>(author));
        }

        public async Task<Result<AuthorResponse>> UpdateAuthor(int id, SaveAuthorRequest request, CancellationToken cancellationToken = default)
        {
            var validateResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validateResult.IsValid) return ValidationResult<AuthorResponse>.From(validateResult.Errors);

            Author author = await _repository.GetAsync(a => a.Id == id, _includes, tracked: true, cancellationToken: cancellationToken);
            if (author is null) return Result<AuthorResponse>.Failure(RepositoryErrors.EntityNotFound);

            author.FirstName = request.FirstName;
            author.LastName = request.LastName;

            Result penNameResult = SyncPenNames(author, request.PenNames);
            if (penNameResult.IsFailure) return Result<AuthorResponse>.Failure(penNameResult.Error);

            _repository.Update(author);
            await _repository.CommitAsync(cancellationToken);

            return Result<AuthorResponse>.Success(_mapper.Map<AuthorResponse>(author));
        }

        public async Task<Result> DeleteAuthor(int id, CancellationToken cancellationToken = default)
        {
            Author author = await _repository.GetAsync(a => a.Id == id, _includes, tracked: true, cancellationToken: cancellationToken);
            if (author is null) return Result.Failure(RepositoryErrors.EntityNotFound);

            bool isBeingReferenced = await _repository.IsUsing(author.Id, cancellationToken);
            if (isBeingReferenced) return Result.Failure(RepositoryErrors.EntityStillBeingReferenced);

            _repository.Remove(author);
            await _repository.CommitAsync(cancellationToken);

            return Result.Success();
        }

        private static Result SyncPenNames(Author author, IEnumerable<string> penNames)
        {
            HashSet<string> names = (penNames ?? []).ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (string name in author.PenNames.Select(p => p.Name).Where(n => !names.Contains(n)).ToList())
            {
                Result result = author.RemovePenName(name);
                if (result.IsFailure) return result;
            }
            foreach (string name in names.Where(n => !author.PenNames.Any(p => string.Equals(p.Name, n, StringComparison.OrdinalIgnoreCase))).ToList())
            {
                Result result = author.AddPenName(name);
                if (result.IsFailure) return result;
            }

            return Result.Success();
        }
    }
}
