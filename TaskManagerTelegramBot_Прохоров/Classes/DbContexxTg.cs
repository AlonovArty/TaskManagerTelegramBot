using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TaskManagerTelegramBot_Прохоров.Classes
{
    public class DbContexxTg : DbContext
    {
        public DbSet<Command> CommandUser { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseMySql("server=127.0.0.1;port=3306;user=root;password=;database=TgBots;", new MySqlServerVersion(new Version(8, 0))
    );
        }
    }


}

