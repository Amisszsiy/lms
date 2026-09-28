using Application.Common;
using Application.Features.Users;
using Domain.Common;
using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Seed
{
    // Only runs when the database doesn't exist yet.
    public class LibraryDbSeeder
    {
        private const string AdminUsername = "admin";
        private const string AdminPassword = "p@ssw0rd";
        private const string AdminEmail = "mathus.songdechagraiwud@gmail.com";
        private const string AdminTel = "0943359420";

        private static readonly string[] Areas = { "A", "B" };
        private const int LinesPerArea = 5;
        private const int ShelvesPerLine = 4;
        private const int RowsPerShelf = 5;
        private const int ColumnsPerShelf = 3;

        private readonly LibraryDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public LibraryDbSeeder(LibraryDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            IRelationalDatabaseCreator databaseCreator = _context.Database.GetService<IRelationalDatabaseCreator>();
            if (await databaseCreator.ExistsAsync(cancellationToken)) return;

            await _context.Database.MigrateAsync(cancellationToken);

            try
            {
                await SeedAsync(cancellationToken);
            }
            catch
            {
                // If seed error, ensure no half seed state
                await _context.Database.EnsureDeletedAsync(CancellationToken.None);
                throw;
            }
        }

        private async Task SeedAsync(CancellationToken cancellationToken)
        {
            await SeedRoles(cancellationToken);
            await SeedAdmin(cancellationToken);
            await SeedGenres(cancellationToken);
            await SeedLocations(cancellationToken);
            await SeedAuthors(cancellationToken);
            await SeedPublishers(cancellationToken);
        }

        private async Task SeedRoles(CancellationToken cancellationToken)
        {
            _context.Roles.Add(GetResult(Role.Create(RoleNames.Admin)));
            _context.Roles.Add(GetResult(Role.Create(RoleNames.NormalUser)));

            // Saved on its own so the admin can be assigned the generated role id.
            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task SeedAdmin(CancellationToken cancellationToken)
        {
            int adminRoleId = await _context.Roles.Where(r => r.RoleName == RoleNames.Admin).Select(r => r.Id).SingleAsync(cancellationToken);

            User admin = GetResult(User.Create("System", "Administrator", GetResult(EmailAddress.Create(AdminEmail)), AdminTel));
            admin.SetActive(true);
            GetResult(admin.AssignRole(adminRoleId));

            Credential credential = GetResult(Credential.Create(AdminUsername, _passwordHasher.HashPassword(admin, AdminPassword), admin));

            _context.Users.Add(admin);
            _context.Credentials.Add(credential);
            await _context.SaveChangesAsync(cancellationToken);

            // Everything seeded after this point is audited as created by the admin.
            _context.SetAuditUser(admin.Id);
        }

        private async Task SeedGenres(CancellationToken cancellationToken)
        {
            string[] genres =
            {
                "Fiction", "Non-Fiction", "Science Fiction", "Fantasy", "Mystery",
                "Romance", "Biography", "History", "Science", "Children"
            };

            _context.Genres.AddRange(genres.Select(g => GetResult(Genre.Create(g))));
            await _context.SaveChangesAsync(cancellationToken);
        }

        // 2 areas x 5 lines x 4 shelves x (5 rows x 3 columns) = 600 locations.
        private async Task SeedLocations(CancellationToken cancellationToken)
        {
            foreach (string area in Areas)
            for (int line = 1; line <= LinesPerArea; line++)
            for (int shelf = 1; shelf <= ShelvesPerLine; shelf++)
            for (int row = 1; row <= RowsPerShelf; row++)
            for (int column = 1; column <= ColumnsPerShelf; column++)
            {
                _context.Locations.Add(GetResult(Location.Create(area, line.ToString(), shelf.ToString(), row, column)));
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task SeedAuthors(CancellationToken cancellationToken)
        {
            var authors = new (string FirstName, string LastName, string[] PenNames)[]
            {
                ("Eric", "Blair", new[] { "George Orwell" }),
                ("Samuel", "Clemens", new[] { "Mark Twain", "Sieur Louis de Conte" }),
                ("Haruki", "Murakami", Array.Empty<string>()),
                ("Mary Ann", "Evans", new[] { "George Eliot" }),
                ("Agatha", "Christie", new[] { "Mary Westmacott" })
            };

            foreach (var (firstName, lastName, penNames) in authors)
            {
                Author author = GetResult(Author.Create(firstName, lastName));
                foreach (string penName in penNames)
                {
                    GetResult(author.AddPenName(penName));
                }

                _context.Authors.Add(author);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task SeedPublishers(CancellationToken cancellationToken)
        {
            Publisher[] publishers =
            {
                CreatePublisher("Penguin Random House", "1745 Broadway", "New York", "NY", "10019", "USA", "+12127829000"),
                CreatePublisher("HarperCollins", "195 Broadway", "New York", "NY", "10007", "USA", "+12122077000"),
                CreatePublisher("Macmillan Publishers", "120 Broadway", "New York", "NY", "10271", "USA", "+16463075151"),
                CreatePublisher("Bloomsbury Publishing", "50 Bedford Square", "London", null, "WC1B 3DP", "United Kingdom", "+442076315600"),
                CreatePublisher("Nanmeebooks", "11 Soi Lat Phrao 32", "Bangkok", null, "10900", "Thailand", "026626000")
            };

            _context.Publishers.AddRange(publishers);
            await _context.SaveChangesAsync(cancellationToken);
        }

        private static Publisher CreatePublisher(string name, string street, string city, string state, string zipCode, string country, string tel)
        {
            Address address = GetResult(Address.Create(null, street, city, state, zipCode, country));
            return GetResult(Publisher.Create(name, address, tel));
        }

        private static T GetResult<T>(Result<T> result) =>
            result.IsSuccess ? result.Value : throw new InvalidOperationException($"Seeding failed: {result.Error.Code} - {result.Error.Description}");

        private static void GetResult(Result result)
        {
            if (result.IsFailure) throw new InvalidOperationException($"Seeding failed: {result.Error.Code} - {result.Error.Description}");
        }
    }
}
