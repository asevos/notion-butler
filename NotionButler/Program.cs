using System;
using System.Text;
using System.Threading.Tasks;

namespace NotionButler
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Setting env variables

            DotNetEnv.Env.Load("../prod.env");
            var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN");
            var ownerId = long.Parse(Environment.GetEnvironmentVariable("BOT_OWNER_ID"));
            var notionToken = Environment.GetEnvironmentVariable("NOTION_TOKEN");
            var tasksDbId = Environment.GetEnvironmentVariable("TASKS_DB_ID");
            var fetchTime = TimeSpan.Parse(Environment.GetEnvironmentVariable("DAILY_FETCH_TIME"));

            // Instanciating clients

            var notion = new NotionWorker(notionToken, tasksDbId);
            var telegram = new TelegramWorker(botToken, ownerId, notion);

            // Main working loop

            while (true)
            {
                try
                {
                    await CheckTodos(notion, telegram, fetchTime);
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("Unexpected error during todos check");
                    Console.WriteLine(e);
                    await telegram.SendMessageToOwner(e.ToString());
                }
            }
        }

        static async Task CheckTodos(NotionWorker notion, TelegramWorker telegram, TimeSpan fetchTime)
        {
            var waitTime = Utils.CalcTimeToNextFetch(fetchTime);
            Console.WriteLine($"Next fetch in {waitTime.ToString()}");
            await Task.Delay(waitTime);

            var fetchInboxTask = notion.FetchInboxTodos();
            var fetchCurrentTodosTask = notion.FetchCurrentTodos();
            await Task.WhenAll(fetchInboxTask, fetchCurrentTodosTask);

            var currentTodos = fetchCurrentTodosTask.Result;
            var inbox = fetchInboxTask.Result;

            if (currentTodos.Count > 0)
            {
                var notification = new StringBuilder("Доброе утро! ");
                if (inbox.Count > 0) notification.Append($"Задач в инбоксе: {inbox.Count}. ");
                notification.Append("Сегодняшние дела:");
                notification.Append(Utils.GetAllTitlesAsBulletedList(currentTodos));

                await telegram.SendMessageToOwner(notification.ToString());
            }
        }
    }
}
