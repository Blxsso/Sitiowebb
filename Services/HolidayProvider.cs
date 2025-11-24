using System;
using System.Collections.Generic;
using System.Linq;

namespace Sitiowebb.Services
{
    public record Holiday(DateTime Date, string Name, string CountryCode);

    public static class HolidayProvider
    {
        public static IEnumerable<Holiday> GetFor(string countryCode, int year)
        {
            var cc = (countryCode ?? "").Trim().ToUpperInvariant();
            return cc switch
            {
                "US" => US(year),
                "ES" => ES(year),
                "MX" => MX(year),
                "CR" => CR(year),
                "AR" => AR(year),
                "BR" => BR(year),
                "AU" => AU(year),
                "NZ" => NZ(year),
                _    => Array.Empty<Holiday>()
            };
        }

        // =================== Países ===================

        // Estados Unidos
        private static IEnumerable<Holiday> US(int y)
        {
            yield return H(y, 1, 1,  "New Year’s Day");
            yield return new Holiday(NthWeekdayOfMonth(y, 1, DayOfWeek.Monday, 3), "Martin Luther King Jr. Day", "US");  // 3er lunes enero
            yield return new Holiday(NthWeekdayOfMonth(y, 2, DayOfWeek.Monday, 3), "Presidents’ Day", "US");             // 3er lunes feb
            yield return new Holiday(LastWeekdayOfMonth(y, 5, DayOfWeek.Monday), "Memorial Day", "US");                   // último lunes mayo
            yield return H(y, 6, 19, "Juneteenth");
            yield return H(y, 7, 4,  "Independence Day");
            yield return new Holiday(NthWeekdayOfMonth(y, 9, DayOfWeek.Monday, 1), "Labor Day", "US");                    // 1er lunes sept
            yield return new Holiday(NthWeekdayOfMonth(y,11, DayOfWeek.Thursday,4), "Thanksgiving", "US");                // 4º jueves nov
            yield return H(y, 12, 25, "Christmas");
        }

        // España
        private static IEnumerable<Holiday> ES(int y)
        {
            yield return H(y, 1, 1,   "Año Nuevo");
            yield return H(y, 5, 1,   "Día del Trabajo");
            yield return H(y, 10,12,  "Fiesta Nacional de España");
            yield return H(y, 12, 6,  "Día de la Constitución");
            yield return H(y, 12,25,  "Navidad");
        }

        // México
        private static IEnumerable<Holiday> MX(int y)
        {
            yield return H(y, 1, 1,  "Año Nuevo");
            yield return H(y, 2, 5,  "Día de la Constitución");
            yield return H(y, 3,21,  "Natalicio de Benito Juárez");
            yield return H(y, 5, 1,  "Día del Trabajo");
            yield return H(y, 9,16,  "Día de la Independencia");
            yield return H(y,11,20,  "Día de la Revolución");
            yield return H(y,12,25,  "Navidad");
        }

        // Costa Rica
        private static IEnumerable<Holiday> CR(int y)
        {
            yield return H(y, 1, 1,  "Año Nuevo");
            yield return H(y, 4,11,  "Juan Santamaría");
            yield return H(y, 5, 1,  "Día del Trabajo");
            yield return H(y, 7,25,  "Anexión del Partido de Nicoya");
            yield return H(y, 8,15,  "Día de la Madre");
            yield return H(y, 9,15,  "Independencia");
            yield return H(y,12,25,  "Navidad");
        }

        // Argentina
        private static IEnumerable<Holiday> AR(int y)
        {
            yield return H(y, 1, 1,  "Año Nuevo");
            yield return H(y, 5, 1,  "Día del Trabajador");
            yield return H(y, 7, 9,  "Día de la Independencia");
            yield return H(y,12,25,  "Navidad");
        }

        // Brasil
        private static IEnumerable<Holiday> BR(int y)
        {
            yield return H(y, 1, 1,  "Confraternização Universal");
            yield return H(y, 4,21,  "Tiradentes");
            yield return H(y, 5, 1,  "Dia do Trabalho");
            yield return H(y, 9, 7,  "Independência");
            yield return H(y,10,12,  "Nossa Senhora Aparecida");
            yield return H(y,11,15,  "Proclamação da República");
            yield return H(y,12,25,  "Natal");
        }

        // Australia
        private static IEnumerable<Holiday> AU(int y)
        {
            yield return H(y, 1, 1,  "New Year’s Day");
            yield return H(y, 1, 26, "Australia Day");
            yield return H(y, 4, 25, "ANZAC Day");
            yield return H(y,12,25,  "Christmas");
            yield return H(y,12,26,  "Boxing Day");
        }

        // New Zealand
        private static IEnumerable<Holiday> NZ(int y)
        {
            yield return H(y, 1, 1, "New Year’s Day");
            yield return H(y, 2, 6, "Waitangi Day");
            yield return H(y, 4,25, "ANZAC Day");
            yield return H(y,12,25, "Christmas");
            yield return H(y,12,26, "Boxing Day");
        }

        // =================== Helpers ===================
        private static Holiday H(int y, int m, int d, string name, string cc = "")
            => new(new DateTime(y, m, d), name, cc);

        private static DateTime NthWeekdayOfMonth(int year, int month, DayOfWeek weekday, int n)
        {
            var dt = new DateTime(year, month, 1);
            while (dt.DayOfWeek != weekday) dt = dt.AddDays(1);
            return dt.AddDays(7 * (n - 1));
        }

        private static DateTime LastWeekdayOfMonth(int year, int month, DayOfWeek weekday)
        {
            var dt = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            while (dt.DayOfWeek != weekday) dt = dt.AddDays(-1);
            return dt;
        }
    }
}
