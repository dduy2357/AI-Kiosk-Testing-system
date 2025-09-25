using API.Models;
using API.ViewModels;
using API.ViewModels.Token;
using AutoMapper;

namespace API.Configurations
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mapping
            CreateMap<User, UserVM>();
            CreateMap<User, UserListVM>();
            CreateMap<User, UserToken>();
            CreateMap<User, UserLoginVM>();
            CreateMap<UserImportVM, CreateUserVM>().ForMember(dest => dest.Sex, opt => opt.MapFrom(src => src.Sex ?? 0));
            

            // Class mapping
            CreateMap<Class, ClassVM>();
            CreateMap<CreateUpdateClassVM, Class>();

            // Subject mapping
            CreateMap<Subject, SubjectVM>();
            CreateMap<CreateUpdateSubjectVM, Subject>();

            // DisabledKey mapping
            CreateMap<DisabledKey, DisabledKeyVM>();

            // ProhibitedApp mapping
            CreateMap<ProhibitedApp, ProhibitedAppVM>();

            // permission mapping
            CreateMap<Permission, PermissionVM>();
            CreateMap<Campus, CampusVM>();
            CreateMap<Department, DepartmentVM>();
            CreateMap<Position, PositionVM>();
            CreateMap<Major, MajorVM>();
            CreateMap<Specialization, SpecializationVM>();

            // ConfigUrl mapping
            CreateMap<ConfigUrl, ConfigUrlVM>();

        }
    }
}
