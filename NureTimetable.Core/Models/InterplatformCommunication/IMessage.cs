namespace NureTimetable.Core.Models.InterplatformCommunication
{
    public interface IMessage
    {
        void LongAlert(string message);

        void ShortAlert(string message);
    }
}
