using Application.Errors;
using Domain.Errors;
using Domain.ValueObjects;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users
{
    public class UserProfileValidator : AbstractValidator<IUserProfileRequest>
    {
        public UserProfileValidator()
        {
            RuleFor(user => user.FirstName)
                .NotEmpty().WithErrorCode(UserErrors.NameNotProvided.Code).WithMessage(UserErrors.NameNotProvided.Description)
                .MaximumLength(100).WithErrorCode("User.FirstNameTooLong").WithMessage("First name must not exceed 100 characters.");

            RuleFor(user => user.LastName)
                .NotEmpty().WithErrorCode(UserErrors.NameNotProvided.Code).WithMessage(UserErrors.NameNotProvided.Description)
                .MaximumLength(100).WithErrorCode("User.LastNameTooLong").WithMessage("Last name must not exceed 100 characters.");

            RuleFor(user => user.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithErrorCode(EmailAddressErrors.Empty.Code).WithMessage(EmailAddressErrors.Empty.Description)
                .MaximumLength(EmailAddress.MaxLength).WithErrorCode(EmailAddressErrors.TooLong.Code).WithMessage(EmailAddressErrors.TooLong.Description)
                .EmailAddress().WithErrorCode(EmailAddressErrors.InvalidFormat.Code).WithMessage(EmailAddressErrors.InvalidFormat.Description);

            RuleFor(user => user.Tel)
                .MaximumLength(15).WithErrorCode("User.TelTooLong").WithMessage("Tel must not exceed 15 characters.")
                .Matches(@"^\+?[0-9\- ]+$").WithErrorCode("User.TelInvalid").WithMessage("Tel may only contain digits, spaces, '-' and a leading '+'.")
                .When(user => !string.IsNullOrEmpty(user.Tel));
        }
    }

    public class RegisterValidator : AbstractValidator<RegisterRequest>
    {
        public const int PasswordMinLength = 8;
        public const int PasswordMaxLength = 100;

        public RegisterValidator()
        {
            Include(new UserProfileValidator());

            RuleFor(user => user.Username)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithErrorCode(CredentialErrors.UsernameNotProvided.Code).WithMessage(CredentialErrors.UsernameNotProvided.Description)
                .Length(3, Credential.UsernameMaxLength).WithErrorCode("Credential.UsernameInvalidLength").WithMessage($"Username must be between 3 and {Credential.UsernameMaxLength} characters.")
                .Matches(@"^[a-zA-Z0-9._-]+$").WithErrorCode("Credential.UsernameInvalid").WithMessage("Username may only contain letters, digits, '.', '_' and '-'.");

            RuleFor(user => user.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithErrorCode(CredentialErrors.PasswordNotProvided.Code).WithMessage(CredentialErrors.PasswordNotProvided.Description)
                .Length(PasswordMinLength, PasswordMaxLength).WithErrorCode("Credential.PasswordInvalidLength").WithMessage($"Password must be between {PasswordMinLength} and {PasswordMaxLength} characters.")
                .Matches("[a-zA-Z]").WithErrorCode("Credential.PasswordNoLetter").WithMessage("Password must contain at least one letter.")
                .Matches("[0-9]").WithErrorCode("Credential.PasswordNoDigit").WithMessage("Password must contain at least one digit.");
        }
    }

    // Only presence is checked: format rules belong to registration and would leak policy on login.
    public class LoginValidator : AbstractValidator<LoginRequest>
    {
        public LoginValidator()
        {
            RuleFor(login => login.Username)
                .NotEmpty().WithErrorCode(CredentialErrors.UsernameNotProvided.Code).WithMessage(CredentialErrors.UsernameNotProvided.Description);

            RuleFor(login => login.Password)
                .NotEmpty().WithErrorCode(CredentialErrors.PasswordNotProvided.Code).WithMessage(CredentialErrors.PasswordNotProvided.Description);
        }
    }

    public class UpdateProfileValidator : AbstractValidator<UpdateProfileRequest>
    {
        public UpdateProfileValidator()
        {
            Include(new UserProfileValidator());
        }
    }

    public class UpdateUserValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserValidator()
        {
            Include(new UserProfileValidator());

            RuleFor(user => user.RoleIds)
                .NotEmpty().WithErrorCode("User.RolesNotProvided").WithMessage("At least one role is required.");

            RuleForEach(user => user.RoleIds)
                .GreaterThan(0).WithErrorCode("User.InvalidRoleId").WithMessage("Role id must be greater than 0.");
        }
    }
}
