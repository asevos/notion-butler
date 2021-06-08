using System;
using System.Threading.Tasks;

namespace NotionButler
{
    class Program
    {
        static async Task Main(string[] args)
        {
            DotNetEnv.Env.Load("../prod.env");
            var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN");
            var ownerId = long.Parse(Environment.GetEnvironmentVariable("TELEGRAM_ID"));
            var notionToken = Environment.GetEnvironmentVariable("NOTION_TOKEN");
            var tasksDbId = Environment.GetEnvironmentVariable("TASKS_DB_ID");

            var notion = new NotionWorker(notionToken, tasksDbId);
            var telegram = new TelegramWorker(botToken, ownerId, notion);

            var fetchInboxTask = notion.FetchInboxTodos();
            var fetchCurrentTodosTask = notion.FetchCurrentTodos();

            await Task.WhenAll(fetchInboxTask, fetchCurrentTodosTask);
            if (fetchCurrentTodosTask.Result.Count > 0)
            {
                var resultMessage = "Доброе утро! Сегодняшние дела:";
                fetchCurrentTodosTask.Result.ForEach(page =>
                {
                    var todoTitle = Utils.GetTodoTitle(page);
                    resultMessage += $"\n- {todoTitle}";
                });

                var inboxCount = fetchInboxTask.Result.Count;
                if (inboxCount > 0)
                {
                    resultMessage += $"\nЗадач в инбоксе: {inboxCount}";
                }

                await telegram.SendMessageToOwner(resultMessage);
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
