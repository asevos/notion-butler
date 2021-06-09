using System;
using System.Threading.Tasks;

namespace NotionButler
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Fetching env variables

            DotNetEnv.Env.Load("../prod.env");
            var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN");
            var ownerId = long.Parse(Environment.GetEnvironmentVariable("TELEGRAM_ID"));
            var notionToken = Environment.GetEnvironmentVariable("NOTION_TOKEN");
            var tasksDbId = Environment.GetEnvironmentVariable("TASKS_DB_ID");
            // TODO - make env variable for this?
            var fetchTasksTime = TimeSpan.FromMinutes(3 * 60 + 38);

            // Instanciating clients

            var notion = new NotionWorker(notionToken, tasksDbId);
            var telegram = new TelegramWorker(botToken, ownerId, notion);

            // Main working loop

            while (true)
            {
                var waitTime = Utils.CalcTimeToNextFetch(fetchTasksTime);
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
