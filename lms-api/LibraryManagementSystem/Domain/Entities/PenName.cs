using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class PenName : BaseEntity<int>, IAuditable
    {
        public string Name { get; private set; }
        public int AuthorId { get; private set; }

        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        internal static PenName Create(string name) => new PenName() { Name = name };
    }
}
