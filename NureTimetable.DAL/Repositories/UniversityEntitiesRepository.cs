using Microsoft.AppCenter.Analytics;
using NureTimetable.Core.Extensions;
using NureTimetable.Core.Models.Consts;
using NureTimetable.Core.Models.Exceptions;
using NureTimetable.DAL.Helpers;
using NureTimetable.DAL.Models.Consts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;
using Cist = NureTimetable.DAL.Models.Cist;
using Local = NureTimetable.DAL.Models.Local;

namespace NureTimetable.DAL
{
    public static class UniversityEntitiesRepository
    {
        public static bool IsInitialized { get; private set; } = false;

        private static readonly object lockObject = new();
        
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

            public Exception GroupsException { get; }
            public Exception TeachersException { get; }
            public Exception RoomsException { get; }

            public bool IsAllSuccessful =>
                GroupsException is null && TeachersException is null && RoomsException is null;

            public bool IsAllFail =>
                GroupsException is not null && TeachersException is not null && RoomsException is not null;

            public bool IsConnectionIssues =>
                GroupsException is WebException 
                || TeachersException is WebException 
                || RoomsException is WebException;

            public bool IsCistException =>
                GroupsException is CistException
                || TeachersException is CistException
                || RoomsException is CistException;
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
                Instance = Get();
            }
        }

        public static bool UpdateLocal()
        {
            Cist::University university = GetLocal();
            if (university is null)
            {
                return false;
            }
            Instance = university;
            return true;
        }

        public static UniversityEntitiesCistUpdateResult UpdateFromCist()
        {
            Cist::University university = GetLocal();
            UniversityEntitiesCistUpdateResult result = UpdateFromCist(ref university);
            Instance = university;
            return result;
        }
        #endregion

        #region Private
        private static Cist::University _instance;
        private static Cist::University Instance
        {
            get => _instance;
            set
            {
                IsInitialized = true;
                _instance = value;
            }
        }

        private static Cist::University Get()
        {
            Cist::University university = GetLocal();
            if (university != null)
            {
                return university;
            }
            university = GetFromCist();
            return university;
        }

        private static Cist::University GetLocal()
        {
            Cist::University loadedUniversity;

            string filePath = FilePath.UniversityEntities;
            if (!File.Exists(filePath))
            {
                return null;
            }
            
            loadedUniversity = Serialisation.FromJsonFile<Cist::University>(filePath);
            return loadedUniversity;
        }

        private static Cist::University GetFromCist()
        {
            Cist::University university = null;
            UpdateFromCist(ref university);
            return university;
        }

        private static UniversityEntitiesCistUpdateResult UpdateFromCist(ref Cist::University university)
        {
            if (SettingsRepository.CheckCistAllEntitiesUpdateRights() == false)
            {
                return new(null, null, null);
            }

            var groupsTask = TaskWithFallbacks(GetAllGroupsFromCist, GetAllGroupsFromCistHtml);
            var teachersTask = TaskWithFallbacks(GetAllTeachersFromCist, GetAllTeachersFromCistHtml);
            var roomsTask = GetAllRoomsFromCist();

            UniversityEntitiesCistUpdateResult result = new();
            try
            {
                Task.WaitAll(groupsTask, teachersTask, roomsTask);
            }
            catch
            {
                result = new
                (
                    groupsTask.Exception?.InnerException,
                    teachersTask.Exception?.InnerException,
                    roomsTask.Exception?.InnerException
                );
            }

            university ??= new();
            if (!roomsTask.IsFaulted)
            {
                university.Buildings = roomsTask.Result;
            }
            if (!groupsTask.IsFaulted)
            {
                foreach (var faculty in groupsTask.Result)
                {
                    Cist::Faculty oldFaculty = university.Faculties.FirstOrDefault(f => f.Id == faculty.Id);
                    if (oldFaculty != null)
                    {
                        faculty.Departments = oldFaculty.Departments;
                        university.Faculties.Remove(oldFaculty);
                    }
                    university.Faculties.Add(faculty);
                }
            }
            if (!teachersTask.IsFaulted)
            {
                foreach (var faculty in teachersTask.Result)
                {
                    Cist::Faculty oldFaculty = university.Faculties.FirstOrDefault(f => f.Id == faculty.Id);
                    if (oldFaculty != null)
                    {
                        faculty.Directions = oldFaculty.Directions;
                        university.Faculties.Remove(oldFaculty);
                    }
                    university.Faculties.Add(faculty);
                }
            }

            if (!result.IsAllFail)
            {
                Serialisation.ToJsonFile(university, FilePath.UniversityEntities);
                if (result.IsAllSuccessful)
                {
                    SettingsRepository.Settings.LastCistAllEntitiesUpdate = DateTime.Now;
                }

                Instance = university;
                MessagingCenter.Send(Application.Current, MessageTypes.UniversityEntitiesUpdated);
            }

            return result;
        }

        private static async Task<T> TaskWithFallbacks<T>(params Func<Task<T>>[] tasks)
        {
            if (tasks.Length == 0)
                throw new ArgumentException($"{nameof(tasks)} cannot be null or empty");

            int tasksLeft = tasks.Length;
            while (true)
            {
                try
                {
                    int currentTaskIndex = tasks.Length - tasksLeft;
                    tasksLeft--;
                    return await tasks[currentTaskIndex]();
                }
                catch (Exception ex)
                { 
                    MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);
                    if (tasksLeft == 0 || ex is WebException)
                    {
                        throw;
                    }
                }
            }
        }

        #region From Cist Api
        private static async Task<List<Cist::Faculty>> GetAllGroupsFromCist()
        {
            Analytics.TrackEvent("Cist request", new Dictionary<string, string>
            {
                { "Type", "GetAllGroups" },
                { "Hour of the day", DateTime.Now.Hour.ToString() }
            });

            using HttpClient client = new();
            string responseStr = await client.GetStringOrWebExceptionAsync(Urls.CistApiAllGroups);
            Cist::University newUniversity = CistHelper.FromJson<Cist::UniversityRootObject>(responseStr).University;

            return newUniversity.Faculties;
        }

        private static async Task<List<Cist::Faculty>> GetAllTeachersFromCist()
        {
            Analytics.TrackEvent("Cist request", new Dictionary<string, string>
            {
                { "Type", "GetAllTeachers" },
                { "Hour of the day", DateTime.Now.Hour.ToString() }
            });

            using HttpClient client = new();
            string responseStr = await client.GetStringOrWebExceptionAsync(Urls.CistApiAllTeachers);
            Cist::University newUniversity = CistHelper.FromJson<Cist::UniversityRootObject>(responseStr).University;

            return newUniversity.Faculties;
        }

        private static async Task<List<Cist::Building>> GetAllRoomsFromCist()
        {
            Analytics.TrackEvent("Cist request", new Dictionary<string, string>
            {
                { "Type", "GetAllRooms" },
                { "Hour of the day", DateTime.Now.Hour.ToString() }
            });

            using HttpClient client = new();
            string responseStr = await client.GetStringOrWebExceptionAsync(Urls.CistApiAllRooms);
            responseStr = responseStr.Replace("\n", "").Replace("[}]", "[]");
            Cist::University newUniversity = CistHelper.FromJson<Cist::UniversityRootObject>(responseStr).University;

            return newUniversity.Buildings;
        }
        #endregion

        #region From Cist Html
        private static async Task<List<Cist::Faculty>> GetAllGroupsFromCistHtml()
        {
            Analytics.TrackEvent("Cist request", new Dictionary<string, string>
            {
                { "Type", "GetAllGroupsHtml" },
                { "Hour of the day", DateTime.Now.Hour.ToString() }
            });

            List<Cist::Faculty> faculties = new();
            using HttpClient client = new();

            // Getting branches
            Uri uri = Urls.CistSiteAllGroups(null);
            string branchesListPage = await client.GetStringOrWebExceptionAsync(uri);
            foreach (string part in branchesListPage.Split(new[] { "IAS_Change_Groups(" }, StringSplitOptions.RemoveEmptyEntries).Skip(1))
            {
                string branchIdStr = part.Remove(part.IndexOf(')'));
                if (!int.TryParse(branchIdStr, out int facultyId))
                {
                    continue;
                }

                faculties.Add(new()
                {
                    Id = facultyId,
                    ShortName = part[(part.IndexOf('>') + 1)..part.IndexOf('<')]
                });
            }

            //Getting groups
            foreach (var faculty in faculties)
            {
                faculty.Directions = new List<Cist::Direction> { new Cist::Direction() };

                uri = Urls.CistSiteAllGroups(faculty.Id);
                string branchGroupsPage = await client.GetStringOrWebExceptionAsync(uri);
                foreach (string part in branchGroupsPage.Split(new[] { "IAS_ADD_Group_in_List(" }, StringSplitOptions.RemoveEmptyEntries).Skip(1))
                {
                    string[] groupInfo = part
                        .Remove(part.IndexOf(")\">"))
                        .Split(new[] { ',', '\'' }, StringSplitOptions.RemoveEmptyEntries);

                    if (groupInfo.Length != 2 || !int.TryParse(groupInfo[1], out int groupID))
                    {
                        continue;
                    }

                    string groupName = groupInfo[0];
                    faculty.Directions[0].Groups.Add(new()
                    {
                        Id = groupID,
                        Name = groupName
                    });
                }
            }

            return faculties;
        }

        private static async Task<List<Cist::Faculty>> GetAllTeachersFromCistHtml()
        {
            Analytics.TrackEvent("Cist request", new Dictionary<string, string>
            {
                { "Type", "GetAllTeachersHtml" },
                { "Hour of the day", DateTime.Now.Hour.ToString() }
            });

            List<Cist::Faculty> faculties = new();

            // Getting faculties
            using HttpClient client = new();
            Uri uri = Urls.CistSiteAllTeachers();
            string facultyListPage = await client.GetStringOrWebExceptionAsync(uri);
            foreach (string part in facultyListPage.Split(new[] { "IAS_Change_Kaf(" }, StringSplitOptions.RemoveEmptyEntries).Skip(1))
            {
                string facultyIdStr = part.Remove(part.IndexOf(','));
                if (!int.TryParse(facultyIdStr, out int facId))
                {
                    continue;
                }

                faculties.Add(new()
                {
                    Id = facId,
                    ShortName = part[(part.IndexOf('>') + 1)..part.IndexOf('<')],
                    Departments = await GetDepartmentsForFaculty(facId)
                });
            }

            return faculties;
        }

        private static async Task<List<Cist::Department>> GetDepartmentsForFaculty(long facultyId)
        {
            List<Cist::Department> departments = new();

            // Getting departments
            using HttpClient client = new();
            Uri uri = Urls.CistSiteAllTeachers(facultyId);
            string facultyPage = await client.GetStringOrWebExceptionAsync(uri);

            foreach (string part in facultyPage.Split(new[] { $"IAS_Change_Kaf({facultyId}," }, StringSplitOptions.RemoveEmptyEntries).Skip(1))
            {
                string departmentIdStr = part.Remove(part.IndexOf(')'));
                if (!int.TryParse(departmentIdStr, out int depId) || depId == -1)
                {
                    continue;
                }

                departments.Add(new()
                {
                    Id = depId,
                    ShortName = part[(part.IndexOf('>') + 1)..part.IndexOf('<')],
                    Teachers = await GetTeachersForDepartment(facultyId, depId)
                });
            }

            return departments;
        }

        private static async Task<List<Cist::Teacher>> GetTeachersForDepartment(long facultyId, long departmentId)
        {
            List<Cist::Teacher> teachers = new();

            // Getting teachers
            Uri uri = Urls.CistSiteAllTeachers(facultyId, departmentId);
            using HttpClient client = new();
            string branchGroupsPage = await client.GetStringOrWebExceptionAsync(uri);
            foreach (string part in branchGroupsPage.Split(new[] { "IAS_ADD_Teach_in_List(" }, StringSplitOptions.RemoveEmptyEntries).Skip(1))
            {
                string[] teacherInfo = part
                    .Remove(part.IndexOf(")\">"))
                    .Split(new[] { ',', '\'' }, StringSplitOptions.RemoveEmptyEntries);

                if (teacherInfo.Length != 2 || !int.TryParse(teacherInfo[1], out int teacherID))
                {
                    continue;
                }

                string teacherName = teacherInfo[0];
                teachers.Add(new()
                {
                    Id = teacherID,
                    ShortName = teacherName,
                    FullName = teacherName
                });
            }

            return teachers;
        }
        #endregion
        #endregion

        #endregion

        #region All Entities Local
        public static IEnumerable<Local::Group> GetAllGroups()
        {
            if (!IsInitialized)
                throw new InvalidOperationException($"You MUST call {nameof(UniversityEntitiesRepository)}.AssureInitialized(); prior to using it.");

            var groups = Instance?.Faculties
                .SelectMany(fac => 
                    fac.Directions.SelectMany(dir =>
                        dir.Groups.Select(gr =>
                        {
                            Local::Group localGroup = MapConfig.Map<Cist::Group, Local::Group>(gr);
                            localGroup.Faculty = MapConfig.Map<Cist::Faculty, Local::BaseEntity<long>>(fac);
                            localGroup.Direction = MapConfig.Map<Cist::Direction, Local::BaseEntity<long>>(dir);
                            return localGroup;
                        })
                        .Concat(dir.Specialities.SelectMany(sp => sp.Groups.Select(gr =>
                        {
                            Local::Group localGroup = MapConfig.Map<Cist::Group, Local::Group>(gr);
                            localGroup.Faculty = MapConfig.Map<Cist::Faculty, Local::BaseEntity<long>>(fac);
                            localGroup.Direction = MapConfig.Map<Cist::Direction, Local::BaseEntity<long>>(dir);
                            localGroup.Speciality = MapConfig.Map<Cist::Speciality, Local::BaseEntity<long>>(sp);
                            return localGroup;
                        })))
                    )
                ).Distinct();
            return groups;
        }

        public static IEnumerable<Local::Teacher> GetAllTeachers()
        {
            if (!IsInitialized)
                throw new InvalidOperationException($"You MUST call {nameof(UniversityEntitiesRepository)}.AssureInitialized(); prior to using it.");

            var teachers = Instance?.Faculties
                .SelectMany(fac => 
                    fac.Departments.SelectMany(dep =>
                        dep.Teachers.Select(tr =>
                        {
                            Local::Teacher localGroup = MapConfig.Map<Cist::Teacher, Local::Teacher>(tr);
                            localGroup.Department = MapConfig.Map<Cist::Department, Local::BaseEntity<long>>(dep);
                            return localGroup;
                        })
                    )
                ).Distinct();
            return teachers;
        }

        public static IEnumerable<Local::Room> GetAllRooms()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException($"You MUST call {nameof(UniversityEntitiesRepository)}.AssureInitialized(); prior to using it.");
            }

            var rooms = Instance?.Buildings
                .SelectMany(bd => 
                    bd.Rooms.Select(rm =>
                    {
                        Local::Room localGroup = MapConfig.Map<Cist::Room, Local::Room>(rm);
                        localGroup.Building = MapConfig.Map<Cist::Building, Local::BaseEntity<string>>(bd);
                        return localGroup;
                    })
                ).Distinct();
            return rooms;
        }
        #endregion

        #region Saved Entities
        public static List<Local::SavedEntity> GetSaved()
        {
            List<Local::SavedEntity> loadedEntities = new();

            string filePath = FilePath.SavedEntitiesList;
            if (!File.Exists(filePath))
            {
                return loadedEntities;
            }
            
            loadedEntities = Serialisation.FromJsonFile<List<Local::SavedEntity>>(filePath) ?? loadedEntities;
            return loadedEntities;
        }

        public static void UpdateSaved(List<Local::SavedEntity> savedEntities)
        {
            savedEntities ??= new();

            List<Local::SavedEntity> duplicates = savedEntities.GroupBy(e => e).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicates.Any())
            {
                throw new InvalidOperationException($"{nameof(savedEntities)} must be unique");
            }

            List<Local::SavedEntity> oldSavedEntities = GetSaved();
            // Removing cache from deleted saved entities if needed
            oldSavedEntities.Where(oldEntity => !savedEntities.Exists(entity => entity == oldEntity))
                .ToList()
                .ForEach((de) => 
                { 
                    try { File.Delete(FilePath.SavedTimetable(de.Entity.Type, de.Entity.ID)); } catch { } 
                });

            if (savedEntities.Any() && !savedEntities.Any(e => e.IsSelected))
            {
                // If no entity is selected, selecting first saved entity
                savedEntities.First().IsSelected = true;
            }

            // Saving saved entities list
            Serialisation.ToJsonFile(savedEntities, FilePath.SavedEntitiesList);
            MessagingCenter.Send(Application.Current, MessageTypes.SavedEntitiesChanged, savedEntities);

            if (oldSavedEntities.Count(e => e.IsSelected) != savedEntities.Count(e => e.IsSelected)
                || oldSavedEntities.Where(e => e.IsSelected).Except(savedEntities.Where(e => e.IsSelected)).Any())
            {
                MessagingCenter.Send(Application.Current, MessageTypes.SelectedEntitiesChanged, savedEntities.Where(e => e.IsSelected).ToList());
            }
        }
        #endregion
    }
}
