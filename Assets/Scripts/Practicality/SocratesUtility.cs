public class SocratesUtility {
    /// <summary>
    /// Returns the passed cents as a string formatted $DD.CC
    /// </summary>
    /// <param name="cents"></param>
    /// <returns></returns>
    public static string CentsToString(int cents) {
        var centsDisplay = "";
        centsDisplay += "$";
        centsDisplay += (cents / 100).ToString();
        centsDisplay += ".";
        centsDisplay += (cents % 100).ToString("00");

        return centsDisplay;
    }
    
    /// <summary>
    /// Returns the passed time as a string formatted HH:MM:SS
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static string FormatTimeHms(int time) {
        int seconds = (int)time % 60;
        int minutes = (int)((time - seconds) / 60);
        int displayMinutes = minutes % 60;
        int hours = (int)(minutes / 60);
        
        return $"{hours:00}:{displayMinutes:00}:{seconds:00}";
    }
}