using Application.Persistance;
using Infrastructure.Persistance;
using Infrastructure.Persistance.Repositories;
using Infrastructure.Persistance.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static void AddInfrastructure(this IHostApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("LibraryDb");
            builder.Services.AddDbContext<LibraryDbContext>(options => options.UseSqlServer(connectionString));

            builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
            builder.Services.AddScoped<IPublisherRepository, PublisherRepository>();
            builder.Services.AddScoped<IGenreRepository, GenreRepository>();
            builder.Services.AddScoped<ILocationRepository, LocationRepository>();
            builder.Services.AddScoped<IBookRepository, BookRepository>();
            builder.Services.AddScoped<IBookCopyRepository, BookCopyRepository>();
            builder.Services.AddScoped<IBorrowingRepository, BorrowingRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<ICredentialRepository, CredentialRepository>();

            builder.Services.AddScoped<LibraryDbSeeder>();
        }

        // Creates, migrates and seeds the database only when it doesn't exist yet.
        public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
        {
            using IServiceScope scope = services.CreateScope();
            LibraryDbSeeder seeder = scope.ServiceProvider.GetRequiredService<LibraryDbSeeder>();
            await seeder.InitializeAsync(cancellationToken);
        }
    }
}
