using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class BookGenre : BaseEntity<int>, IAuditable
    {
        public long BookId { get; private set; }
        public int GenreId { get; private set; }

        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        internal static BookGenre Create(int genreId) => new BookGenre() { GenreId = genreId };
    }
}
