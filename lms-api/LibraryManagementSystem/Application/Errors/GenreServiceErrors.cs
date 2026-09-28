using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Errors
{
    public static class GenreServiceErrors
    {
        public static readonly Error NameAlreadyExists = new Error("Genre.NameAlreadyExists", "Another genre already uses this name.");
    }
}
