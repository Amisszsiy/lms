using Domain.Common;
using FluentValidation.Results;

namespace Application.Common
{
    // Result carrying every validation failure, not just one.
    public sealed class ValidationResult<T> : Result<T>
    {
        public static readonly Error ValidationFailed = new Error("Validation.Failed", "One or more validation errors occurred.");

        private ValidationResult(IReadOnlyList<Error> errors) : base(false, ValidationFailed, default)
        {
            Errors = errors;
        }

        public IReadOnlyList<Error> Errors { get; }

        public static ValidationResult<T> From(IEnumerable<ValidationFailure> failures) =>
            new ValidationResult<T>(failures.Select(f => new Error(f.ErrorCode, f.ErrorMessage)).ToList());
    }
}
