using AutoMapper;
using BlogDemo.Core.Entites;
using BlogDemo.Infrastructure.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogDemo.API.MyProfile
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<PostDto, Post>();                
            CreateMap<Post,PostDto>().ForMember(a=>a.updatetime,b=>b.MapFrom(c=>c.LastModified));

            CreateMap<PostAddDto, Post>();
            CreateMap<Post, PostAddDto>();

            CreateMap<PostUpdateDto, Post>();
            CreateMap<Post,PostUpdateDto>();

            CreateMap<PostImage, PostImageDto>();
            CreateMap<PostImageDto, PostImage>();
        }
    }
}
