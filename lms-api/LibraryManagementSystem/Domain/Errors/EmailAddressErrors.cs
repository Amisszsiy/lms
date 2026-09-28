using Domain.Common;
using Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Errors
{
    public static class EmailAddressErrors
    {
        public static readonly Error Empty = new Error("EmailAddress.Empty", "Email address is required.");

        public static readonly Error TooLong = new Error("EmailAddress.TooLong", $"Email address must not exceed {EmailAddress.MaxLength} characters.");

        public static readonly Error InvalidFormat = new Error("EmailAddress.InvalidFormat", "Email address format is invalid.");
    }
}
