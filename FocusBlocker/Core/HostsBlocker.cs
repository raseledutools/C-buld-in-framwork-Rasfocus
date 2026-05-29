using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FocusBlocker.Core
{
    public static class HostsBlocker
    {
        private static readonly string HostsPath = @"C:\Windows\System32\drivers\etc\hosts";
        private const string MarkerStart = "# FocusBlocker START";
        private const string MarkerEnd   = "# FocusBlocker END";

        public static void BlockSites(List<string> domains)
        {
            var lines = File.ReadAllLines(HostsPath).ToList();
            RemoveMarkers(lines);
            lines.Add(MarkerStart);
            foreach (var d in domains.Where(d => !string.IsNullOrWhiteSpace(d)))
            {
                lines.Add($"127.0.0.1 {d.Trim().ToLower()}");
                lines.Add($"127.0.0.1 www.{d.Trim().ToLower()}");
            }
            lines.Add(MarkerEnd);
            File.WriteAllLines(HostsPath, lines);
            FlushDns();
        }

        public static void UnblockAll()
        {
            var lines = File.ReadAllLines(HostsPath).ToList();
            RemoveMarkers(lines);
            File.WriteAllLines(HostsPath, lines);
            FlushDns();
        }

        public static List<string> GetBlocked()
        {
            var result = new List<string>();
            bool inside = false;
            foreach (var line in File.ReadAllLines(HostsPath))
            {
                if (line.Trim() == MarkerStart) { inside = true; continue; }
                if (line.Trim() == MarkerEnd)   { inside = false; continue; }
                if (!inside) continue;
                var parts = line.Trim().Split(' ');
                if (parts.Length == 2 && !parts[1].StartsWith("www."))
                    result.Add(parts[1]);
            }
            return result;
        }

        private static void RemoveMarkers(List<string> lines)
        {
            int s = lines.FindIndex(l => l.Trim() == MarkerStart);
            int e = lines.FindIndex(l => l.Trim() == MarkerEnd);
            if (s >= 0 && e > s) lines.RemoveRange(s, e - s + 1);
        }

        private static void FlushDns()
        {
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                { FileName = "ipconfig", Arguments = "/flushdns", CreateNoWindow = true, UseShellExecute = false }); }
            catch { }
        }
    }
}
