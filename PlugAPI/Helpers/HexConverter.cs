using PlugAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PlugAPI.Helpers
{
    public static class HexConverter
    {
        public static string PreparePowerSchedule(IEnumerable<Schedule> entriesToSchedule)
        {
            int[] minuteSchedule = InitMinuteSchedule(); // an array containing one element for each minute of a day


            foreach (Schedule entry in entriesToSchedule)
            {
                // filling in scheduled minutes for each scheduled entry
                FillMinuteSchedule(entry, ref minuteSchedule);
            }

            //Transfer it to a 360 elements based hex schedule

            char[] hexSchedule = InitHexSchedule();

            int schedulePosition = 0;
            for (int quadrupletStart = 0; quadrupletStart < 1440; quadrupletStart += 4)
            {
                hexSchedule[schedulePosition] = QuadrupletToHex(minuteSchedule, quadrupletStart);
                schedulePosition++;
            }

            return new string(hexSchedule); // string with length 360, each position contains a value between '0' and 'F'
        }

        public static int[] InitMinuteSchedule()
        {
            const int MinutesOfADay = 1440;
            int[] scheduleTemplate = new int[MinutesOfADay];
            for (int i = 0; i < MinutesOfADay; i++)
            {
                scheduleTemplate[i] = 0;
            }
            return scheduleTemplate;
        }

        public static char[] InitHexSchedule()
        {
            char[] scheduleTemplate = new char[360];
            for (int i = 0; i < 360; i++)
            {
                scheduleTemplate[i] = '0';
            }
            return scheduleTemplate;
        }

        public static void FillMinuteSchedule(Schedule entry, ref int[] minuteSchedule)
        {
            var hourStart = entry.HourStart.Split(':');
            var hourEnd = entry.HourEnd.Split(':');
            FillMinuteSchedule(int.Parse(hourStart[0]), int.Parse(hourStart[1]),
                              int.Parse(hourEnd[0]), int.Parse(hourEnd[1]), ref minuteSchedule);

        }

        public static void FillMinuteSchedule(int fromHour, int fromMinute, int toHour, int toMinute, ref int[] minuteSchedule)
        {
            int begin = fromHour * 60 + fromMinute;
            int end = toHour * 60 + toMinute;
            for (int pos = begin; pos < end; pos++)
            {
                minuteSchedule[pos] = 1;
            }
        }

        public static char QuadrupletToHex(int[] minuteSchedule, int quadrupletBaseAdress)
        {
            int quadrupletSum = 0;
            for (int position = quadrupletBaseAdress; position < (quadrupletBaseAdress + 4); position++)
            {
                quadrupletSum = quadrupletSum * 2 + minuteSchedule[position];
            }
            return quadrupletSum.ToString("X")[0];
        }
    }
}