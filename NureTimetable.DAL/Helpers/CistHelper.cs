using NureTimetable.Core.Models.Exceptions;
using System;
using System.Text.RegularExpressions;

namespace NureTimetable.DAL.Helpers
{
    public static class CistHelper
    {
        public static T FromJson<T>(string json)
        {
            try
            {
                T result = Serialisation.FromJson<T>(json);
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
            catch (ArgumentException ex) when (Regex.IsMatch(json, @"ORA-\d+"))
            {
                Match match = Regex.Match(json, @"ORA-\d+");
                ex.Data.Add("Status", match.Value);
                throw new CistException($"Cist returned \"Oracle exception: {match.Value}\" exception", CistExceptionStatus.UnknownError, ex);
            }
        }
    }
}
