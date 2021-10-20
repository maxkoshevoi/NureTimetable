namespace NureTimetable.DAL.Models.Moodle;

public enum GetCourseContentsOption
{
    /// <summary>
    /// (bool) Do not return modules, return only the sections structure
    /// </summary>
    excludemodules,

    /// <summary>
    /// (bool) Do not return module contents (i.e: files inside a resource)
    /// </summary>
    excludecontents,

    /// <summary>
    /// (bool) Return stealth modules for students in a special section (with id -1)
    /// </summary>
    includestealthmodules,

    /// <summary>
    /// (int) Return only this section
    /// </summary>
    sectionid,

    /// <summary>
    /// (int) Return only this section with number (order)
    /// </summary>
    sectionnumber,

    /// <summary>
    /// (int) Return only this module information (among the whole sections structure)
    /// </summary>
    cmid,

    /// <summary>
    /// (string) Return only modules with this name "label, forum, etc..."
    /// </summary>
    modname,

    /// <summary>
    /// (int) Return only the module with this id (to be used with modname
    /// </summary>
    modid
}
