using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Configurations
{
    public class BookConfiguration : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> builder)
        {
            builder.ToTable("Book");

            builder.HasKey(b => b.Id);

            // Stored ISBN was already validated, so rebuilding it from the column can't fail.
            builder.Property(b => b.Isbn)
                .HasColumnName("ISBN")
                .HasConversion(isbn => isbn.Value, value => Isbn.Create(value).Value)
                .HasMaxLength(Isbn.Length)
                .IsFixedLength()
                .IsUnicode(false)
                .IsRequired();

            builder.HasIndex(b => b.Isbn).IsUnique();

            builder.Property(b => b.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasIndex(b => b.Title);

            builder.Property(b => b.PublicationYear).IsRequired();

            builder.Property(b => b.IsActive).IsRequired();

            builder.HasOne<Publisher>()
                .WithMany()
                .HasForeignKey(b => b.PublisherId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(b => b.BookAuthors)
                .WithOne()
                .HasForeignKey(ba => ba.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(b => b.BookGenres)
                .WithOne()
                .HasForeignKey(bg => bg.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(b => b.BookAuthors).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(b => b.BookGenres).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.ConfigureAudit();
        }
    }
}
