using Domain.Common;
using Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Errors
{
    public static class IsbnErrors
    {
        public static readonly Error Empty = new Error("Isbn.Empty", "ISBN is required.");

        public static readonly Error InvalidFormat = new Error("Isbn.InvalidFormat", $"ISBN must be exactly {Isbn.Length} digits.");
    }
}
