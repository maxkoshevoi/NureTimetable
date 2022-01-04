using System.Collections.Generic;

namespace NureTimetable.DAL.Moodle.Models.Calendar
{
    public record GetUpCommingEventsResponse(List<Event> Events);
}