using Domain.Common;

namespace Domain.Errors
{
    public static class AddressErrors
    {
        public static readonly Error StreetNotProvided = new Error("Address.StreetNotProvided", "Street is required.");

        public static readonly Error CityNotProvided = new Error("Address.CityNotProvided", "City is required.");

        public static readonly Error CountryNotProvided = new Error("Address.CountryNotProvided", "Country is required.");
    }
}
