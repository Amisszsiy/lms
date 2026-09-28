using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Errors
{
    public static class LocationErrors
    {
        public static readonly Error PositionNotProvided = new Error("Location.PositionNotProvided", "Area, line number and shelf number are required.");
        public static readonly Error InvalidRowOrColumn = new Error("Location.InvalidRowOrColumn", "Row and column numbers must be greater than zero.");
    }
}
