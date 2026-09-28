using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Authors
{
    public class AuthorProfile : Profile
    {
        public AuthorProfile()
        {
            CreateMap<Author, AuthorResponse>()
                .ForMember(dest => dest.PenNames, opt => opt.MapFrom(src => src.PenNames.Select(p => p.Name)));
        }
    }
}
