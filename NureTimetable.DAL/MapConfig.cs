using AutoMapper;
using System;
using Cist = NureTimetable.DAL.Models.Cist;
using Local = NureTimetable.DAL.Models.Local;

namespace NureTimetable.DAL
{
    public static class MapConfig
    {
        private static IMapper _config;

        private static IMapper config
        {
            get
            {
                if (_config == null)
                {
                    Init();
                }
                return _config;
            }
        }

        public static TDestination Map<TSource, TDestination>(TSource source)
            => config.Map<TSource, TDestination>(source);

        private static void Init()
        {
            if (_config == null)
            {
                var localTimezone = TimeZoneInfo.Local;
                _config = new MapperConfiguration(cfg => {
                    // UniversityEntitiesRepository
                    cfg.CreateMap<Cist.Group, Local.Group>();
                    cfg.CreateMap<Cist.Faculty, Local.BaseEntity<long>>()
                        .ForMember("FullName", opt => opt.MapFrom(src => src.ShortName))
                        .ForMember("ShortName", opt => opt.MapFrom(src => src.FullName));
                    cfg.CreateMap<Cist.Direction, Local.BaseEntity<long>>()
                        .ForMember("FullName", opt => opt.MapFrom(src => src.ShortName))
                        .ForMember("ShortName", opt => opt.MapFrom(src => src.FullName));
                    cfg.CreateMap<Cist.Speciality, Local.BaseEntity<long>>();

                    cfg.CreateMap<Cist.Teacher, Local.Teacher>()
                        .ForMember("Name", opt => opt.MapFrom(src => src.FullName));
                    cfg.CreateMap<Cist.Department, Local.BaseEntity<long>>();

                    cfg.CreateMap<Cist.Room, Local.Room>()
                        .ForMember("Name", opt => opt.MapFrom(src => src.ShortName));
                    cfg.CreateMap<Cist.RoomType, Local.RoomType>();
                    cfg.CreateMap<Cist.Building, Local.BaseEntity<string>>();

                    // EventsRepository
                    cfg.CreateMap<Cist.Event, Local.Event>()
                        .ForMember("RoomName", opt => opt.MapFrom(src => src.Room))
                        .ForMember("Start", opt => opt.MapFrom(src => src.StartTime.Add(localTimezone.GetUtcOffset(src.StartTime))))
                        .ForMember("End", opt => opt.MapFrom(src => src.EndTime.Add(localTimezone.GetUtcOffset(src.EndTime))));
                    cfg.CreateMap<Cist.EventType, Local.EventType>();
                    cfg.CreateMap<Cist.Lesson, Local.Lesson>();
                }).CreateMapper();
            }
        }
    }
}
