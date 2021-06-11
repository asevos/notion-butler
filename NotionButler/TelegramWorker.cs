using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace NotionButler
{
    public class TelegramWorker
    {
        private readonly TelegramBotClient _botClient;
        private readonly long _ownerId;
        private readonly NotionWorker _notionClient;

        public TelegramWorker(string token, long ownerId, NotionWorker notionClient)
        {
            this._botClient = new TelegramBotClient(token);
            this._ownerId = ownerId;
            _notionClient = notionClient;

            SetOnMessageBehavior();

            _botClient.StartReceiving();
            _botClient.SendTextMessageAsync(_ownerId, "[Bot started]").Wait();
        }

        private void SetOnMessageBehavior()
        {
            _botClient.OnMessage += OnOwnerMessage;
            _botClient.OnMessage += OnOtherMessage;
        }

        private async void OnOwnerMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Chat.Id == _ownerId)
            {
                await _notionClient.AddTodoToInbox(e.Message.Text);
                await SendMessageToOwner($"Добавил в инбокс 👌");
            }
        }

        private async void OnOtherMessage(object sender, MessageEventArgs e)
        {
            // TODO: Add suggestions
            if (e.Message.Chat.Id != _ownerId)
            {
                await _botClient.SendTextMessageAsync(e.Message.Chat.Id, "Я тебя не знаю");
            }
        }

        public async Task SendMessageToOwner(string message)
        {
            await _botClient.SendTextMessageAsync(_ownerId, message);
        }
    }
}