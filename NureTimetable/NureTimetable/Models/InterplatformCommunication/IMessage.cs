namespace NureTimetable.Models.InterplatformCommunication
{
    public interface IMessage
    {
        void LongAlert(string message);
        void ShortAlert(string message);
    }
}
