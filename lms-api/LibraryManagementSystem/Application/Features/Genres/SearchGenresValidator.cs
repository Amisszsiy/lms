using FluentValidation;

namespace Application.Features.Genres
{
    public class SearchGenresValidator : AbstractValidator<SearchGenresRequest>
    {
        public SearchGenresValidator()
        {
            RuleFor(query => query.Search)
                .MaximumLength(100).WithErrorCode("Genre.SearchTooLong").WithMessage("Search text must not exceed 100 characters.");

            RuleFor(query => query.PageNo)
                .GreaterThanOrEqualTo(1).WithErrorCode("Paging.InvalidPageNo").WithMessage("Page number must be at least 1.");

            RuleFor(query => query.PageSize)
                .InclusiveBetween(1, SearchGenresRequest.MaxPageSize).WithErrorCode("Paging.InvalidPageSize").WithMessage($"Page size must be between 1 and {SearchGenresRequest.MaxPageSize}.");
        }
    }
}
