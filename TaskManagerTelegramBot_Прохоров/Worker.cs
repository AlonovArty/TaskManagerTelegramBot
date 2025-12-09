using TaskManagerTelegramBot_Прохоров.Classes;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TaskManagerTelegramBot_Прохоров
{
    public class Worker : BackgroundService
    {
        readonly string Toker = "";
        TelegramBotClient TelegramBotClient;
        List<Users> users = new List<Users> ();
        Timer Timer;
        List<string> Messages = new List<string>()
        {
            "Здравствуйте!" +
            "\nЭто бог напоминалка, чтобы напоминать вам о важных событиях и мероприятиях!" +
            "\nДобавь бота в список контактов и настой уведомления! Прошу не пропускать!",

            "Укажите дату и верия напоминания в формате:" +
            "\n<i><b>12:51 26.04.2025</b>" +
            "\nНапомни о том, что я хоетл сходить в магазин.</i>",


              "Кажется, у тебя что-то не получилось! :" +
            "\n<i><b>12:51 26.04.2025</b>" +
            "\nНапомни о том, что я хоетл сходить в магазин.</i>",

            "",
            "Задачи пользователя не найдены.",
            "Событие удалено",
            "Все события удалены."

        };


  
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }

        public bool CheckFormatDateTime(string value, out DateTime time)
        {
            return DateTime.TryParse(value, out time);
        }

        private static ReplyKeyboardMarkup GetButtons()
        {
            List<KeyboardButton> keyboardButtons = new List<KeyboardButton> ();
            keyboardButtons.Add(new KeyboardButton("Удалить все задачи"));

            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    keyboardButtons
                }
            };
        }
       
        public static InlineKeyboardMarkup DeleteEvent(string Message)
        {
            List<InlineKeyboardButton> inlineKeyboards = new List<InlineKeyboardButton> ();
            inlineKeyboards.Add(new InlineKeyboardButton("Удалить",Message));
            return new InlineKeyboardMarkup(inlineKeyboards);
        }

        public async void SendMessage(long chatId,int typeMessage)
        {
            if(typeMessage != 3)
            {
                await TelegramBotClient.SendMessage(chatId, Messages[typeMessage],ParseMode.Html,replyMarkup: GetButtons());
            }
            else if (typeMessage == 3)
            {

                await TelegramBotClient.SendMessage(chatId,
                    $"Указанное вами время и дата не могут быть установлены, " +
                    $"потому что сейчас уже : {DateTime.Now.ToString("HH.mm dd.MM.yyyy")}");
            }
        }

        public async void Command(long chatId, string command)
        {

            if (command.ToLower() == "/start") SendMessage(chatId, 0);
            else if (command.ToLower() == "/create_task") SendMessage (chatId, 1);
            else if (command.ToLower() == "/list_tasks")
            {
                Users User = users.Find(x=>x.IdUser  == chatId);
                if (User == null) SendMessage(chatId, 4);
                else if (User.Events.Count == 0) SendMessage(chatId, 4);
                else
                {
                    foreach (Events Event in User.Events)
                    {
                        await TelegramBotClient.SendMessage(chatId,
                            $"Уведомить пользователя: {Event.Time.ToString("HH:mm dd.MM.yyyy")}" +
                            $"\nСообщение: {Event.Message}",
                            replyMarkup:DeleteEvent(Event.Message));
                    }
                }

            }
        }
    }
}
