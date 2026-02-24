using System;
using System.Collections.Generic;
using System.IO;

namespace KitBox;

/// <summary>
/// Reads a .env file (key=value per line) and exposes its entries.
/// Looks for the file relative to the application base directory.
/// </summary>
public static class EnvConfig
{
    private static readonly Dictionary<string, string> _values = new();

    static EnvConfig()
    {
        // Search in the app directory and one level up (useful during development)
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, ".env"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".env")
        };

        foreach (var path in candidates)
        {
            var full = Path.GetFullPath(path);
            if (!File.Exists(full)) continue;

            foreach (var line in File.ReadAllLines(full))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
                    continue;

                var sep = trimmed.IndexOf('=');
                if (sep <= 0) continue;

                var key   = trimmed[..sep].Trim();
                var value = trimmed[(sep + 1)..].Trim();
                _values[key] = value;
            }
            break; // use first file found
        }
    }

    /// <summary>Returns the value for the given key, or the fallback if not found.</summary>
    public static string Get(string key, string fallback = "")
        => _values.TryGetValue(key, out var val) ? val : fallback;
}
