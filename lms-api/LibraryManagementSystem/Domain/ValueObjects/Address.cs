using Domain.Common;
using Domain.Errors;

namespace Domain.ValueObjects
{
    public sealed record Address
    {
        private Address(string building, string street, string city, string state, string zipCode, string country)
        {
            Building = building;
            Street = street;
            City = city;
            State = state;
            ZipCode = zipCode;
            Country = country;
        }

        public string Building { get; }
        public string Street { get; }
        public string City { get; }
        public string State { get; }
        public string ZipCode { get; }
        public string Country { get; }

        // Building, State and ZipCode are optional.
        public static Result<Address> Create(string building, string street, string city, string state, string zipCode, string country)
        {
            if (string.IsNullOrWhiteSpace(street))
            {
                return Result<Address>.Failure(AddressErrors.StreetNotProvided);
            }

            if (string.IsNullOrWhiteSpace(city))
            {
                return Result<Address>.Failure(AddressErrors.CityNotProvided);
            }

            if (string.IsNullOrWhiteSpace(country))
            {
                return Result<Address>.Failure(AddressErrors.CountryNotProvided);
            }

            return Result<Address>.Success(new Address(
                Normalize(building),
                street.Trim(),
                city.Trim(),
                Normalize(state),
                Normalize(zipCode),
                country.Trim()));
        }

        private static string Normalize(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        public override string ToString() => 
            string.Join(", ", new[] { Building, Street, City, State, ZipCode, Country }.Where(v => !string.IsNullOrEmpty(v)));
    }
}
