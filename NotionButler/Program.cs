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
            var telegramId = long.Parse(Environment.GetEnvironmentVariable("TELEGRAM_ID"));

            var telegramClient = new TelegramClient(botToken, telegramId);
            await telegramClient.SendMessageToOwner("Ya zapustilsya");

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
