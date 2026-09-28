using Application.Features.Users;
using Application.Services;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance
{
    public class LibraryDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;
        private Guid? _auditUserOverride;

        public LibraryDbContext(DbContextOptions<LibraryDbContext> options, ICurrentUserService currentUserService) : base(options)
        {
            _currentUserService = currentUserService;
        }

        //DbSets
        public DbSet<Author> Authors { get; set; }
        public DbSet<PenName> PenNames { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookAuthor> BookAuthors { get; set; }
        public DbSet<BookGenre> BookGenres { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }
        public DbSet<Borrowing> Borrowings { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Credential> Credentials { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(LibraryDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        // For saves outside an HTTP request (e.g. seeding), where there is no current user.
        internal void SetAuditUser(Guid userId) => _auditUserOverride = userId;

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            OnBeforeSaveChanges();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var changes = ChangeTracker.Entries().Where(c => c.State == EntityState.Added || c.State == EntityState.Modified);
            Guid? userId = _auditUserOverride ?? _currentUserService.UserId;

            foreach (var change in changes)
            {
                if (change.Entity is IAuditable entity)
                {
                    if (change.State == EntityState.Added)
                    {
                        entity.CreatedDate = DateTime.UtcNow;
                        entity.CreatedBy = userId;
                    }

                    if (change.State == EntityState.Modified)
                    {
                        entity.ModifiedDate = DateTime.UtcNow;
                        entity.ModifiedBy = userId;
                    }
                }
            }
        }
    }
}
