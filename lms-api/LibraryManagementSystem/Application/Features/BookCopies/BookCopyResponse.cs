using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.BookCopies
{
    public record BookCopyResponse
    {
        public Guid Id { get; set; }
        public long BookId { get; set; }
        public int LocationId { get; set; }
        public BookCopyStatus AvailableStatus { get; set; }
    }
}
