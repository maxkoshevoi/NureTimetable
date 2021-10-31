using System;

namespace NureTimetable.DAL.Moodle.Models;

public record ErrorResult(string ErrorCode)
{
    public string? Error { get; init; }

    public string? Message { get; init; }

    public string? ErrorMessage => Error ?? Message;
}
