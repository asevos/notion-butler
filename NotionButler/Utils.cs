using Notion.Client;

namespace NotionButler
{
    public static class Utils
    {
        public static string GetTodoName(RetrievedPage page)
        {
            PropertyValue title;
            page.Properties.TryGetValue("Что", out title);
            return ((TitlePropertyValue)title).Title[0].PlainText;
        }
    }
}