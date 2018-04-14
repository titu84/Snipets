using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HolidayClass
{
    public static class HolidayClass
    {
        public static bool IsHolidayPL(DateTime day, bool withSaturday = false, bool withSunday = false)
        {
            // Weekend jeśli brany pod uwagę
            if ((withSaturday && day.DayOfWeek == DayOfWeek.Saturday) ||
                (withSunday && day.DayOfWeek == DayOfWeek.Sunday))
                return true;
            // Święta stałe
            if (day.Month == 1 && day.Day == 1 ||
                day.Month == 1 && day.Day == 6 ||
                day.Month == 5 && day.Day == 1 ||
                day.Month == 5 && day.Day == 3 ||
                day.Month == 8 && day.Day == 15 ||
                day.Month == 11 && day.Day == 1 ||
                day.Month == 11 && day.Day == 11 ||
                day.Month == 12 && day.Day == 25 ||
                day.Month == 12 && day.Day == 26)
                return true;
            // Święta ruchome - obliczanie Wielkanocy.
            int a = day.Year % 19;
            int b = day.Year % 4;
            int c = day.Year % 7;
            int d = (a * 19 + 24) % 30;
            int e = (2 * b + 4 * c + 6 * d + 5) % 7;
            if (d == 29 && e == 6)
                d -= 7;
            if (d == 28 && e == 6 && a > 10)
                d -= 7;
            var easter = new DateTime(day.Year, 3, 22).AddDays(d + e);
            if (day == easter || // Wielkanoc (niedziela).
                day.AddDays(-1) == easter || // Wielkanoc (poniedziałek).               
                day.AddDays(-49) == easter || //Zielone Świątki                
                day.AddDays(-60) == easter) //Boże Ciało
                return true;
            return false;
        }
        public static int CountHolidays(DateTime begin, DateTime end, bool withSaturday = false, bool withSunday = false)
        {
            int count = 0;
            if (begin.Date > end.Date)
                throw new ArgumentException();
            DateTime day = begin.Date;
            while (day <= end)
            {
                count = IsHolidayPL(day, withSaturday, withSunday) == true ? ++count : count;
                day = day.AddDays(1);
            }
            return count;
        }
    }
}
