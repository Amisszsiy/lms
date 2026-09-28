using Domain.Common;
using Domain.Errors;

namespace Domain.ValueObjects
{
    public sealed record Isbn
    {
        public const int Length = 13;

        private Isbn(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static Result<Isbn> Create(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
            {
                return Result<Isbn>.Failure(IsbnErrors.Empty);
            }

            var digits = isbn.Replace("-", string.Empty);

            if (digits.Length != Length || !digits.All(char.IsAsciiDigit))
            {
                return Result<Isbn>.Failure(IsbnErrors.InvalidFormat);
            }

            return Result<Isbn>.Success(new Isbn(digits));
        }

        public override string ToString() => Value;
    }
}
