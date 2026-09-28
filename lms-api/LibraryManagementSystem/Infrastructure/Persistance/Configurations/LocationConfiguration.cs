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
    public class LocationConfiguration : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> builder)
        {
            builder.ToTable("Location");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Area)
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(l => l.LineNo)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(l => l.ShelfNo)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(l => l.RowNo).IsRequired();

            builder.Property(l => l.ColumnNo).IsRequired();

            builder.HasIndex(l => new { l.Area, l.LineNo, l.ShelfNo, l.RowNo, l.ColumnNo }).IsUnique();

            builder.ConfigureAudit();
        }
    }
}
