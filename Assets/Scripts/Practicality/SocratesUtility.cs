public class SocratesUtility {
    public static string CentsToString(int cents) {
        var centsDisplay = "";
        centsDisplay += "$";
        centsDisplay += (cents / 100).ToString();
        centsDisplay += ".";
        centsDisplay += (cents % 100).ToString("00");

        return centsDisplay;
    }

    public static string ConvertMoneyAmountToText(int price) {
        return (price / 100F).ToString("0.00");
    }

    public static string TimeToTextMinutes(int convertFrom) {
        var seconds = convertFrom % 60;
        var minutes = (convertFrom - seconds) / 60;
        return $"{minutes}:{seconds.ToString("00")}";
    }
}