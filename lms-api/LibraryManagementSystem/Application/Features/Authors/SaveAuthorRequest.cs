using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Authors
{
    public record SaveAuthorRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ICollection<string> PenNames { get; set; } = new List<string>();
    }
}
