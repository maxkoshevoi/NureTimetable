using AutoMapper;
using Cist = NureTimetable.DAL.Models.Cist;
using Local = NureTimetable.DAL.Models.Local;

namespace NureTimetable.DAL
{
    public static class MapConfig
    {
        private static IMapper? _instance;
        private static readonly object configLockObj = new();

        private static IMapper Instance => _instance ?? Init();

        public static TDestination Map<TSource, TDestination>(TSource source) =>
            Instance.Map<TSource, TDestination>(source);

        private static IMapper Init()
        {
            lock (configLockObj)
            {
                _instance ??= new MapperConfiguration(cfg =>
                {
                    // UniversityEntitiesRepository
                    cfg.CreateMap<Cist::Group, Local::Group>();
                    cfg.CreateMap<Cist::Faculty, Local::BaseEntity<long>>()
                        .ForMember(f => f.FullName, opt => opt.MapFrom(src => src.ShortName))
                        .ForMember(f => f.ShortName, opt => opt.MapFrom(src => src.FullName));
                    cfg.CreateMap<Cist::Direction, Local::BaseEntity<long>>()
                        .ForMember(d => d.FullName, opt => opt.MapFrom(src => src.ShortName))
                        .ForMember(d => d.ShortName, opt => opt.MapFrom(src => src.FullName));
                    cfg.CreateMap<Cist::Speciality, Local::BaseEntity<long>>();

                    cfg.CreateMap<Cist::Teacher, Local::Teacher>()
                        .ForMember(t => t.Name, opt => opt.MapFrom(src => src.FullName));
                    cfg.CreateMap<Cist::Department, Local::BaseEntity<long>>();

                    cfg.CreateMap<Cist::Room, Local::Room>()
                        .ForMember(r => r.Name, opt => opt.MapFrom(src => src.ShortName));
                    cfg.CreateMap<Cist::RoomType, Local::RoomType>();
                    cfg.CreateMap<Cist::Building, Local::BaseEntity<string>>();

                    // EventsRepository
                    cfg.CreateMap<Cist::Event, Local::Event>()
                        .ForMember(e => e.RoomName, opt => opt.MapFrom(src => src.Room))
                        .ForMember(e => e.StartUtc, opt => opt.MapFrom(src => src.StartTime))
                        .ForMember(e => e.EndUtc, opt => opt.MapFrom(src => src.EndTime));
                    cfg.CreateMap<Cist::EventType, Local::EventType>();
                    cfg.CreateMap<Cist::Lesson, Local::Lesson>();
                }).CreateMapper();
            }

            return _instance;
        }
    }
}
