using Domain.Errors;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Publishers
{
    public class PublisherValidator : AbstractValidator<SavePublisherRequest>
    {
        public PublisherValidator()
        {
            RuleFor(pub => pub.Name)
                .NotEmpty().WithErrorCode(PublisherErrors.NameNotProvided.Code).WithMessage(PublisherErrors.NameNotProvided.Description)
                .MaximumLength(200).WithErrorCode("Publisher.NameTooLong").WithMessage("Publisher name must not exceed 200 characters.");

            RuleFor(pub => pub.Address)
                .NotNull().WithErrorCode(PublisherErrors.AddressNotProvided.Code).WithMessage(PublisherErrors.AddressNotProvided.Description);

            When(pub => pub.Address is not null, () =>
            {
                RuleFor(pub => pub.Address.Building)
                    .MaximumLength(100).WithErrorCode("Address.BuildingTooLong").WithMessage("Building must not exceed 100 characters.");

                RuleFor(pub => pub.Address.Street)
                    .NotEmpty().WithErrorCode(AddressErrors.StreetNotProvided.Code).WithMessage(AddressErrors.StreetNotProvided.Description)
                    .MaximumLength(100).WithErrorCode("Address.StreetTooLong").WithMessage("Street must not exceed 100 characters.");

                RuleFor(pub => pub.Address.City)
                    .NotEmpty().WithErrorCode(AddressErrors.CityNotProvided.Code).WithMessage(AddressErrors.CityNotProvided.Description)
                    .MaximumLength(100).WithErrorCode("Address.CityTooLong").WithMessage("City must not exceed 100 characters.");

                RuleFor(pub => pub.Address.State)
                    .MaximumLength(100).WithErrorCode("Address.StateTooLong").WithMessage("State must not exceed 100 characters.");

                RuleFor(pub => pub.Address.ZipCode)
                    .MaximumLength(10).WithErrorCode("Address.ZipCodeTooLong").WithMessage("Zip code must not exceed 10 characters.");

                RuleFor(pub => pub.Address.Country)
                    .NotEmpty().WithErrorCode(AddressErrors.CountryNotProvided.Code).WithMessage(AddressErrors.CountryNotProvided.Description)
                    .MaximumLength(50).WithErrorCode("Address.CountryTooLong").WithMessage("Country must not exceed 50 characters.");
            });

            RuleFor(pub => pub.Tel)
                .MaximumLength(15).WithErrorCode("Publisher.TelTooLong").WithMessage("Tel must not exceed 15 characters.")
                .Matches(@"^\+?[0-9\- ]+$").WithErrorCode("Publisher.TelInvalid").WithMessage("Tel may only contain digits, spaces, '-' and a leading '+'.")
                .When(pub => !string.IsNullOrEmpty(pub.Tel));
        }
    }
}
