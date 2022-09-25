namespace NureTimetable.BL;

public static class LocalizationService
{
    public static CultureInfo GetPreferredCulture()
    {
        if (SettingsRepository.Settings.Language != AppLanguage.FollowSystem)
        {
            return new CultureInfo((int)SettingsRepository.Settings.Language);
        }
        else if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ru")
        {
            return new CultureInfo((int)AppLanguage.Ukrainian);
        }

        return CultureInfo.CurrentCulture;
    }
}
