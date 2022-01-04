namespace NureTimetable.DAL.Moodle.Models.Courses
{
    public enum GetCourseContentsOption
    {
        /// <summary>
        /// (bool) Do not return modules, return only the sections structure
        /// </summary>
        ExcludeModules,

        /// <summary>
        /// (bool) Do not return module contents (i.e: files inside a resource)
        /// </summary>
        ExcludeContents,

        /// <summary>
        /// (bool) Return stealth modules for students in a special section (with id -1)
        /// </summary>
        IncludeStealthModules,

        /// <summary>
        /// (int) Return only this section
        /// </summary>
        SectionId,

        /// <summary>
        /// (int) Return only this section with number (order)
        /// </summary>
        SectionNumber,

        /// <summary>
        /// (int) Return only this module information (among the whole sections structure)
        /// </summary>
        CMId,

        /// <summary>
        /// (string) Return only modules with this name "label, forum, etc..."
        /// </summary>
        ModName,

        /// <summary>
        /// (int) Return only the module with this id (to be used with modname)
        /// </summary>
        ModId
    }
}