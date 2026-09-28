using Domain.Errors;
using FluentValidation;

namespace Application.Features.Locations
{
    public class LocationValidator : AbstractValidator<SaveLocationRequest>
    {
        // Lengths follow LocationConfiguration
        public LocationValidator()
        {
            RuleFor(location => location.Area)
                .NotEmpty().WithErrorCode(LocationErrors.PositionNotProvided.Code).WithMessage(LocationErrors.PositionNotProvided.Description)
                .MaximumLength(50).WithErrorCode("Location.AreaTooLong").WithMessage("Area must not exceed 50 characters.");

            RuleFor(location => location.LineNo)
                .NotEmpty().WithErrorCode(LocationErrors.PositionNotProvided.Code).WithMessage(LocationErrors.PositionNotProvided.Description)
                .MaximumLength(10).WithErrorCode("Location.LineNoTooLong").WithMessage("Line number must not exceed 10 characters.");

            RuleFor(location => location.ShelfNo)
                .NotEmpty().WithErrorCode(LocationErrors.PositionNotProvided.Code).WithMessage(LocationErrors.PositionNotProvided.Description)
                .MaximumLength(10).WithErrorCode("Location.ShelfNoTooLong").WithMessage("Shelf number must not exceed 10 characters.");

            RuleFor(location => location.RowNo)
                .GreaterThan(0).WithErrorCode(LocationErrors.InvalidRowOrColumn.Code).WithMessage(LocationErrors.InvalidRowOrColumn.Description);

            RuleFor(location => location.ColumnNo)
                .GreaterThan(0).WithErrorCode(LocationErrors.InvalidRowOrColumn.Code).WithMessage(LocationErrors.InvalidRowOrColumn.Description);
        }
    }
}
