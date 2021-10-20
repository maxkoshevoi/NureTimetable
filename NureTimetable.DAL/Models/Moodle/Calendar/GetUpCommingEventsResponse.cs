using System.Collections.Generic;

namespace NureTimetable.DAL.Models.Moodle.Calendar;

public record GetUpCommingEventsResponse(List<Event> Events);
