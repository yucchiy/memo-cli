using Scriban;
using System;
using System.Globalization;
using System.IO;

namespace Memo
{
    public static class Utility
    {
        public static string CategoryName2CategoryAbsoluteDirectoryPath(CommandConfig commandConfig, string categoryName)
        {
            return Path.Combine(commandConfig.HomeDirectory.FullName, categoryName);
        }

        public static DateTime FirstDayOfWeek()
        {
            var cultulre = CultureInfo.CurrentCulture;
            var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, cultulre.Calendar);
            var diff = now.DayOfWeek - cultulre.DateTimeFormat.FirstDayOfWeek; 
            return diff > 0 ? now.AddDays(-diff) : now.AddDays(7 + diff);
        }

        public static string Format(string text, object model)
        {
            var template = Template.ParseLiquid(text);
            return template.Render(model);
        }
    }
}