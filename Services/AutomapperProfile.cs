using AutoMapper;
using Data.Entities;
using ServiceContracts.DTO;

namespace Services
{
    public class AutomapperProfile : Profile
    {
        public AutomapperProfile() 
        {
            CreateMap<CategoryAddRequest, CategoryEntity>();

            CreateMap<CategoryUpdateRequest, CategoryEntity>()
                .ForMember(dest => dest.UserId, src => src.Ignore());

            CreateMap<CategoryEntity, CategoryResponse>();

            CreateMap<TaskAddRequest, TaskEntity>();

            CreateMap<TaskUpdateRequest, TaskEntity>()
                .ForMember(dest => dest.DateCreated, src => src.Ignore())
                .ForMember(dest => dest.UserId, src => src.Ignore());

            CreateMap<TaskEntity, TaskResponse>();

            CreateMap<UserAddRequest, UserEntity>()
                .ForMember(dest => dest.PasswordHash, src => src.Ignore());
        }
    }
}
