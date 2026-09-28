using FluentValidation;

namespace Application.Features.Locations
{
    public class SearchLocationsValidator : AbstractValidator<SearchLocationsRequest>
    {
        public SearchLocationsValidator()
        {
            RuleFor(query => query.Area)
                .MaximumLength(50).WithErrorCode("Location.AreaTooLong").WithMessage("Area must not exceed 50 characters.");

            RuleFor(query => query.LineNo)
                .MaximumLength(10).WithErrorCode("Location.LineNoTooLong").WithMessage("Line number must not exceed 10 characters.");

            RuleFor(query => query.ShelfNo)
                .MaximumLength(10).WithErrorCode("Location.ShelfNoTooLong").WithMessage("Shelf number must not exceed 10 characters.");

            RuleFor(query => query.PageNo)
                .GreaterThanOrEqualTo(1).WithErrorCode("Paging.InvalidPageNo").WithMessage("Page number must be at least 1.");

            RuleFor(query => query.PageSize)
                .InclusiveBetween(1, SearchLocationsRequest.MaxPageSize).WithErrorCode("Paging.InvalidPageSize").WithMessage($"Page size must be between 1 and {SearchLocationsRequest.MaxPageSize}.");
        }
    }
}
