using Microsoft.EntityFrameworkCore;
using SMAControlApp.Data;
using SMAControlApp.Models;
using SMAControlApp.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace SMAControlApp
{
    public partial class App : Application
    {
        public static User CurrentUser { get; private set; } = null!;
        public static Configuration Config { get; private set; } = null!;
        public static ObservableCollection<ActuatorChannel> Actuators { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();

                var user = db.Users
                             .Include(u => u.Config)
                             .Include(u => u.Actuators)
                             .FirstOrDefault(u => u.UserName == "User1");

                if (user == null)
                    user = CreateDummyUser(db);

                CurrentUser = user;
                Config = user.Config;
                Actuators = new ObservableCollection<ActuatorChannel>(
                    user.Actuators.OrderBy(a => a.ChannelId));
            }

            Config.PropertyChanged += Config_PropertyChanged;

            MainWindow mainWindow = new MainWindow();
            mainWindow.LoginArea.Content = new LoginView();
            mainWindow.Show();
        }

        private User CreateDummyUser(AppDbContext db)
        {
            var newUser = new User { UserName = "User1", PasswordHash = "admin123" };
            newUser.Config = new Configuration { ActuatorCount = 17, MaxVoltage = 120 };

            for (int i = 1; i <= 17; i++)
                newUser.Actuators.Add(new ActuatorChannel { ChannelId = i });

            db.Users.Add(newUser);
            db.SaveChanges();
            return newUser;
        }

        private void Config_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Configuration.ActuatorCount))
                SyncActuatorsWithDatabase();
        }

        public static void SyncActuatorsWithDatabase()
        {
            int targetCount = Config.ActuatorCount;

            using (var db = new AppDbContext())
            {
                var dbConfig = db.Configs.First(c => c.UserId == CurrentUser.UserId);
                dbConfig.ActuatorCount = targetCount;

                if (targetCount > Actuators.Count)
                {
                    for (int i = Actuators.Count + 1; i <= targetCount; i++)
                    {
                        var newChannel = new ActuatorChannel { ChannelId = i, UserId = CurrentUser.UserId };
                        db.Channels.Add(newChannel);
                        Actuators.Add(newChannel);
                    }
                }
                else if (targetCount < Actuators.Count)
                {
                    while (Actuators.Count > targetCount)
                    {
                        var toRemove = Actuators.Last();
                        var dbItem = db.Channels.FirstOrDefault(c => c.Id == toRemove.Id);
                        if (dbItem != null) db.Channels.Remove(dbItem);
                        Actuators.Remove(toRemove);
                    }
                }

                db.SaveChanges();
            }
        }
    }
}

