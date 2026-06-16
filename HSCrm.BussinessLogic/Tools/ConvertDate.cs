using MD.PersianDateTime.Standard;

namespace HSCrm.BussinessLagic.Tools
{
    public static class ConvertDate
    {
        public static DateTime ConvertShamsiToMiladi(string date)
        {
            PersianDateTime persianDate = PersianDateTime.Parse(date);
            return persianDate.ToDateTime();
        }

        public static string ConvertMiladiToShamsi(DateTime date, string format)
        {
            PersianDateTime persianDate = new PersianDateTime(date);
            return persianDate.ToString(format);
        }
    }
}
