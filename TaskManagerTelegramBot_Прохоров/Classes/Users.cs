using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagerTelegramBot_Прохоров.Classes
{
    public class Users
    {
        public int Id { get; set; } 
        public long IdUser { get; set; }
        public List<Events> Events { get; set; }
        public List<RepeatEvent> RepeatEvents { get; set; }
        public Users(long idUser)
        {
            IdUser = idUser;
            Events = new List<Events>();
            RepeatEvents = new List<RepeatEvent>();
        }

    }
}
