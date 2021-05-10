using Scriban;
using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Memo
{
    public static class Utility
    {
        public static DateTime FirstDayOfWeek()
        {
            var cultulre = CultureInfo.CurrentCulture;
            var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, cultulre.Calendar);
            var diff = now.DayOfWeek - cultulre.DateTimeFormat.FirstDayOfWeek; 
            return diff > 0 ? now.AddDays(-diff) : now.AddDays(7 + diff);
        }

        public static bool TryParseTitle(string content, out string title)
        {
            title = string.Empty;
            var result = Regex.Match(content, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase);
            if (!result.Success) return false;

            title = result.Groups["Title"].Value;
            return true;
        }
    }
}