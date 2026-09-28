using Application.Features.Users;
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
    public class CredentialConfiguration : IEntityTypeConfiguration<Credential>
    {
        public void Configure(EntityTypeBuilder<Credential> builder)
        {
            builder.ToTable("UserCredential");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Username)
                .HasMaxLength(Credential.UsernameMaxLength)
                .IsUnicode(false)
                .IsRequired();

            builder.HasIndex(c => c.Username).IsUnique();

            builder.Property(c => c.PasswordHash)
                .HasMaxLength(200)
                .IsUnicode(false)
                .IsRequired();

            builder.HasOne(c => c.User)
                .WithOne()
                .HasForeignKey<Credential>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(c => c.UserId).IsUnique();
        }
    }
}
