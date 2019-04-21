using AutoMapper;
using System;
using Cist = NureTimetable.DAL.Models.Cist;
using Local = NureTimetable.DAL.Models.Local;

namespace NureTimetable.DAL
{
    public static class DALBase
    {
        public static void Init()
        {
            // Yeah static constructor is bad, but I need to initialize AutoMapper somewhere

            var localTimezone = TimeZoneInfo.Local;
            Mapper.Initialize(cfg => {
                // UniversityEntitiesRepository
                cfg.CreateMap<Cist.Group, Local.Group>();
                cfg.CreateMap<Cist.Faculty, Local.BaseEntity<long>>()
                    .ForMember("FullName", opt => opt.MapFrom(src => src.ShortName))
                    .ForMember("ShortName", opt => opt.MapFrom(src => src.FullName));
                cfg.CreateMap<Cist.Direction, Local.BaseEntity<long>>()
                    .ForMember("FullName", opt => opt.MapFrom(src => src.ShortName))
                    .ForMember("ShortName", opt => opt.MapFrom(src => src.FullName));
                cfg.CreateMap<Cist.Speciality, Local.BaseEntity<long>>();

                cfg.CreateMap<Cist.Teacher, Local.Teacher>();
                cfg.CreateMap<Cist.Department, Local.BaseEntity<long>>();

                cfg.CreateMap<Cist.Room, Local.Room>();
                cfg.CreateMap<Cist.RoomType, Local.RoomType>();
                cfg.CreateMap<Cist.Building, Local.BaseEntity<string>>();

                // EventsRepository
                cfg.CreateMap<Cist.Event, Local.Event>()
                    .ForMember("RoomName", opt => opt.MapFrom(src => src.Room))
                    .ForMember("Start", opt => opt.MapFrom(src => src.StartTime.Add(localTimezone.GetUtcOffset(src.StartTime))))
                    .ForMember("End", opt => opt.MapFrom(src => src.EndTime.Add(localTimezone.GetUtcOffset(src.EndTime))));
                cfg.CreateMap<Cist.EventType, Local.EventType>();
                cfg.CreateMap<Cist.Lesson, Local.Lesson>();
            });
        }
    }
}
