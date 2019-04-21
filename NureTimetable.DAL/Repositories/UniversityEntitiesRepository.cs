using AutoMapper;
using Newtonsoft.Json;
using NureTimetable.Core.Models.Consts;
using NureTimetable.DAL.Helpers;
using NureTimetable.DAL.Models.Consts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Xamarin.Forms;
using Cist = NureTimetable.DAL.Models.Cist;
using Local = NureTimetable.DAL.Models.Local;

namespace NureTimetable.DAL
{
    public static class UniversityEntitiesRepository
    {
        public static bool IsLoaded => Singleton != null;
        
        #region All Entities Cist

        #region Public
        public static void Init()
        {
           Singleton = Get();
        }

        public static void UpdateLocal()
        {
            Singleton = GetLocal();
        }

        public static void UpdateFromCist()
        {
            Singleton = GetFromCist();
        }
        #endregion

        #region Private
        private static Cist.University Singleton;

        private static Cist.University Get()
        {
            Cist.University university;
            university = GetLocal();
            if (university != null)
            {
                return university;
            }
            university = GetFromCist();
            return university;
        }

        private static Cist.University GetLocal()
        {
            Cist.University loadedUniversity;

            string filePath = FilePath.UniversityEntities;
            if (!File.Exists(filePath))
            {
                return null;
            }
            
            loadedUniversity = Serialisation.FromJsonFile<Cist.University>(filePath);
            return loadedUniversity;
        }

        private static Cist.University GetFromCist()
        {
            if (SettingsRepository.CheckCistAllEntitiesUpdateRights() == false)
            {
                return null;
            }

            var university = new Cist.University();
            bool isGroupsOk = GetAllGroupsFromCist(ref university);
            bool isTeachersOk = GetAllTeachersFromCist(ref university);
            bool isRoomsOk = GetAllRoomsFromCist(ref university);

            Serialisation.ToJsonFile(university, FilePath.UniversityEntities);
            SettingsRepository.UpdateCistAllEntitiesUpdateTime();
            
            return university;
        }

        private static bool GetAllGroupsFromCist(ref Cist.University university)
        {
            if (university == null)
            {
                university = new Cist.University();
            }

            using (var client = new WebClient
            {
                Encoding = Encoding.GetEncoding("Windows-1251")
            })
            {
                try
                {
                    Uri uri = new Uri(Urls.CistAllGroupsUrl);
                    string responseStr = client.DownloadString(uri);
                    Cist.University newUniversity = JsonConvert.DeserializeObject<Cist.UniversityRootObject>(responseStr).University;

                    foreach (Cist.Faculty faculty in newUniversity.Faculties)
                    {
                        Cist.Faculty oldFaculty = university.Faculties.FirstOrDefault(f => f.Id == faculty.Id);
                        if (oldFaculty == null)
                        {
                            university.Faculties.Add(faculty);
                            continue;
                        }
                        faculty.Departments = oldFaculty.Departments;
                        university.Faculties.Remove(oldFaculty);
                        university.Faculties.Add(faculty);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);
                    });
                    return false;
                }
            }
        }

        private static bool GetAllTeachersFromCist(ref Cist.University university)
        {
            if (university == null)
            {
                university = new Cist.University();
            }
            
            using (var client = new WebClient
            {
                Encoding = Encoding.GetEncoding("Windows-1251")
            })
            {
                try
                {
                    Uri uri = new Uri(Urls.CistAllTeachersUrl);
                    string responseStr = client.DownloadString(uri);
                    Cist.University newUniversity = JsonConvert.DeserializeObject<Cist.UniversityRootObject>(responseStr).University;

                    foreach (Cist.Faculty faculty in newUniversity.Faculties)
                    {
                        Cist.Faculty oldFaculty = university.Faculties.FirstOrDefault(f => f.Id == faculty.Id);
                        if (oldFaculty == null)
                        {
                            university.Faculties.Add(faculty);
                            continue;
                        }
                        faculty.Directions = oldFaculty.Directions;
                        university.Faculties.Remove(oldFaculty);
                        university.Faculties.Add(faculty);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);
                    });
                    return false;
                }
            }
        }

        private static bool GetAllRoomsFromCist(ref Cist.University university)
        {
            if (university == null)
            {
                university = new Cist.University();
            }

            using (var client = new WebClient
            {
                Encoding = Encoding.GetEncoding("Windows-1251")
            })
            {
                try
                {
                    Uri uri = new Uri(Urls.CistAllRoomsUrl);
                    string responseStr = client.DownloadString(uri);
                    responseStr = responseStr.Replace("\n", "").Replace("[}]", "[]");
                    Cist.University newUniversity = JsonConvert.DeserializeObject<Cist.UniversityRootObject>(responseStr).University;

                    university.Buildings = newUniversity.Buildings;

                    return true;
                }
                catch (Exception ex)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);
                    });
                    return false;
                }
            }
        }
        #endregion

        #endregion

        #region All Entities Local
        public static IEnumerable<Local.Group> GetAllGroups()
        {
            if (!IsLoaded)
            {
                throw new InvalidOperationException($"You MUST call {nameof(UniversityEntitiesRepository)}.Init(); prior to using it.");
            }

            var groups = Singleton.Faculties.SelectMany(fac => fac
                .Directions.SelectMany(dir =>
                    dir.Groups.Select(gr =>
                    {
                        Local.Group localGroup = Mapper.Map<Cist.Group, Local.Group>(gr);
                        localGroup.Faculty = Mapper.Map<Cist.Faculty, Local.BaseEntity<long>>(fac);
                        localGroup.Direction = Mapper.Map<Cist.Direction, Local.BaseEntity<long>>(dir);
                        return localGroup;
                    })
                    .Concat(dir.Specialities.SelectMany(sp => sp.Groups.Select(gr =>
                    {
                        Local.Group localGroup = Mapper.Map<Cist.Group, Local.Group>(gr);
                        localGroup.Faculty = Mapper.Map<Cist.Faculty, Local.BaseEntity<long>>(fac);
                        localGroup.Direction = Mapper.Map<Cist.Direction, Local.BaseEntity<long>>(dir);
                        localGroup.Speciality = Mapper.Map<Cist.Speciality, Local.BaseEntity<long>>(sp);
                        return localGroup;
                    })))
                )
            );
            return groups;
        }

        public static IEnumerable<Local.Teacher> GetAllTeachers()
        {
            if (!IsLoaded)
            {
                throw new InvalidOperationException($"You MUST call {nameof(UniversityEntitiesRepository)}.Init(); prior to using it.");
            }

            var teachers = Singleton.Faculties.SelectMany(fac => fac
                .Departments.SelectMany(dep =>
                    dep.Teachers.Select(tr =>
                    {
                        Local.Teacher localGroup = Mapper.Map<Cist.Teacher, Local.Teacher>(tr);
                        localGroup.Department = Mapper.Map<Cist.Department, Local.BaseEntity<long>>(dep);
                        return localGroup;
                    })
                )
            );
            return teachers;
        }

        public static IEnumerable<Local.Room> GetAllRooms()
        {
            if (!IsLoaded)
            {
                throw new InvalidOperationException($"You MUST call {nameof(UniversityEntitiesRepository)}.Init(); prior to using it.");
            }

            var rooms = Singleton.Buildings.SelectMany(bd => bd
                .Rooms.Select(rm =>
                {
                    Local.Room localGroup = Mapper.Map<Cist.Room, Local.Room>(rm);
                    localGroup.Building = Mapper.Map<Cist.Building, Local.BaseEntity<string>>(bd);
                    return localGroup;
                })
            );
            return rooms;
        }
        #endregion

        #region Saved Entities
        public static List<Local.SavedEntity> GetSaved()
        {
            List<Local.SavedEntity> loadedEntities = new List<Local.SavedEntity>();

            string filePath = FilePath.SavedEntitiesList;
            if (!File.Exists(filePath))
            {
                return loadedEntities;
            }
            
            loadedEntities = Serialisation.FromJsonFile<List<Local.SavedEntity>>(filePath) ?? loadedEntities;
            return loadedEntities;
        }

        public static void UpdateSaved(List<Local.SavedEntity> savedEntities)
        {
            // Removing cache from deleted saved entities if needed
            List<Local.SavedEntity> deletedEntities = GetSaved()
                .Where(oldEntity => !savedEntities.Exists(entity => entity.ID == oldEntity.ID))
                .ToList();
            if (deletedEntities.Count > 0)
            {
                deletedEntities.ForEach((de) =>
                {
                    try
                    {
                        File.Delete(FilePath.SavedTimetable(de.Type, de.ID));
                    }
                    catch {}
                });
            }
            // Saving saved entities list
            Serialisation.ToJsonFile(savedEntities, FilePath.SavedEntitiesList);
            Device.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send(Application.Current, MessageTypes.SavedEntitiesChanged, savedEntities);
            });
            // Updating selected entity if needed
            Local.SavedEntity selectedEntity = GetSelected();
            if ((selectedEntity == null || !savedEntities.Exists(g => g.ID == selectedEntity.ID)) && savedEntities.Count > 0)
            {
                UpdateSelected(savedEntities[0]);
            }
            else if(savedEntities.Count == 0)
            {
                UpdateSelected(null);
            }
        }
        #endregion

        #region Selected Entity
        public static Local.SavedEntity GetSelected()
        {
            string filePath = FilePath.SelectedEntity;
            if (!File.Exists(filePath))
            {
                return null;
            }

            Local.SavedEntity selectedEntity = Serialisation.FromJsonFile<Local.SavedEntity>(filePath);
            return selectedEntity;
        }

        public static void UpdateSelected(Local.SavedEntity selectedEntity)
        {
            Serialisation.ToJsonFile(selectedEntity, FilePath.SelectedEntity);
            Device.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send(Application.Current, MessageTypes.SelectedEntityChanged, selectedEntity);
            });
        }
        #endregion
    }
}
