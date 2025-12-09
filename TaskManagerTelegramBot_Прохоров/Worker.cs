using TaskManagerTelegramBot_Прохоров.Classes;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TaskManagerTelegramBot_Прохоров
{
    public class Worker : BackgroundService
    {
        readonly string Token = "8357760400:AAGDdHGtNeHrnOrJC9iWkvojgIQy9NlU7bE";
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
            "Все события удалены.",

             "Укажите дни недели и время в формате:\n" +
            "<i>вторник, среда 21:00\nПолить цветы</i>"

        };


  
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            TelegramBotClient = new TelegramBotClient(Token);
            TelegramBotClient.StartReceiving(HandleUpdateAsync,
                HandleErroeAsync,
                null,
                new CancellationTokenSource().Token);

            TimerCallback TimerCalBack = new TimerCallback(Tick);
            Timer = new Timer(TimerCalBack, 0, 0, 60 * 1000);
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
            else if (command.ToLower() == "/create_repeattask") SendMessage(chatId, 7);
            else if (command.ToLower() == "/list_repeat")
            {
                Users User = users.Find(x => x.IdUser == chatId);
                if (User == null)
                {
                    SendMessage(chatId, 4); 
                }
                else if (User.RepeatEvents.Count == 0)
                {
                    SendMessage(chatId, 4);
                }
                else
                {
                    foreach (var repeat in User.RepeatEvents)
                    {
                        string days = string.Join(", ", repeat.Days.Select(d => d.ToString()));

                        await TelegramBotClient.SendMessage(
                            chatId,
                            $"Повторяющееся напоминание: {repeat.Message}\n" +
                            $"Дни недели: {days}\n" +
                            $"Время: {repeat.Time:hh\\:mm}",
                              replyMarkup: DeleteRepeatEvent(repeat.Message)

                        );
                    }
                }
            }
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

        private void GetMessages(Message message)
        {
            Console.WriteLine("Получено сообщение: " + message.Text + " от пользователя: " + message.Chat.Username);
            long IdUser = message.Chat.Id;
            string MessageUser = message.Text;
            SaveToDatabaseAsync(IdUser.ToString(), MessageUser, message.Chat.Username);
            if (message.Text.Contains("/")) Command(message.Chat.Id, message.Text);
            else if (message.Text.Equals("Удалить все задачи"))
            {
                Users User = users.Find(x => x.IdUser == message.Chat.Id);
                if(User == null) SendMessage(message.Chat.Id, 4);
                else if (User.Events.Count == 0) SendMessage(User.IdUser, 4);
                else
                {
                    User.Events =new List<Events>();
                    SendMessage(User.IdUser, 6);
                }
            }

            else
            {
                Users User = users.Find(x=>x.IdUser == message.Chat.Id);
                if(User == null)
                {
                    User = new Users(message.Chat.Id);
                    users.Add(User);
                }
                if (TryParseRepeatTask(MessageUser, out List<DayOfWeek> days, out TimeSpan repeatTime, out string repeatMessage))
                {
                    User.RepeatEvents.Add(new RepeatEvent(days, repeatTime, repeatMessage));
                    TelegramBotClient.SendMessage(message.Chat.Id, "Повторяющееся напоминание добавлено!");
                    return;
                }
                string[] Info = message.Text.Split('\n');
                if(Info.Length < 2)
                {
                    SendMessage(message.Chat.Id, 2);
                    return;
                }

                DateTime Time;
                if (CheckFormatDateTime(Info[0],out Time) == false)
                {
                    SendMessage(message.Chat.Id, 2);
                    return;
                }

                if(Time < DateTime.Now) SendMessage(message.Chat.Id, 3);

                User.Events.Add(new Events(Time, message.Text.Replace(Time.ToString("HH:mm dd.MM.yyyy") + "\n", "")));

            }
        }

        private async Task HandleUpdateAsync(
            ITelegramBotClient client,
            Update update,
            CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message) GetMessages(update.Message);

            else if (update.Type == UpdateType.CallbackQuery)
            {
                CallbackQuery query = update.CallbackQuery;
                Users User = users.Find(x=> x.IdUser == query.Message.Chat.Id);
                Events Event = User.Events.Find(x => x.Message == query.Data);
                User.Events.Remove(Event);
                SendMessage(query.Message.Chat.Id, 5);

                var repeatEvent = User.RepeatEvents.Find(x => x.Message == query.Data);
                if (repeatEvent != null)
                {
                    User.RepeatEvents.Remove(repeatEvent);
                    await TelegramBotClient.SendMessage(query.Message.Chat.Id, "Повторяющееся напоминание удалено!");
                }
            }
          
        }

        private async Task HandleErroeAsync(ITelegramBotClient client, Exception exception,HandleErrorSource source,CancellationToken token)
        {
            Console.WriteLine("ОШИБКА: " + exception.Message);
        }
        
        public async void Tick(object obj)
        {
            string nowHHmm = DateTime.Now.ToString("HH:mm");
            DayOfWeek nowDay = DateTime.Now.DayOfWeek;

            foreach (Users user in users)
            {
                foreach (var repeat in user.RepeatEvents)
                {
                    if (repeat.Days.Contains(nowDay) &&
                        repeat.Time.Hours == DateTime.Now.Hour &&
                        repeat.Time.Minutes == DateTime.Now.Minute)
                    {
                        await TelegramBotClient.SendMessage(user.IdUser, "Напоминание: " + repeat.Message);
                    }
                }
            }


            string TimeNow = DateTime.Now.ToString("HH.mm dd.MM.yyyy");
            foreach(Users User in users)
            {
                for (int i = 0; i < User.Events.Count; i++)
                {
                    if (User.Events[i].Time.ToString("HH.mm dd.MM.yyyy") != TimeNow) continue;

                    await TelegramBotClient.SendMessage(User.IdUser, "Напоминание: " + User.Events[i].Message);

                    User.Events.Remove(User.Events[i]);
                }
            }
        }


        private async void SaveToDatabaseAsync(string userId, string message, string username)
        {
            try
            {
                using (var db = new DbContexxTg())
                {
                    var command = new Command
                    {
                        User = "@" + username,
                        Commands = message
                    };

                    await db.CommandUser.AddAsync(command);
                    await db.SaveChangesAsync();

                    Console.WriteLine($"Сохранено в БД: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка БД: {ex.Message}");
            }
        }
        public static InlineKeyboardMarkup DeleteRepeatEvent(string message)
        {
            var inlineKeyboards = new List<InlineKeyboardButton>
        {
        InlineKeyboardButton.WithCallbackData("Удалить", message)
        };
            return new InlineKeyboardMarkup(inlineKeyboards);
        }
        private bool TryParseRepeatTask(string text, out List<DayOfWeek> days, out TimeSpan time, out string msg)
        {
            days = new List<DayOfWeek>();
            time = TimeSpan.Zero;
            msg = "";

            string[] lines = text.Split('\n');

            if (lines.Length < 2)
                return false;

            string header = lines[0].Trim();
            msg = lines[1].Trim();

            string[] parts = header.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (!TimeSpan.TryParse(parts.Last(), out time))
                return false;
            string daysPart = header.Replace(parts.Last(), "").Trim();

            string[] dayNames = daysPart.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (string dn in dayNames)
            {
                if (TryParseRussianDay(dn.Trim(), out DayOfWeek d))
                    days.Add(d);
            }

            return days.Count > 0;
        }
        private bool TryParseRussianDay(string name, out DayOfWeek day)
        {
            day = default;
            name = name.ToLower();

            var culture = new System.Globalization.CultureInfo("ru-RU");

            for (int i = 0; i < 7; i++)
            {
                string full = culture.DateTimeFormat.GetDayName((DayOfWeek)i).ToLower();

                if (full.StartsWith(name.Substring(0, Math.Min(3, name.Length))))
                {
                    day = (DayOfWeek)i;
                    return true;
                }
            }
            return false;
        }
    }
}
