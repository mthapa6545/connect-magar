using System;
namespace ConnectMagar.Helper
{
    public class Utility
    {
        public static string GetPrettyDate(DateTime date)
        {
            // 1.
            // Get time span elapsed since the date.
            var s = DateTime.Now.Subtract(date);

            // 2.
            // Get total number of days elapsed.
            var dayDiff = (int)s.TotalDays;

            // 3.
            // Get total number of seconds elapsed.
            var secDiff = (int)s.TotalSeconds;

            // 4.
            // Don't allow out of range values.
            if (dayDiff < 0)
            {
                return null;
            }

            // 5.
            // Handle same-day times.
            if (dayDiff == 0)
            {
                // A.
                // Less than one minute ago.
                if (secDiff < 60)
                {
                    return "just now";
                }
                // B.
                // Less than 2 minutes ago.
                if (secDiff < 120)
                {
                    return "1m ago";
                }
                // C.
                // Less than one hour ago.
                if (secDiff < 3600)
                {
                    return string.Format("{0}m ago",
                        Math.Floor((double)secDiff / 60));
                }
                // D.
                // Less than 2 hours ago.
                if (secDiff < 7200)
                {
                    return "1h ago";
                }
                // E.
                // Less than one day ago.
                if (secDiff < 86400)
                {
                    return string.Format("{0}h ago",
                        Math.Floor((double)secDiff / 3600));
                }
            }
            // 6.
            // Handle previous days.
            if (dayDiff == 1)
            {
                return "yesterday";
            }
            if (dayDiff < 7)
            {
                return string.Format("{0}d ago",
                    dayDiff);
            }
            if (dayDiff < 91)
            {
                return string.Format("{0}w ago",
                    Math.Ceiling((double)dayDiff / 7));
            }
            // 7.
            // Handle very old values            
            return date.ToShortDateString();
        }
    }
}