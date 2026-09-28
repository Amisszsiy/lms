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
    public class BookCopyConfiguration : IEntityTypeConfiguration<BookCopy>
    {
        public const string RowVersion = "RowVersion";

        public void Configure(EntityTypeBuilder<BookCopy> builder)
        {
            builder.ToTable("BookCopy");

            builder.HasKey(bc => bc.Id);

            // Stored as int: 1.Available 2.Borrowed 3.Damaged 4.Lost 5.Withdrawn 6.IssueReturn
            builder.Property(bc => bc.AvailableStatus)
                .HasConversion<int>()
                .IsRequired();

            builder.Ignore(bc => bc.IsOnLoan);

            // Optimistic concurrency so two borrows of the same copy can't both succeed.
            builder.Property<byte[]>(RowVersion)
                .IsRowVersion()
                .IsRequired();

            builder.HasOne(bc => bc.Book)
                .WithMany()
                .HasForeignKey(bc => bc.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Location>()
                .WithMany()
                .HasForeignKey(bc => bc.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(bc => bc.BookId);

            builder.ConfigureAudit();
        }
    }
}
