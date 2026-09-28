using Application.Features.Authors;
using Application.Features.BookCopies;
using Application.Features.Books;
using Application.Features.Borrowings;
using Application.Features.Genres;
using Application.Features.Locations;
using Application.Features.Publishers;
using Application.Features.Users;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    public static class DependencyInjection
    {
        public static void AddApplication(this IHostApplicationBuilder builder)
        {
            builder.Services.AddAutoMapper(cfg => cfg.LicenseKey = null, typeof(DependencyInjection).Assembly);
            builder.Services.AddScoped<AuthorValidator>();
            builder.Services.AddScoped<SearchAuthorsValidator>();
            builder.Services.AddScoped<AuthorService>();
            builder.Services.AddScoped<PublisherValidator>();
            builder.Services.AddScoped<SearchPublishersValidator>();
            builder.Services.AddScoped<PublisherService>();
            builder.Services.AddScoped<GenreValidator>();
            builder.Services.AddScoped<SearchGenresValidator>();
            builder.Services.AddScoped<GenreService>();
            builder.Services.AddScoped<LocationValidator>();
            builder.Services.AddScoped<SearchLocationsValidator>();
            builder.Services.AddScoped<LocationService>();
            builder.Services.AddScoped<BookValidator>();
            builder.Services.AddScoped<SearchBooksValidator>();
            builder.Services.AddScoped<BookService>();
            builder.Services.AddScoped<CreateBookCopyValidator>();
            builder.Services.AddScoped<UpdateBookCopyValidator>();
            builder.Services.AddScoped<BookCopyService>();
            builder.Services.AddSingleton(TimeProvider.System);
            builder.Services.AddScoped<BorrowValidator>();
            builder.Services.AddScoped<AssignBorrowValidator>();
            builder.Services.AddScoped<SearchBorrowingsValidator>();
            builder.Services.AddScoped<BorrowingService>();
            builder.Services.AddScoped<RegisterValidator>();
            builder.Services.AddScoped<LoginValidator>();
            builder.Services.AddScoped<UpdateProfileValidator>();
            builder.Services.AddScoped<UpdateUserValidator>();
            builder.Services.AddScoped<SearchUsersValidator>();
            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            builder.Services.AddScoped<UserService>();
        }
    }
}
