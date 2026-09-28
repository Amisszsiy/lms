using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Errors
{
    public static class GenreErrors
    {
        public static readonly Error NameNotProvided = new Error("Genre.NameNotProvided", "Genre name is required.");
    }
}
