using Scriban;
using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

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

        public static bool TryParseTitle(string content, out string title)
        {
            title = string.Empty;
            var result = Regex.Match(content, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase);
            if (!result.Success) return false;

            title = result.Groups["Title"].Value;
            return true;
        }

        public static string LocalPath2Filename(string localPath)
        {
            var elements = localPath.Split('/');
            if (elements.Length == 0) return localPath;

            return elements[elements.Length - 1];
        }
    }
}