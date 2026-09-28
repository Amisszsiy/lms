using FluentValidation;

namespace Application.Features.Books
{
    public class SearchBooksValidator : AbstractValidator<SearchBooksRequest>
    {
        public SearchBooksValidator()
        {
            RuleFor(query => query.Search)
                .MaximumLength(200).WithErrorCode("Book.SearchTooLong").WithMessage("Search text must not exceed 200 characters.");

            RuleFor(query => query.PageNo)
                .GreaterThanOrEqualTo(1).WithErrorCode("Paging.InvalidPageNo").WithMessage("Page number must be at least 1.");

            RuleFor(query => query.PageSize)
                .InclusiveBetween(1, SearchBooksRequest.MaxPageSize).WithErrorCode("Paging.InvalidPageSize").WithMessage($"Page size must be between 1 and {SearchBooksRequest.MaxPageSize}.");
        }
    }
}
