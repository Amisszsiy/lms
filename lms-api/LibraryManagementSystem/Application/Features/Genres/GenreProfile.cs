using AutoMapper;
using Domain.Entities;

namespace Application.Features.Genres
{
    public class GenreProfile : Profile
    {
        public GenreProfile()
        {
            CreateMap<Genre, GenreResponse>();
        }
    }
}
