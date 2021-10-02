using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Notion.Client;

namespace NotionButler
{
    public static class Utils
    {
        private static CultureInfo _culture = CultureInfo.CreateSpecificCulture("ru-RU");

        public static string GetTodoTitle(Page page)
        {
            PropertyValue title;
            PropertyValue date;
            page.Properties.TryGetValue("Что", out title);
            page.Properties.TryGetValue("Когда", out date);

            var titleText = ((TitlePropertyValue)title).Title[0].PlainText;
            var startDate = ((DatePropertyValue)date).Date.Start;

            if (startDate != null)
            {
                var time = ((DateTime)startDate).ToString("t", _culture);
                if (time != "00:00") return $"{time} - {titleText}";
            }

            return titleText;
        }

        public static string GetAllTitlesAsBulletedList(List<Page> pages)
        {
            var titles = pages.Aggregate(
                new StringBuilder(),
                (result, page) => result.Append($"\n- {GetTodoTitle(page)}")
            );
            return titles.ToString();
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