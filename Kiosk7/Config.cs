using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kiosk7
{
    public class Config
    {
        public string Url { get; set; } = "https://www.example.com";
        public string Pin { get; set; } = "1234";
        public List<string> Allowlist { get; set; } = new();
        public bool ShowExitButton { get; set; } = false; // NEW

        public static Config Load(string exeBase)
        {
            var cfg = new Config();
            var path = Path.Combine(exeBase, "settings.cfg");
            if (!File.Exists(path)) return cfg;

            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || !line.Contains("=")) continue;

                var idx = line.IndexOf('=');
                var key = line[..idx].Trim().ToLowerInvariant();
                var val = line[(idx + 1)..].Trim();

                switch (key)
                {
                    case "url":
                        cfg.Url = val; break;
                    case "pin":
                        cfg.Pin = val; break;
                    case "allowlist":
                        cfg.Allowlist = val.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                           .Select(v => v.Trim())
                                           .ToList();
                        break;
                    case "show_exit_button":
                        cfg.ShowExitButton = val.Equals("true", StringComparison.OrdinalIgnoreCase) || val == "1";
                        break;
                }
            }

            // If no allowlist provided, allow the origin of Url by default
            try
            {
                var origin = new Uri(cfg.Url).GetLeftPart(UriPartial.Authority);
                if (!cfg.Allowlist.Any()) cfg.Allowlist.Add(origin);
            }
            catch { /* ignore parse errors */ }

            return cfg;
        }

        public void Save(string exeBase)
        {
            var path = System.IO.Path.Combine(exeBase, "settings.cfg");
            var allow = (Allowlist == null || Allowlist.Count == 0)
                ? ""
                : string.Join(",", Allowlist);

            var lines = new[]
            {
        $"url={Url}",
        $"pin={Pin}",
        $"allowlist={allow}",
        $"show_exit_button={(ShowExitButton ? "true" : "false")}"
    };
            System.IO.File.WriteAllLines(path, lines);
        }


        public bool IsAllowed(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var u)) return false;
            var origin = u.GetLeftPart(UriPartial.Authority);
            var full = u.ToString();

            foreach (var rule in Allowlist)
            {
                if (Matches(rule, full) || Matches(rule, origin)) return true;
            }
            return false;
        }

        private static bool Matches(string pattern, string text)
        {
            // supports https://*.example.com and '*' wildcards
            var rx = "^" + Regex.Escape(pattern)
                      .Replace("\\*\\.", "([a-z0-9-]+\\.)?")
                      .Replace("\\*", ".*")
                     + "$";
            return Regex.IsMatch(text, rx, RegexOptions.IgnoreCase);
        }
    }
}
