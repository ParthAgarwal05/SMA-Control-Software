using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SMAControlApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SMAControlApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<ActuatorChannel> Channels { get; set; }
        public DbSet<Configuration> Configs { get; set; }
        public DbSet<SensorSelector> SensorSelectors { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string path = Path.Combine(folder, "SMAControlApp");
            Directory.CreateDirectory(path);

            options.UseSqlite($"Data Source={Path.Combine(path, "sma_control.db")}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define the comparer for List<double>
            var doubleListComparer = new ValueComparer<List<double>>(
                (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c == null ? new List<double>() : c.ToList());

            var jsonOptions = new JsonSerializerOptions();

            modelBuilder.Entity<Configuration>()
                .Property(e => e.EquationCoefficients)
                .HasConversion(
                    v => JsonSerializer.Serialize(v ?? new List<double>(), jsonOptions),
                    v => string.IsNullOrWhiteSpace(v)
                        ? new List<double>()
                        : JsonSerializer.Deserialize<List<double>>(v, jsonOptions) ?? new List<double>())
                .Metadata.SetValueComparer(doubleListComparer);
        }
    }
}