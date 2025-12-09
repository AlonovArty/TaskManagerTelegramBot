using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagerTelegramBot_Прохоров.Classes
{
    public class RepeatEvent
    {
        public List<DayOfWeek> Days { get; set; } = new();
        public TimeSpan Time { get; set; }
        public string Message { get; set; }

        public RepeatEvent(List<DayOfWeek> days, TimeSpan time, string message)
        {
            Days = days;
            Time = time;
            Message = message;
        }
    }
}
