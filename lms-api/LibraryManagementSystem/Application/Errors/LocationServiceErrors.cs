using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Errors
{
    public static class LocationServiceErrors
    {
        public static readonly Error PositionAlreadyExists = new Error("Location.PositionAlreadyExists", "Another location already occupies this position.");
    }
}
