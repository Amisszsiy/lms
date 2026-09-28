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
    public class BorrowingConfiguration : IEntityTypeConfiguration<Borrowing>
    {
        public void Configure(EntityTypeBuilder<Borrowing> builder)
        {
            builder.ToTable("Borrowing");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.BorrowDate).IsRequired();

            builder.Property(b => b.DueDate).IsRequired();

            builder.Property(b => b.ReturnDate);

            builder.Ignore(b => b.IsReturned);

            builder.HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.BookCopy)
                .WithMany()
                .HasForeignKey(b => b.BookCopyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(b => new { b.UserId, b.BorrowDate });

            builder.HasIndex(b => b.BookCopyId, "IX_Borrowing_BookCopyId");

            // Final safety net. A copy can only have one open loan at a time.
            builder.HasIndex(b => b.BookCopyId, "UX_Borrowing_OpenLoan")
                .IsUnique()
                .HasFilter("[ReturnDate] IS NULL");

            builder.ConfigureAudit();
        }
    }
}
