namespace NureTimetable.Core.Models.InterplatformCommunication
{
    public interface IMessageManager
    {
        void LongAlert(string message);

        void ShortAlert(string message);
    }
}
