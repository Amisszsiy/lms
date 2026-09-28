using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Books
{
    public class BookProfile : Profile
    {
        public BookProfile()
        {
            CreateMap<Book, BookResponse>()
                .ForMember(dest => dest.Isbn, opt => opt.MapFrom(src => src.Isbn.Value))
                .ForMember(dest => dest.AuthorIds, opt => opt.MapFrom(src => src.BookAuthors.Select(a => a.AuthorId)))
                .ForMember(dest => dest.GenreIds, opt => opt.MapFrom(src => src.BookGenres.Select(g => g.GenreId)))
                .ForMember(dest => dest.AvailableCopyCount, opt => opt.Ignore());
        }
    }
}
