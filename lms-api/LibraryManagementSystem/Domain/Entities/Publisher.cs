using Domain.Common;
using Domain.Errors;
using Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Publisher : BaseEntity<int>, IAuditable
    {
        public string Name { get; set; }
        public Address Address { get; set; }
        public string Tel { get; set; }

        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public static Result<Publisher> Create(string name, Address address, string tel)
        {
            if (string.IsNullOrWhiteSpace(name)) return Result<Publisher>.Failure(PublisherErrors.NameNotProvided);
            if (address is null) return Result<Publisher>.Failure(PublisherErrors.AddressNotProvided);

            Publisher publisher = new Publisher()
            {
                Name = name,
                Address = address,
                Tel = tel
            };

            return Result<Publisher>.Success(publisher);
        }
    }
}
