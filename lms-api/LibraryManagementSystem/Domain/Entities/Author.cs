using Domain.Common;
using Domain.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Author : BaseEntity<int>, IAuditable
    {
        private readonly List<PenName> _penNames = new List<PenName>();

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IReadOnlyCollection<PenName> PenNames => _penNames.AsReadOnly();

        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public static Result<Author> Create(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName)) return Result<Author>.Failure(AuthorErrors.FirstNameNotProvided);

            Author author = new Author()
            {
                FirstName = firstName,
                LastName = lastName
            };

            return Result<Author>.Success(author);
        }

        public Result AddPenName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return Result.Failure(AuthorErrors.PenNameNotProvided);
            if (_penNames.Any(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))) return Result.Failure(AuthorErrors.PenNameAlreadyExists);

            _penNames.Add(PenName.Create(name));

            return Result.Success();
        }

        public Result RemovePenName(string name)
        {
            PenName penName = _penNames.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
            if (penName is null) return Result.Failure(AuthorErrors.PenNameNotFound);

            _penNames.Remove(penName);

            return Result.Success();
        }
    }
}
