using Domain.Common;
using Domain.Errors;
using Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class User : BaseEntity<Guid>
    {
        private readonly List<UserRole> _userRoles = new List<UserRole>();

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public EmailAddress Email { get; set; }
        public string Tel { get; set; }
        public bool IsActive { get; private set; }
        public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

        public static Result<User> Create(string firstName, string lastName, EmailAddress email, string tel)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName)) return Result<User>.Failure(UserErrors.NameNotProvided);
            if (email is null) return Result<User>.Failure(UserErrors.EmailNotProvided);

            User user = new User()
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Tel = tel,
                IsActive = false
            };

            return Result<User>.Success(user);
        }

        public void SetActive(bool active) => IsActive = active;

        public Result AssignRole(int roleId)
        {
            if (_userRoles.Any(r => r.RoleId == roleId)) return Result.Failure(UserErrors.RoleAlreadyAssigned);

            _userRoles.Add(UserRole.Create(roleId));

            return Result.Success();
        }

        public Result RemoveRole(int roleId)
        {
            UserRole userRole = _userRoles.FirstOrDefault(r => r.RoleId == roleId);
            if (userRole is null) return Result.Failure(UserErrors.RoleNotAssigned);

            _userRoles.Remove(userRole);

            return Result.Success();
        }
    }
}
