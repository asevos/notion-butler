using System;
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
            var ownerId = long.Parse(Environment.GetEnvironmentVariable("TELEGRAM_ID"));
            var notionToken = Environment.GetEnvironmentVariable("NOTION_TOKEN");
            var tasksDbId = Environment.GetEnvironmentVariable("TASKS_DB_ID");
            var fetchTime = TimeSpan.Parse(Environment.GetEnvironmentVariable("DAILY_FETCH_TIME"));

            // Instanciating clients

            var notion = new NotionWorker(notionToken, tasksDbId);
            var telegram = new TelegramWorker(botToken, ownerId, notion);

            // Main working loop

            while (true)
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
                    var resultMessage = "Доброе утро! Сегодняшние дела:";
                    resultMessage += Utils.GetAllTitlesAsBulletedList(currentTodos);
                    if (inbox.Count > 0)
                    {
                        resultMessage += $"\nЗадач в инбоксе: {inbox.Count}";
                    }

                    await telegram.SendMessageToOwner(resultMessage);
                }
            }
        }
    }
}
