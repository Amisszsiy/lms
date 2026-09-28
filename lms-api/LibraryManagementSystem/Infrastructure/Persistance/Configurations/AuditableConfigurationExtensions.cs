using Domain.Common;
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
    public static class AuditableConfigurationExtensions
    {
        public static void ConfigureAudit<T>(this EntityTypeBuilder<T> builder) where T : class, IAuditable
        {
            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(nameof(IAuditable.CreatedBy))
                .OnDelete(DeleteBehavior.Restrict); //deleting a user never cascades into audited rows

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(nameof(IAuditable.ModifiedBy))
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
