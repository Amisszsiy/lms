using AutoMapper;
using Domain.Entities;

namespace Application.Features.Locations
{
    public class LocationProfile : Profile
    {
        public LocationProfile()
        {
            CreateMap<Location, LocationResponse>();
        }
    }
}
