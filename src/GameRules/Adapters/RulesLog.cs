using System;
using System.IO;
using System.Threading.Tasks;
using BepInEx.Logging;
using GameRules.Core;

namespace GameRules.Adapters;

/// <summary>
/// Writes to the BepInEx log and to an append-only file that survives restarts
/// (LogOutput.log and Player.log are both overwritten on the next launch).
/// Never throws: it is called from inside Load and the temperature hooks, where an escaping exception would reach the game.
/// The file copy is written on a worker (<see cref="LogQueue"/>), so a hook that logs never waits on the disk.
/// </summary>
internal sealed class RulesLog
{
    private const long TrimAboveBytes = 1024 * 1024;
    private const int KeepBytes = 200 * 1024;

    private readonly ManualLogSource _bepinex;
    private readonly string _path;
    private readonly object _gate = new();
    private readonly LogQueue _file;

    public RulesLog(ManualLogSource bepinex, string path)
    {
        _bepinex = bepinex;
        _path = path;
        _file = new LogQueue(Append, work => Task.Run(work));
    }

    public void Write(string message)
    {
        try
        {
            if (HookFailure.Is(message)) _bepinex.LogError(message);
            else _bepinex.LogInfo(message);
        }
        catch (Exception)
        {
            // A throwing log listener costs the BepInEx copy; the hooks log from inside the game's own calls.
        }

        WriteFileOnly(message);
    }

    /// <summary>The mod's file only, for worker threads: the BepInEx console is left to the game's thread.</summary>
    public void WriteFileOnly(string message)
    {
        try
        {
            _file.Add(DateTime.Now, message);
        }
        catch (Exception)
        {
            // Costs the file copy, never the plugin.
        }
    }

    /// <summary>Runs on the worker; the queue drops the lines if it throws.</summary>
    private void Append(string text)
    {
        lock (_gate) File.AppendAllText(_path, text);
    }

    /// <summary>Lines add up over many sessions, so past 1 MB only the newest ~200 KB of whole lines is kept.</summary>
    public void TrimIfLarge()
    {
        lock (_gate)
        {
            try
            {
                var file = new FileInfo(_path);

                if (!file.Exists || file.Length <= TrimAboveBytes) return;

                var bytes = File.ReadAllBytes(_path);
                var cut = bytes.Length - KeepBytes;
                var nextLine = Array.IndexOf(bytes, (byte)'\n', cut) + 1;

                File.WriteAllBytes(_path, bytes[(nextLine > 0 ? nextLine : cut)..]);
            }
            catch (Exception)
            {
                // An oversized log is harmless; failing to trim it must not stop the plugin loading.
            }
        }
    }
}
