using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Borrowings
{
    public class BorrowingProfile : Profile
    {
        public BorrowingProfile()
        {
            // Only service control status. User and Book stay null unless their navigations are loaded.
            CreateMap<Borrowing, BorrowingResponse>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Book, opt => opt.MapFrom(src => src.BookCopy.Book))
                .ForMember(dest => dest.Status, opt => opt.Ignore());

            CreateMap<User, BorrowingUserResponse>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value));

            CreateMap<Book, BorrowingBookResponse>()
                .ForMember(dest => dest.Isbn, opt => opt.MapFrom(src => src.Isbn.Value));
        }
    }
}
