using Domain.Errors;
using FluentValidation;

namespace Application.Features.Genres
{
    public class GenreValidator : AbstractValidator<SaveGenreRequest>
    {
        // Length follows GenreConfiguration
        public GenreValidator()
        {
            RuleFor(genre => genre.GenreName)
                .NotEmpty().WithErrorCode(GenreErrors.NameNotProvided.Code).WithMessage(GenreErrors.NameNotProvided.Description)
                .MaximumLength(100).WithErrorCode("Genre.NameTooLong").WithMessage("Genre name must not exceed 100 characters.");
        }
    }
}
