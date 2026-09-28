using Application.Errors;
using Domain.Common;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users
{
    public class Credential : BaseEntity<Guid>
    {
        public const int UsernameMaxLength = 100;

        public Guid UserId { get; private set; }
        public string Username { get; private set; }
        public string PasswordHash { get; private set; }
        public User User { get; private set; }

        // Takes the user itself so the FK is fixed up when both are saved in the same commit.
        public static Result<Credential> Create(string username, string passwordHash, User user)
        {
            if (string.IsNullOrWhiteSpace(username)) return Result<Credential>.Failure(CredentialErrors.UsernameNotProvided);
            if (string.IsNullOrWhiteSpace(passwordHash)) return Result<Credential>.Failure(CredentialErrors.PasswordNotProvided);
            if (user is null) return Result<Credential>.Failure(CredentialErrors.UserNotProvided);

            Credential credential = new Credential()
            {
                Username = username.Trim(),
                PasswordHash = passwordHash,
                User = user,
                UserId = user.Id
            };

            return Result<Credential>.Success(credential);
        }

        public Result ChangePassword(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash)) return Result.Failure(CredentialErrors.PasswordNotProvided);

            PasswordHash = passwordHash;

            return Result.Success();
        }
    }
}
