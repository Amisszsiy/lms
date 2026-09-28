using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Errors
{
    public static class RepositoryErrors
    {
        public static readonly Error EntityNotFound = new Error("Repository.EntityNotFound", "Entity not found");
        public static readonly Error EntityStillBeingReferenced = new Error("Repository.EntityStillBeingReferenced", "Can't delete entity that being referenced");
    }
}
