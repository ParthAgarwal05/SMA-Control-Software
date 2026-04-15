using Microsoft.EntityFrameworkCore;
using SMAControlApp.Data;
using SMAControlApp.Models;
using SMAControlApp.ViewModels;
using SMAControlApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace SMAControlApp
{
    public partial class App : Application
    {
        public static User CurrentUser { get; private set; } = null!;
        public static Configuration Config { get; private set; } = null!;
        public static ObservableCollection<ActuatorChannel> Actuators { get; private set; } = null!;

        public static GraphViewModel GraphVM { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Catch ALL unhandled exceptions on UI thread
            DispatcherUnhandledException += (s, ex) =>
            {
                MessageBox.Show(
                    $"UI Error:\n{ex.Exception.Message}\n\n" +
                    $"{ex.Exception.InnerException?.Message}\n\n" +
                    $"{ex.Exception.StackTrace}",
                    "Unhandled UI Exception");
                ex.Handled = true;
            };

            // Catch unhandled exceptions on background threads
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                var exception = ex.ExceptionObject as Exception;
                MessageBox.Show(
                    $"Fatal Error:\n{exception?.Message}\n\n{exception?.StackTrace}",
                    "Unhandled Exception");
            };

            try
            {
                base.OnStartup(e);

            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();
                using (var db = new AppDbContext())
                {
                    db.Database.Migrate();

                    var user = db.Users
                                 .Include(u => u.Config)
                                 .Include(u => u.Actuators)
                                 .FirstOrDefault(u => u.UserName == "User1");

                    if (user == null) user = CreateDummyUser(db);

                    CurrentUser = user;
                    Config = user.Config;
                    Actuators = new ObservableCollection<ActuatorChannel>(user.Actuators.OrderBy(a => a.ChannelId));
                }

                MainWindow mainWindow = new MainWindow();
                GraphVM = new GraphViewModel();
                mainWindow.LoginArea.Content = new LoginView();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Startup Error: {ex.Message}\n\n{ex.InnerException?.Message}\n\n{ex.StackTrace}",
                    "Startup Error");
                Shutdown();
            }
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
                        var newChannel = new ActuatorChannel
                        {
                            ChannelId = i,
                            UserId = CurrentUser.UserId
                            // Do NOT set User = CurrentUser here — EF will graph-walk
                            // CurrentUser.Config and try to track App.Config in this new
                            // context, conflicting with the already-tracked instance.
                        };
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
                GraphVM?.RebuildSeries();
            }
        }
    }
}

