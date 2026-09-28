using Domain.Common;
using Domain.Errors;
using System.Text.RegularExpressions;

namespace Domain.ValueObjects
{
    public sealed record EmailAddress
    {
        public const int MaxLength = 254;

        private const string Pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        private EmailAddress(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static Result<EmailAddress> Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Result<EmailAddress>.Failure(EmailAddressErrors.Empty);
            }

            if (email.Length > MaxLength)
            {
                return Result<EmailAddress>.Failure(EmailAddressErrors.TooLong);
            }

            if (!Regex.IsMatch(email, Pattern, RegexOptions.IgnoreCase))
            {
                return Result<EmailAddress>.Failure(EmailAddressErrors.InvalidFormat);
            }

            return Result<EmailAddress>.Success(new EmailAddress(email));
        }

        public override string ToString() => Value;
    }
}
