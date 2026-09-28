using Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Publishers
{
    public record SavePublisherRequest
    {
        public string Name { get; set; }
        public Address Address { get; set; }
        public string Tel { get; set; }
    }
}
