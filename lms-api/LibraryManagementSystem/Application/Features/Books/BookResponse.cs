using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Books
{
    public record BookResponse
    {
        public long Id { get; set; }
        public string Isbn { get; set; }
        public string Title { get; set; }
        public int PublisherId { get; set; }
        public short PublicationYear { get; set; }
        public bool IsActive { get; set; }
        public List<int> AuthorIds { get; set; }
        public List<int> GenreIds { get; set; }
        // Filled by the service. Client uses it to decide whether the book can be borrowed.
        public int AvailableCopyCount { get; set; }
    }
}
