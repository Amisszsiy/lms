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
    public class PenNameConfiguration : IEntityTypeConfiguration<PenName>
    {
        public void Configure(EntityTypeBuilder<PenName> builder)
        {
            builder.ToTable("PenName");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired();

            // Author relationship is configured from AuthorConfiguration.

            builder.ConfigureAudit();
        }
    }
}
