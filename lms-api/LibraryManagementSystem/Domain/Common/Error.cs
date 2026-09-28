using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common
{
    public sealed record Error(string Code, string Description)
    {
        public static readonly Error None = new Error(string.Empty, string.Empty);
    }
}
