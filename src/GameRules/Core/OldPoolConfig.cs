using System;
using System.Collections.Generic;

namespace GameRules.Core;

/// <summary>Reads the Pool switches out of the old Whole Floor Climate config, so moving to this mod keeps them.</summary>
public static class OldPoolConfig
{
    /// <returns>Each Pool key found with a true or false value; anything else in the file is ignored.</returns>
    public static IReadOnlyDictionary<string, bool> Read(string text)
    {
        var values = new Dictionary<string, bool>(StringComparer.Ordinal);
        var isInPool = false;

        foreach (var rawLine in text.Split('\n'))
        {
            var line = rawLine.Trim();

            if (line.StartsWith("[", StringComparison.Ordinal))
            {
                isInPool = line == "[Pool]";

                continue;
            }

            var equals = line.IndexOf('=');

            if (!isInPool || line.StartsWith("#", StringComparison.Ordinal) || equals <= 0) continue;

            if (bool.TryParse(line[(equals + 1)..].Trim(), out var value)) values[line[..equals].Trim()] = value;
        }

        return values;
    }
}
