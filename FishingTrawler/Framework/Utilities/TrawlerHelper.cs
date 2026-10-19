using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FishingTrawler.Framework.External.GenericModConfigMenu;
using HarmonyLib;

namespace FishingTrawler.Framework.Utilities
{
    internal static class TrawlerHelper
    {
        internal static bool IsDayARange(string startDay, string endDay)
        {
            return (startDay != endDay);
        }

        internal static string DayRangeString(string startDay, string endDay, string front, string end)
        {
            if (!IsDayARange(startDay, endDay))
                return $"{front} {startDay}";

            return $"{front} {startDay} {end} {endDay}";
        }

        internal static bool IsTodayInRangeOfDay(string today, string startDay, string endDay)
        {
            int targetIndex = DayToIndex(today);
            int startIndex = DayToIndex(startDay);
            int endIndex = DayToIndex(endDay);

            for (int i = startIndex; i <= endIndex; i++)
            {
                if (i == targetIndex)
                    return true;
            }

            return false;
        }

        internal static string RangeOfDayString(string startDay, string endDay)
        {
            int startIndex = DayToIndex(startDay);
            int endIndex = DayToIndex(endDay);
            string str = string.Empty;

            for (int i = startIndex; i != (endIndex + 1) % 7; i = (i + 1) % 7)
            {
                str += $"{ModConfig.murphyDayToAppear[i]}, ";
            }

            return str[..^2];
        }

        internal static int DayToIndex(string day)
        {
            int index = -1;
            switch (day)
            {
                case "Monday":
                    index = 0;
                    break;
                case "Tuesday":
                    index = 1;
                    break;
                case "Wednesday":
                    index = 2;
                    break;
                case "Thursday":
                    index = 3;
                    break;
                case "Friday":
                    index = 4;
                    break;
                case "Saturday":
                    index = 5;
                    break;
                case "Sunday":
                    index = 6;
                    break;
            };
            return index;
        }
        
        internal static int RangeOfDayLength(string startDay, string endDay)
        {
            int start = DayToIndex(startDay);
            int end = DayToIndex(endDay);
            int length = 0;

            for (int i = start; i != (end + 1) % 7; i = (i + 1) % 7)
            {
                length++;
            }
            return length;
        }
        
        // Only used when startDay > endDay
        internal static int NextRangeOfDayLength(string startDay, string endDay)
        {
            int start = DayToIndex(startDay);
            int end = DayToIndex(endDay);
            int difference = start - end;

            return 7 - difference;
        }

        internal static bool IsRangePossible(string startDayBeach, string endDayBeach, string startDayIsland, string endDayIsland)
        {
            int beachRange = RangeOfDayLength(startDayBeach, endDayBeach);
            int islandRange = RangeOfDayLength(startDayIsland, endDayIsland);

            return (beachRange + islandRange) <= 5; // the reason why 5 is the upper bound is to give it one day in-between before Trawler appears
        }

        internal static bool IsDayNextTo(string firstDay, string secondDay)
        {
            int first = DayToIndex(firstDay);
            int second = DayToIndex(secondDay);
            int beforeIndex = (first - 1) % 7;
            int afterIndex = (first + 1) % 7;

            return (beforeIndex == second) || (afterIndex == second);
        }
    }
}
