using Domain.Errors;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.BookCopies
{
    public class CreateBookCopyValidator : AbstractValidator<CreateBookCopyRequest>
    {
        public CreateBookCopyValidator()
        {
            RuleFor(copy => copy.BookId)
                .GreaterThan(0).WithErrorCode(BookCopyErrors.BookNotProvided.Code).WithMessage(BookCopyErrors.BookNotProvided.Description);

            RuleFor(copy => copy.LocationId)
                .GreaterThan(0).WithErrorCode(BookCopyErrors.LocationNotProvided.Code).WithMessage(BookCopyErrors.LocationNotProvided.Description);
        }
    }

    public class UpdateBookCopyValidator : AbstractValidator<UpdateBookCopyRequest>
    {
        public UpdateBookCopyValidator()
        {
            RuleFor(copy => copy.LocationId)
                .GreaterThan(0).WithErrorCode(BookCopyErrors.LocationNotProvided.Code).WithMessage(BookCopyErrors.LocationNotProvided.Description);

            RuleFor(copy => copy.Status)
                .IsInEnum().WithErrorCode("BookCopy.InvalidStatus").WithMessage("Status is not a valid book copy status.");
        }
    }
}
