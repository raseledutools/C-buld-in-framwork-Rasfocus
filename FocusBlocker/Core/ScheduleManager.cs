using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace FocusBlocker.Core
{
    public class ScheduleEntry
    {
        public string Name     { get; set; } = "";
        public DayOfWeek[] Days { get; set; } = Array.Empty<DayOfWeek>();
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime   { get; set; }
        public List<string> Sites { get; set; } = new();
        public bool Enabled { get; set; } = true;
    }

    public static class ScheduleManager
    {
        private static readonly string DataPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                         "FocusBlocker", "schedules.json");

        private static readonly Timer _checker = new() { Interval = 30_000 }; // every 30s
        private static bool _currentlyBlocking;

        public static List<ScheduleEntry> Entries { get; private set; } = new();

        public static void Init()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DataPath)!);
            Load();
            _checker.Tick += (_, _) => Evaluate();
            _checker.Start();
            Evaluate(); // immediate check
        }

        public static void Save()
        {
            var json = JsonSerializer.Serialize(Entries, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(DataPath, json);
        }

        public static void Load()
        {
            if (!File.Exists(DataPath)) return;
            try { Entries = JsonSerializer.Deserialize<List<ScheduleEntry>>(File.ReadAllText(DataPath)) ?? new(); }
            catch { Entries = new(); }
        }

        public static void Evaluate()
        {
            var now    = DateTime.Now;
            var tod    = now.TimeOfDay;
            var day    = now.DayOfWeek;

            var active = Entries
                .Where(e => e.Enabled && e.Days.Contains(day) && tod >= e.StartTime && tod < e.EndTime)
                .ToList();

            if (active.Any())
            {
                var allSites = active.SelectMany(e => e.Sites).Distinct().ToList();
                try { HostsBlocker.BlockSites(allSites); _currentlyBlocking = true; }
                catch { }
            }
            else if (_currentlyBlocking)
            {
                try { HostsBlocker.UnblockAll(); _currentlyBlocking = false; }
                catch { }
            }
        }

        public static string ActiveScheduleName()
        {
            var now = DateTime.Now;
            var tod = now.TimeOfDay;
            var day = now.DayOfWeek;
            return Entries.FirstOrDefault(e => e.Enabled && e.Days.Contains(day) && tod >= e.StartTime && tod < e.EndTime)?.Name ?? "";
        }
    }
}
