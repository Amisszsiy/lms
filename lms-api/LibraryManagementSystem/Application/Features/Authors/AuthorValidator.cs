using Domain.Errors;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Authors
{
    public class AuthorValidator : AbstractValidator<SaveAuthorRequest>
    {
        // Lengths follow the Author and PenName table definitions in Tables.txt
        public AuthorValidator()
        {
            RuleFor(author => author.FirstName)
                .NotEmpty().WithErrorCode(AuthorErrors.FirstNameNotProvided.Code).WithMessage(AuthorErrors.FirstNameNotProvided.Description)
                .MaximumLength(100).WithErrorCode("Author.FirstNameTooLong").WithMessage("First name must not exceed 100 characters.");

            RuleFor(author => author.LastName)
                .MaximumLength(100).WithErrorCode("Author.LastNameTooLong").WithMessage("Last name must not exceed 100 characters.");

            RuleForEach(author => author.PenNames)
                .NotEmpty().WithErrorCode(AuthorErrors.PenNameNotProvided.Code).WithMessage(AuthorErrors.PenNameNotProvided.Description)
                .MaximumLength(100).WithErrorCode("Author.PenNameTooLong").WithMessage("Pen name must not exceed 100 characters.");

            RuleFor(author => author.PenNames)
                .Must(names => names.Distinct(StringComparer.OrdinalIgnoreCase).Count() == names.Count)
                .When(author => author.PenNames is not null)
                .WithErrorCode(AuthorErrors.PenNameAlreadyExists.Code).WithMessage("Pen names must not contain duplicates.");
        }
    }
}
