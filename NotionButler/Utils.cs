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
    }
}