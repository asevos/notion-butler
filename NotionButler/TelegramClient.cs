using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace NotionButler
{
    public class TelegramClient
    {
        private readonly TelegramBotClient _botClient;
        private readonly long _ownerId;

        public TelegramClient(string token, long ownerId)
        {
            this._botClient = new TelegramBotClient(token);
            this._ownerId = ownerId;

            SetOnMessageBehavior();

            _botClient.StartReceiving();
        }

        private void SetOnMessageBehavior()
        {
            _botClient.OnMessage += OnOwnerMessage;
        }

        private async void OnOwnerMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Chat.Id == _ownerId)
            {
                await SendMessageToOwner($"Ты просишь '{e.Message.Text}', но ты меня этому ещё не научил");
            }
        }

        public async Task SendMessageToOwner(string message)
        {
            await _botClient.SendTextMessageAsync(_ownerId, message);
        }
    }
}