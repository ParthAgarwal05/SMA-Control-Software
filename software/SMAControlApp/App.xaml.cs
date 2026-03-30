using Microsoft.EntityFrameworkCore;
using SMAControlApp.Data;
using SMAControlApp.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace SMAControlApp
{
    public partial class App : Application
    {
        // Globals for the UI to bind to
        public static User CurrentUser { get; private set; } = null!;
        public static Configuration Config { get; private set; } = null!;
        public static ObservableCollection<ActuatorChannel> Actuators { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using (var db = new AppDbContext())
            {
                // 1. Ensure DB exists and is ready
                db.Database.EnsureCreated();

                // 2. Load or Create the Dummy User (Engineer_One)
                var user = db.Users
                             .Include(u => u.Config)
                             .Include(u => u.Actuators)
                             .FirstOrDefault(u => u.UserName == "User1");

                if (user == null)
                {
                    user = CreateDummyUser(db);
                }

                // 3. Set global references
                CurrentUser = user;
                Config = user.Config;

                // Convert the DB list to an ObservableCollection for WPF UI binding
                Actuators = new ObservableCollection<ActuatorChannel>(user.Actuators.OrderBy(a => a.ChannelId));
            }

            // 4. Hook up the event listener for live changes
            Config.PropertyChanged += Config_PropertyChanged;
        }

        private User CreateDummyUser(AppDbContext db)
        {
            var newUser = new User { UserName = "User1", PasswordHash = "admin123" };
            newUser.Config = new Configuration { ActuatorCount = 17, MaxVoltage = 120 };

            for (int i = 1; i <= 17; i++)
            {
                newUser.Actuators.Add(new ActuatorChannel { ChannelId = i });
            }

            db.Users.Add(newUser);
            db.SaveChanges();
            return newUser;
        }

        private void Config_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Configuration.ActuatorCount))
            {
                SyncActuatorsWithDatabase();
            }
        }

        public static void SyncActuatorsWithDatabase()
        {
            int targetCount = Config.ActuatorCount;

            using (var db = new AppDbContext())
            {
                // Re-fetch user to track changes in this context
                var user = db.Users.Include(u => u.Actuators).First(u => u.UserId == CurrentUser.UserId);

                // Update the Config count in DB
                var dbConfig = db.Configs.First(c => c.UserId == CurrentUser.UserId);
                dbConfig.ActuatorCount = targetCount;

                // Handle Additions
                if (targetCount > Actuators.Count)
                {
                    for (int i = Actuators.Count + 1; i <= targetCount; i++)
                    {
                        var newChannel = new ActuatorChannel { ChannelId = i, UserId = CurrentUser.UserId };
                        db.Channels.Add(newChannel);
                        Actuators.Add(newChannel);
                    }
                }
                // Handle Removals
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