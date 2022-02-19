using Nito.AsyncEx;
using NureTimetable.DAL.Cist.Consts;
using NureTimetable.DAL.Consts;
using NureTimetable.DAL.Settings;
using System.Net;
using Cist = NureTimetable.DAL.Cist.Models;
using Local = NureTimetable.DAL.Models;

namespace NureTimetable.DAL.Cist;

public static class UniversityEntitiesRepository
{
    public static bool IsInitialized { get; private set; } = false;

    private static readonly AsyncLock updatingSavedLock = new();
    private static readonly object initializingLock = new();

    public class UniversityEntitiesCistUpdateResult
    {
        public UniversityEntitiesCistUpdateResult(Cist::University updatedUniversity)
        {
            UpdatedUniversity = updatedUniversity;
        }

        public UniversityEntitiesCistUpdateResult(Cist::University updatedUniversity, Exception? groupsException, Exception? teachersException, Exception? roomsException)
            : this(updatedUniversity)
        {
            GroupsException = groupsException;
            TeachersException = teachersException;
            RoomsException = roomsException;
        }

        public Cist::University UpdatedUniversity { get; }

        public Exception? GroupsException { get; }
        public Exception? TeachersException { get; }
        public Exception? RoomsException { get; }

        public bool IsAllSuccessful =>
            GroupsException == null && TeachersException == null && RoomsException == null;

        public bool IsAllFail =>
            GroupsException != null && TeachersException != null && RoomsException != null;

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
        lock (initializingLock)
        {
            if (!IsInitialized)
            {
                Instance = GetAsync().Result;
            }
        }
    }

    public static async Task<UniversityEntitiesCistUpdateResult> UpdateFromCistAsync()
    {
        Cist::University? university = await GetLocalAsync();
        var result = await UpdateFromCistAsync(university);
        Instance = result.UpdatedUniversity;

        return result;
    }
    #endregion

    #region Private
    private static Cist::University? _instance;
    private static Cist::University Instance
    {
        get => _instance ?? throw new InvalidOperationException($"Please call {nameof(UniversityEntitiesRepository.AssureInitialized)} first.");
        set
        {
            _instance = value;
            IsInitialized = true;

            MessagingCenter.Send(Application.Current, MessageTypes.UniversityEntitiesUpdated);
        }
    }

    private static async Task<Cist::University> GetAsync()
    {
        Cist::University? localUniversity = await GetLocalAsync();
        if (localUniversity != null)
        {
            return localUniversity;
        }

        var cistResult = await UpdateFromCistAsync(null);
        if (!cistResult.IsAllFail)
        {
            return cistResult.UpdatedUniversity;
        }

        return new();
    }

    private static async Task<Cist::University?> GetLocalAsync()
    {
        string filePath = FilePath.UniversityEntities;
        if (!File.Exists(filePath))
        {
            return null;
        }

        var loadedUniversity = await Serialisation.FromJsonFile<Cist::University>(filePath);
        return loadedUniversity;
    }

    private static async Task<UniversityEntitiesCistUpdateResult> UpdateFromCistAsync(Cist::University? university)
    {
        university ??= new();

        if (SettingsRepository.CheckCistAllEntitiesUpdateRights() == false)
        {
            return new(university);
        }

        var groupsTask = TaskWithFallbacks(GetAllGroupsFromCistAsync, GetAllGroupsFromCistHtmlAsync);
        var teachersTask = TaskWithFallbacks(GetAllTeachersFromCistAsync, GetAllTeachersFromCistHtmlAsync);
        var roomsTask = GetAllRoomsFromCistAsync();

        UniversityEntitiesCistUpdateResult result = new(university);
        try
        {
            await Task.WhenAll(groupsTask, teachersTask, roomsTask);
        }
        catch
        {
            result = new
            (
                university,
                groupsTask.Exception?.InnerException,
                teachersTask.Exception?.InnerException,
                roomsTask.Exception?.InnerException
            );
        }

        if (!roomsTask.IsFaulted)
        {
            university.Buildings = roomsTask.Result;
        }
        if (!groupsTask.IsFaulted)
        {
            foreach (var faculty in groupsTask.Result)
            {
                Cist::Faculty? oldFaculty = university.Faculties.FirstOrDefault(f => f.Id == faculty.Id);
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
                Cist::Faculty? oldFaculty = university.Faculties.FirstOrDefault(f => f.Id == faculty.Id);
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
            await Serialisation.ToJsonFile(university, FilePath.UniversityEntities);
            if (result.IsAllSuccessful)
            {
                SettingsRepository.Settings.LastCistAllEntitiesUpdate = DateTime.Now;
            }
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
                ExceptionService.LogException(ex);
                if (tasksLeft == 0 || ex is WebException)
                {
                    throw;
                }
            }
        }
    }

    #region From Cist Api
    private static async Task<List<Cist::Faculty>> GetAllGroupsFromCistAsync()
    {
        Analytics.TrackEvent("Cist request", new Dictionary<string, string>
            {
                { "Type", "GetAllGroups" },
                { "Hour of the day", DateTime.Now.Hour.ToString() }
            });

        string responseStr = await Urls.CistApiAllGroups.GetStringOrWebExceptionAsync();
        Cist::University newUniversity = CistHelper.FromJson<Cist::UniversityRootObject>(responseStr).University;

        return newUniversity.Faculties;
    }

    private static async Task<List<Cist::Faculty>> GetAllTeachersFromCistAsync()
    {
        Analytics.TrackEvent("Cist request", new Dictionary<string, string>
            {
                { "Type", "GetAllTeachers" },
                { "Hour of the day", DateTime.Now.Hour.ToString() }
            });

        string responseStr = await Urls.CistApiAllTeachers.GetStringOrWebExceptionAsync();
        Cist::University newUniversity = CistHelper.FromJson<Cist::UniversityRootObject>(responseStr).University;

        return newUniversity.Faculties;
    }

    private static async Task<List<Cist::Building>> GetAllRoomsFromCistAsync()
    {
        Analytics.TrackEvent("Cist request", new Dictionary<string, string>
            {
                { "Type", "GetAllRooms" },
                { "Hour of the day", DateTime.Now.Hour.ToString() }
            });

        string responseStr = await Urls.CistApiAllRooms.GetStringOrWebExceptionAsync();
        responseStr = responseStr.Replace("\n", "").Replace("[}]", "[]");
        Cist::University newUniversity = CistHelper.FromJson<Cist::UniversityRootObject>(responseStr).University;

        return newUniversity.Buildings;
    }
    #endregion

    #region From Cist Html
    private static async Task<List<Cist::Faculty>> GetAllGroupsFromCistHtmlAsync()
    {
        Analytics.TrackEvent("Cist request", new Dictionary<string, string>
            {
                { "Type", "GetAllGroupsHtml" },
                { "Hour of the day", DateTime.Now.Hour.ToString() }
            });

        List<Cist::Faculty> faculties = new();

        // Getting branches
        Uri uri = Urls.CistSiteAllGroups(null);
        string branchesListPage = await uri.GetStringOrWebExceptionAsync();
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
            string branchGroupsPage = await uri.GetStringOrWebExceptionAsync();
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

    private static async Task<List<Cist::Faculty>> GetAllTeachersFromCistHtmlAsync()
    {
        Analytics.TrackEvent("Cist request", new Dictionary<string, string>
            {
                { "Type", "GetAllTeachersHtml" },
                { "Hour of the day", DateTime.Now.Hour.ToString() }
            });

        List<Cist::Faculty> faculties = new();

        // Getting faculties
        string facultyListPage = await Urls.CistSiteAllTeachers().GetStringOrWebExceptionAsync();
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
        string facultyPage = await Urls.CistSiteAllTeachers(facultyId).GetStringOrWebExceptionAsync();

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
        string branchGroupsPage = await uri.GetStringOrWebExceptionAsync();
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
        var groups = Instance.Faculties
            .SelectMany(fac =>
                fac.Directions.SelectMany(dir =>
                    dir.Groups.Select(gr =>
                    {
                        Local::Group localGroup = MapConfig.Map<Cist::Group, Local::Group>(gr);
                        localGroup.Faculty = MapConfig.Map<Cist::Faculty, Local::BaseEntity<long>>(fac);
                        localGroup.Direction = MapConfig.Map<Cist::Direction, Local::BaseEntity<long>>(dir);
                        return localGroup;
                    })
                    .Concat(dir.Specialities.SelectMany(sp =>
                    sp.Groups.Select(gr =>
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
        var teachers = Instance.Faculties
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
        var rooms = Instance.Buildings
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
    public static async Task<List<Local::SavedEntity>> GetSavedAsync()
    {
        List<Local::SavedEntity> loadedEntities = new();

        string filePath = FilePath.SavedEntitiesList;
        if (!File.Exists(filePath))
        {
            return loadedEntities;
        }

        loadedEntities = await Serialisation.FromJsonFile<List<Local::SavedEntity>>(filePath) ?? loadedEntities;
        return loadedEntities;
    }

    public static Task ModifySavedAsync(Action<List<Local::SavedEntity>> modefier) =>
        ModifySavedAsync(se =>
        {
            modefier(se);
            return false;
        });

    /// <param name="modefier">Returns is cancelation requested</param>
    /// <returns>Is updated</returns>
    public static async Task<bool> ModifySavedAsync(Func<List<Local::SavedEntity>, bool> modefier)
    {
        using var lockHandle = updatingSavedLock.Lock();

        List<Local::SavedEntity> savedEntities = await GetSavedAsync();
        bool isCancelationRequested = modefier(savedEntities);
        if (isCancelationRequested)
        {
            return false;
        }

        await UpdateSavedAsync(savedEntities);
        return true;
    }

    private static async Task UpdateSavedAsync(List<Local::SavedEntity> savedEntities)
    {
        savedEntities ??= new();

        List<Local::SavedEntity> duplicates = savedEntities.GroupBy(e => e).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicates.Any())
        {
            savedEntities = savedEntities.GroupBy(e => e).Select(g => g.Key).ToList();
            Crashes.TrackError(new InvalidOperationException($"{nameof(savedEntities)} must be unique"));
        }

        List<Local::SavedEntity> oldSavedEntities = await GetSavedAsync();
        // Removing cache from deleted saved entities if needed
        oldSavedEntities.Where(oldEntity => !savedEntities.Exists(entity => entity == oldEntity))
            .ToList()
            .ForEach((de) =>
            {
                try { File.Delete(FilePath.SavedTimetable(de.Entity.Type, de.Entity.ID)); } catch { }
            });

        if (savedEntities.Any() && savedEntities.None(e => e.IsSelected))
        {
            // If no entity is selected, selecting first saved entity
            savedEntities.First().IsSelected = true;
        }

        // Saving saved entities list
        await Serialisation.ToJsonFile(savedEntities, FilePath.SavedEntitiesList);
        MessagingCenter.Send(Application.Current, MessageTypes.SavedEntitiesChanged, savedEntities);

        if (oldSavedEntities.Count(e => e.IsSelected) != savedEntities.Count(e => e.IsSelected)
            || oldSavedEntities.Where(e => e.IsSelected).Except(savedEntities.Where(e => e.IsSelected)).Any())
        {
            MessagingCenter.Send(Application.Current, MessageTypes.SelectedEntitiesChanged, savedEntities.Where(e => e.IsSelected).ToList());
        }
    }
    #endregion
}
