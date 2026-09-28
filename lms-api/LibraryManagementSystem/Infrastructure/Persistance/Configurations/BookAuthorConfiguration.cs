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
    public class BookAuthorConfiguration : IEntityTypeConfiguration<BookAuthor>
    {
        public void Configure(EntityTypeBuilder<BookAuthor> builder)
        {
            builder.ToTable("BookAuthor");

            builder.HasKey(ba => ba.Id);

            // Book relationship is configured from BookConfiguration.

            builder.HasOne<Author>()
                .WithMany()
                .HasForeignKey(ba => ba.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(ba => new { ba.BookId, ba.AuthorId }).IsUnique();

            builder.ConfigureAudit();
        }
    }
}
