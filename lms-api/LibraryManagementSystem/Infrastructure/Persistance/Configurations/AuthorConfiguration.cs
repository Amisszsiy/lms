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
    public class AuthorConfiguration : IEntityTypeConfiguration<Author>
    {
        public void Configure(EntityTypeBuilder<Author> builder)
        {
            builder.ToTable("Author");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(a => a.LastName)
                .HasMaxLength(100);

            builder.HasIndex(a => new { a.FirstName, a.LastName });

            // Pen names are part of the Author aggregate, backed by the _penNames field.
            builder.HasMany(a => a.PenNames)
                .WithOne()
                .HasForeignKey(p => p.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Bypass setter
            builder.Navigation(a => a.PenNames).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.ConfigureAudit();
        }
    }
}
