using Domain.Errors;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Borrowings
{
    public class BorrowValidator : AbstractValidator<BorrowRequest>
    {
        public BorrowValidator()
        {
            RuleFor(borrow => borrow.BookCopyId)
                .NotEmpty().WithErrorCode(BorrowingErrors.BookCopyNotProvided.Code).WithMessage(BorrowingErrors.BookCopyNotProvided.Description);
        }
    }

    public class AssignBorrowValidator : AbstractValidator<AssignBorrowRequest>
    {
        public AssignBorrowValidator()
        {
            RuleFor(borrow => borrow.UserId)
                .NotEmpty().WithErrorCode(BorrowingErrors.UserNotProvided.Code).WithMessage(BorrowingErrors.UserNotProvided.Description);

            RuleFor(borrow => borrow.BookCopyId)
                .NotEmpty().WithErrorCode(BorrowingErrors.BookCopyNotProvided.Code).WithMessage(BorrowingErrors.BookCopyNotProvided.Description);
        }
    }
}
