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
using NureTimetable.Core.Extensions;
using Xamarin.Forms;
using Cist = NureTimetable.DAL.Models.Cist;
using Local = NureTimetable.DAL.Models.Local;
using System.Threading.Tasks;
using System.Net.Http;

namespace NureTimetable.DAL
{
    public static class UniversityEntitiesRepository
    {
        public static bool IsInitialized { get; private set; } = false;

        private static readonly object lockObject = new object();
        
        public class UniversityEntitiesCistUpdateResult
        {
            public UniversityEntitiesCistUpdateResult()
            { }

            public UniversityEntitiesCistUpdateResult(Exception groupsException, Exception teachersException, Exception roomsException)
            {
                this.GroupsException = groupsException;
                this.TeachersException = teachersException;
                this.RoomsException = roomsException;
            }

            public Exception GroupsException { get; set; }
            public Exception TeachersException { get; set; }
            public Exception RoomsException { get; set; }

            public bool IsAllSuccessful =>
                GroupsException is null && TeachersException is null && RoomsException is null;

            public bool IsAllFail =>
                GroupsException != null && TeachersException != null && RoomsException != null;

            public bool IsConnectionIssues =>
                GroupsException?.IsNoInternet() == true 
                || TeachersException?.IsNoInternet() == true 
                || RoomsException?.IsNoInternet() == true;

        }

        #region All Entities Cist

        #region Public
        public static void AssureInitialized()
        {
            lock (lockObject)
            {
                if (IsInitialized)
                {
                    return;
                }
                Singleton = Get();
            }
        }

        public static bool UpdateLocal()
        {
            Cist.University university = GetLocal();
            if (university == null)
            {
                return false;
            }
            Singleton = university;
            return true;
        }

        public static UniversityEntitiesCistUpdateResult UpdateFromCist()
        {
            Cist.University university = GetLocal();
            UniversityEntitiesCistUpdateResult result = UpdateFromCist(ref university);
            Singleton = university;
            return result;
        }
        #endregion

        #region Private
        private static Cist.University _singleton;
        private static Cist.University Singleton
        {
            get => _singleton;
            set
            {
                IsInitialized = true;
                _singleton = value;
            }
        }

        private static Cist.University Get()
        {
            Cist.University university = GetLocal();
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
            Cist.University university = null;
            UpdateFromCist(ref university);
            return university;
        }

        private static UniversityEntitiesCistUpdateResult UpdateFromCist(ref Cist.University university)
        {
            if (SettingsRepository.CheckCistAllEntitiesUpdateRights() == false)
            {
                return new UniversityEntitiesCistUpdateResult(null, null, null);
            }

            var groupsTask = GetAllGroupsFromCist();
            var teachersTask = GetAllTeachersFromCist();
            var roomsTask = GetAllRoomsFromCist();

            var result = new UniversityEntitiesCistUpdateResult();
            try
            {
                Task.WaitAll(groupsTask, teachersTask, roomsTask);
            }
            catch
            { 
                result = new UniversityEntitiesCistUpdateResult
                {
                    GroupsException = groupsTask.Exception?.InnerException,
                    TeachersException = teachersTask.Exception?.InnerException,
                    RoomsException = roomsTask.Exception?.InnerException
                };
            }

            university ??= new Cist.University();
            if (!roomsTask.IsFaulted)
            {
                university.Buildings = roomsTask.Result;
            }
            if (!groupsTask.IsFaulted)
            {
                university.Faculties = groupsTask.Result;
            }
            if (!teachersTask.IsFaulted)
            {
                foreach (Cist.Faculty faculty in teachersTask.Result)
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
            }

            if (!result.IsAllFail)
            {
                Serialisation.ToJsonFile(university, FilePath.UniversityEntities);
                if (result.IsAllSuccessful)
                {
                    SettingsRepository.UpdateCistAllEntitiesUpdateTime();
                }
                MessagingCenter.Send(Application.Current, MessageTypes.UniversityEntitiesUpdated);
            }

            return result;
        }

        private static async Task<List<Cist.Faculty>> GetAllGroupsFromCist()
        {
            using var client = new HttpClient();
            try
            {
                Uri uri = Urls.CistAllGroupsUrl;
                string responseStr = await client.GetStringAsync(uri);
                Cist.University newUniversity = Serialisation.FromJson<Cist.UniversityRootObject>(responseStr).University;

                return newUniversity.Faculties;
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);
                });
                throw;
            }
        }

        private static async Task<List<Cist.Faculty>> GetAllTeachersFromCist()
        {
            using var client = new HttpClient();
            try
            {
                Uri uri = Urls.CistAllTeachersUrl;
                string responseStr = await client.GetStringAsync(uri);
                Cist.University newUniversity;
                try
                {
                    newUniversity = Serialisation.FromJson<Cist.UniversityRootObject>(responseStr).University;
                }
                catch (JsonReaderException ex) when (ex.Message.StartsWith("JsonToken EndObject is not valid for closing JsonType Array. Path 'university.faculties'"))
                {
                    // Temporary workaround. Needs to be removed when fixed on Cist!
                    responseStr = responseStr.TrimEnd('\n');
                    responseStr = responseStr.Remove(responseStr.Length - 1) + "]}}";
                    newUniversity = Serialisation.FromJson<Cist.UniversityRootObject>(responseStr).University;
                }

                return newUniversity.Faculties;
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);
                });
                throw;
            }
        }

        private static async Task<List<Cist.Building>> GetAllRoomsFromCist()
        {
            using var client = new HttpClient();
            try
            {
                Uri uri = Urls.CistAllRoomsUrl;
                string responseStr = await client.GetStringAsync(uri);
                responseStr = responseStr.Replace("\n", "").Replace("[}]", "[]");
                Cist.University newUniversity = Serialisation.FromJson<Cist.UniversityRootObject>(responseStr).University;

                return newUniversity.Buildings;
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);
                });
                throw;
            }
        }
        #endregion

        #endregion

        #region All Entities Local
        public static IEnumerable<Local.Group> GetAllGroups()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException($"You MUST call {nameof(UniversityEntitiesRepository)}.AssureInitialized(); prior to using it.");
            }

            var groups = Singleton?.Faculties.SelectMany(fac => fac
                .Directions.SelectMany(dir =>
                    dir.Groups.Select(gr =>
                        {
                            Local.Group localGroup = MapConfig.Map<Cist.Group, Local.Group>(gr);
                            localGroup.Faculty = MapConfig.Map<Cist.Faculty, Local.BaseEntity<long>>(fac);
                            localGroup.Direction = MapConfig.Map<Cist.Direction, Local.BaseEntity<long>>(dir);
                            return localGroup;
                        })
                        .Concat(dir.Specialities.SelectMany(sp => sp.Groups.Select(gr =>
                        {
                            Local.Group localGroup = MapConfig.Map<Cist.Group, Local.Group>(gr);
                            localGroup.Faculty = MapConfig.Map<Cist.Faculty, Local.BaseEntity<long>>(fac);
                            localGroup.Direction = MapConfig.Map<Cist.Direction, Local.BaseEntity<long>>(dir);
                            localGroup.Speciality = MapConfig.Map<Cist.Speciality, Local.BaseEntity<long>>(sp);
                            return localGroup;
                        })))
                )
            ).Distinct();
            return groups;
        }

        public static IEnumerable<Local.Teacher> GetAllTeachers()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException($"You MUST call {nameof(UniversityEntitiesRepository)}.AssureInitialized(); prior to using it.");
            }

            var teachers = Singleton?.Faculties.SelectMany(fac => fac
                .Departments.SelectMany(dep =>
                    dep.Teachers.Select(tr =>
                    {
                        Local.Teacher localGroup = MapConfig.Map<Cist.Teacher, Local.Teacher>(tr);
                        localGroup.Department = MapConfig.Map<Cist.Department, Local.BaseEntity<long>>(dep);
                        return localGroup;
                    })
                )
            ).Distinct();
            return teachers;
        }

        public static IEnumerable<Local.Room> GetAllRooms()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException($"You MUST call {nameof(UniversityEntitiesRepository)}.AssureInitialized(); prior to using it.");
            }

            var rooms = Singleton?.Buildings.SelectMany(bd => bd
                .Rooms.Select(rm =>
                {
                    Local.Room localGroup = MapConfig.Map<Cist.Room, Local.Room>(rm);
                    localGroup.Building = MapConfig.Map<Cist.Building, Local.BaseEntity<string>>(bd);
                    return localGroup;
                })
            ).Distinct();
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
            savedEntities ??= new List<Local.SavedEntity>();

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
            List<Local.SavedEntity> selectedEntities = savedEntities.Intersect(GetSelected()).ToList();
            if (selectedEntities.Count == 0 && savedEntities.Count > 0)
            {
                UpdateSelected(savedEntities[0]);
            }
            else if(savedEntities.Count == 0)
            {
                UpdateSelected();
            }
        }
        #endregion

        #region Selected Entity
        public static List<Local.SavedEntity> GetSelected()
        {
            var selectedEntities = new List<Local.SavedEntity>();

            string filePath = FilePath.SelectedEntities;
            if (!File.Exists(filePath))
            {
                return selectedEntities;
            }

            selectedEntities = Serialisation.FromJsonFile<List<Local.SavedEntity>>(filePath) ?? selectedEntities;
            return selectedEntities;
        }

        public static void UpdateSelected(Local.SavedEntity selectedEntity = null)
        {
            List<Local.SavedEntity> entities = new List<Local.SavedEntity>();
            if (selectedEntity != null)
            {
                entities.Add(selectedEntity);
            }
            UpdateSelected(entities);
        }

        public static void UpdateSelected(List<Local.SavedEntity> selectedEntities)
        {
            selectedEntities ??= new List<Local.SavedEntity>();

            List<Local.SavedEntity> currentEntities = GetSelected();
            if (currentEntities.Count == selectedEntities.Count && !currentEntities.Except(selectedEntities).Any())
            {
                return;
            }

            Serialisation.ToJsonFile(selectedEntities, FilePath.SelectedEntities);
            Device.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send(Application.Current, MessageTypes.SelectedEntitiesChanged, selectedEntities);
            });
        }
        #endregion
    }
}
