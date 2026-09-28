using FluentValidation;

namespace Application.Features.Borrowings
{
    public class SearchBorrowingsValidator : AbstractValidator<SearchBorrowingsRequest>
    {
        public SearchBorrowingsValidator()
        {
            RuleFor(query => query.Search)
                .MaximumLength(200).WithErrorCode("Borrowing.SearchTooLong").WithMessage("Search text must not exceed 200 characters.");

            RuleFor(query => query.Status)
                .IsInEnum().WithErrorCode("Borrowing.InvalidStatus").WithMessage("Status is not a valid borrowing status.");

            RuleFor(query => query.PageNo)
                .GreaterThanOrEqualTo(1).WithErrorCode("Paging.InvalidPageNo").WithMessage("Page number must be at least 1.");

            RuleFor(query => query.PageSize)
                .InclusiveBetween(1, SearchBorrowingsRequest.MaxPageSize).WithErrorCode("Paging.InvalidPageSize").WithMessage($"Page size must be between 1 and {SearchBorrowingsRequest.MaxPageSize}.");
        }
    }
}
