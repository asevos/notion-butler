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
            _botClient.OnMessage += OnMessage;
        }

        private async void OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text == "/start")
            {
                await OnStartMessage(sender, e);
            }
            else if (e.Message.Chat.Id == _ownerId)
            {
                await AddTodoToInbox(sender, e);
            }
            else
            {
                await ProcessFeedback(sender, e);
            }
        }

        private async Task OnStartMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.From.Id == _ownerId)
            {
                await SendMessageToOwner($"Ты знаешь, что делать");
            }
            else
            {
                var greetingMsg = "This bot does not have publicly available features yet.\n\n"
                    + "If you have any suggestions for Notion Telegram Bot features please send "
                    + "them to the bot";
                await _botClient.SendTextMessageAsync(e.Message.Chat.Id, greetingMsg);
            }
        }

        private async Task AddTodoToInbox(object sender, MessageEventArgs e)
        {
            await _notionClient.AddTodoToInbox(e.Message.Text);
            await SendMessageToOwner($"Добавил в инбокс 👌");
        }

        public async Task SendMessageToOwner(string message)
        {
            await _botClient.SendTextMessageAsync(_ownerId, message);
        }

        private async Task ProcessFeedback(object sender, MessageEventArgs e)
        {
            await _botClient.ForwardMessageAsync(_ownerId, e.Message.From.Id, e.Message.MessageId);
            await _botClient.SendTextMessageAsync(e.Message.From.Id, "Thank you for your suggestion!");
        }
    }
}