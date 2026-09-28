using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Configurations
{
    public class BookGenreConfiguration : IEntityTypeConfiguration<BookGenre>
    {
        public void Configure(EntityTypeBuilder<BookGenre> builder)
        {
            builder.ToTable("BookGenre");

            builder.HasKey(bg => bg.Id);

            // Book relationship is configured from BookConfiguration.

            builder.HasOne<Genre>()
                .WithMany()
                .HasForeignKey(bg => bg.GenreId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(bg => new { bg.BookId, bg.GenreId }).IsUnique();

            builder.ConfigureAudit();
        }
    }
}
