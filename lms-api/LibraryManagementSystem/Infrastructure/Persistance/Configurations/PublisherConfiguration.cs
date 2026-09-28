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
    public class PublisherConfiguration : IEntityTypeConfiguration<Publisher>
    {
        public void Configure(EntityTypeBuilder<Publisher> builder)
        {
            builder.ToTable("Publisher");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .HasMaxLength(200)
                .IsRequired();

            // Address value object is flattened into the Publisher table.
            builder.ComplexProperty(p => p.Address, address =>
            {
                address.IsRequired();

                address.Property(a => a.Building)
                    .HasColumnName("Building")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                address.Property(a => a.Street)
                    .HasColumnName("Street")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                address.Property(a => a.City)
                    .HasColumnName("City")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                address.Property(a => a.State)
                    .HasColumnName("State")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                address.Property(a => a.ZipCode)
                    .HasColumnName("ZipCode")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                address.Property(a => a.Country)
                    .HasColumnName("Country")
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            builder.Property(p => p.Tel)
                .HasMaxLength(15)
                .IsUnicode(false);

            builder.ConfigureAudit();
        }
    }
}
