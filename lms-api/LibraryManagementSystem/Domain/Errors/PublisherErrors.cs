using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Errors
{
    public static class PublisherErrors
    {
        public static readonly Error NameNotProvided = new Error("Publisher.NameNotProvided", "Publisher name is required.");
        public static readonly Error AddressNotProvided = new Error("Publisher.AddressNotProvided", "Publisher address is required.");
    }
}
