using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Books
{
    public record SaveBookRequest
    {
        public string Isbn { get; set; }
        public string Title { get; set; }
        public int PublisherId { get; set; }
        public short PublicationYear { get; set; }
        public List<int> AuthorIds { get; set; } = [];
        public List<int> GenreIds { get; set; } = [];
    }
}
