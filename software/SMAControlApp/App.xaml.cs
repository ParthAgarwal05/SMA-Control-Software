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
        public static User CurrentUser { get; set; } = null!;
        public static Configuration Config { get; set; } = null!;
        public static ObservableCollection<ActuatorChannel> Actuators { get; set; } = null!;

        public static GraphViewModel GraphVM { get; set; } = null!;

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
                    db.Database.Migrate();
                    SeedDefaultUsers(db);
                }

                MainWindow mainWindow = new MainWindow();
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

        private static void SeedDefaultUsers(AppDbContext db)
        {
            var defaultUsers = new[]
            {
                new { UserId = 1, UserName = "cs310",       PasswordHash = "123456" },
                new { UserId = 2, UserName = "nikunjbheda", PasswordHash = "123456" },
            };

            foreach (var u in defaultUsers)
            {
                if (db.Users.Any(x => x.UserId == u.UserId)) continue;

                var user = new User { UserName = u.UserName, PasswordHash = u.PasswordHash };
                user.Config = new Configuration { ActuatorCount = 17, AmplifierGain = 1, MinVoltage = 0, MaxVoltage = 120, EquationCoefficients = new List<double>() };
                for (int i = 1; i <= 17; i++)
                    user.Actuators.Add(new ActuatorChannel { ChannelId = i });

                db.Users.Add(user);
            }

            db.SaveChanges();
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