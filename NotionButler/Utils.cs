using System;
using System.Collections.Generic;
using System.Linq;
using Notion.Client;

namespace NotionButler
{
    public static class Utils
    {
        public static string GetTodoTitle(RetrievedPage page)
        {
            PropertyValue title;
            page.Properties.TryGetValue("Что", out title);
            return ((TitlePropertyValue)title).Title[0].PlainText;
        }

        public static string GetAllTitlesAsBulletedList(List<RetrievedPage> pages)
        {
            return pages.Aggregate("", (result, page) => result += $"\n- {GetTodoTitle(page)}");
        }

        public static TimeSpan CalcTimeToNextFetch(TimeSpan checkTime)
        {
            var timeOfToday = DateTime.Now - DateTime.Today;
            if (timeOfToday <= checkTime)
            {
                return checkTime - timeOfToday;
            }
            else
            {
                return DateTime.Today.AddDays(1) - DateTime.Now + checkTime;
            }
        }

    }
}