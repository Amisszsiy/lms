using Domain.Common;
using Domain.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Genre : BaseEntity<int>, IAuditable
    {
        public string GenreName { get; private set; }

        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public static Result<Genre> Create(string genreName)
        {
            if (string.IsNullOrWhiteSpace(genreName)) return Result<Genre>.Failure(GenreErrors.NameNotProvided);

            return Result<Genre>.Success(new Genre() { GenreName = genreName });
        }

        public Result Rename(string genreName)
        {
            if (string.IsNullOrWhiteSpace(genreName)) return Result.Failure(GenreErrors.NameNotProvided);

            GenreName = genreName;

            return Result.Success();
        }
    }
}
