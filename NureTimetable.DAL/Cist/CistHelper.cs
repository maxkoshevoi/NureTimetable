﻿using NureTimetable.Core.Models.Exceptions;
using System.Text.RegularExpressions;

namespace NureTimetable.DAL.Cist;

public static class CistHelper
{
    public static T FromJson<T>(string json)
    {
        try
        {
            T result = Serialization.FromJson<T>(json);
            return result;
        }
        catch (ArgumentException ex) when (json.Contains("ORA-04030"))
        {
            throw new CistException("Cist returned \"out of process memory\" exception", CistExceptionStatus.OutOfMemory, ex);
        }
        catch (ArgumentException ex) when (json.Contains("ORA-01089"))
        {
            throw new CistException("Cist returned \"immediate shutdown in progress - no operations permitted\" exception", CistExceptionStatus.ShutdownInProgress, ex);
        }
        catch (ArgumentException ex) when (json.Contains("ORA-01652"))
        {
            throw new CistException("Cist returned \"unable to extend temp segment\" exception", CistExceptionStatus.UnableToExtendTempSegment, ex);
        }
        catch (ArgumentException ex) when (json.Contains("ORA-08103"))
        {
            throw new CistException("Cist returned \"object no longer exists\" exception", CistExceptionStatus.ObjectNoLongerExists, ex);
        }
        catch (ArgumentException ex) when (Regex.IsMatch(json, @"ORA-\d+"))
        {
            Match match = Regex.Match(json, @"ORA-\d+");
            ex.Data.Add("Status", match.Value);
            throw new CistException($"Cist returned \"Oracle exception: {match.Value}\" exception", CistExceptionStatus.UnknownError, ex);
        }
    }
}
