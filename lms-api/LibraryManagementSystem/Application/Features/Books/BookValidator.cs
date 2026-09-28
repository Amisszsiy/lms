using Domain.Errors;
using Domain.ValueObjects;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Books
{
    public class BookValidator : AbstractValidator<SaveBookRequest>
    {
        public BookValidator()
        {
            RuleFor(book => book.Isbn)
                .NotEmpty().WithErrorCode(IsbnErrors.Empty.Code).WithMessage(IsbnErrors.Empty.Description);

            RuleFor(book => book.Title)
                .NotEmpty().WithErrorCode(BookErrors.TitleNotProvided.Code).WithMessage(BookErrors.TitleNotProvided.Description)
                .MaximumLength(200).WithErrorCode("Book.TitleTooLong").WithMessage("Book title must not exceed 200 characters.");

            RuleFor(book => book.PublisherId)
                .GreaterThan(0).WithErrorCode(BookErrors.PublisherNotProvided.Code).WithMessage(BookErrors.PublisherNotProvided.Description);

            RuleFor(book => book.PublicationYear)
                .Must(year => year > 0 && year <= DateTime.UtcNow.Year).WithErrorCode(BookErrors.InvalidPublicationYear.Code).WithMessage(BookErrors.InvalidPublicationYear.Description);

            RuleFor(book => book.AuthorIds)
                .NotEmpty().WithErrorCode("Book.AuthorsNotProvided").WithMessage("At least one author is required.");

            RuleForEach(book => book.AuthorIds)
                .GreaterThan(0).WithErrorCode("Book.InvalidAuthorId").WithMessage("Author id must be greater than 0.");

            RuleFor(book => book.GenreIds)
                .NotEmpty().WithErrorCode("Book.GenresNotProvided").WithMessage("At least one genre is required.");

            RuleForEach(book => book.GenreIds)
                .GreaterThan(0).WithErrorCode("Book.InvalidGenreId").WithMessage("Genre id must be greater than 0.");
        }
    }
}
