using System;
using System.Collections.Generic;

namespace GameRules.Core;

/// <summary>
/// Remembers the game's answers for the rest of a frame. The game asks a zone's temperature dozens of times a frame,
/// and each pooled answer reads every other pool zone; a device's heat and an AC's power do not change within a frame,
/// so each zone is read from the game once per frame.
/// </summary>
public sealed class FrameClimate : IGameClimate
{
    private readonly IGameClimate _game;
    private readonly Func<int> _frame;
    private readonly Dictionary<int, int> _heat = new();
    private readonly Dictionary<int, bool> _acPowered = new();
    private readonly Dictionary<int, bool?> _takesHeat = new();
    private int _cachedFrame = int.MinValue;

    public FrameClimate(IGameClimate game, Func<int> frame)
    {
        _game = game;
        _frame = frame;
    }

    public int DeviceHeatIn(int zone)
    {
        lock (_heat)
        {
            ForgetOlderFrames();

            if (!_heat.TryGetValue(zone, out var heat)) _heat[zone] = heat = _game.DeviceHeatIn(zone);

            return heat;
        }
    }

    public bool? TakesDeviceHeat(int zone)
    {
        lock (_heat)
        {
            ForgetOlderFrames();

            if (!_takesHeat.TryGetValue(zone, out var takes)) _takesHeat[zone] = takes = _game.TakesDeviceHeat(zone);

            return takes;
        }
    }

    public bool IsAcPoweredIn(int zone)
    {
        lock (_heat)
        {
            ForgetOlderFrames();

            if (!_acPowered.TryGetValue(zone, out var isPowered)) _acPowered[zone] = isPowered = _game.IsAcPoweredIn(zone);

            return isPowered;
        }
    }

    private void ForgetOlderFrames()
    {
        var frame = _frame();

        if (frame == _cachedFrame) return;

        _cachedFrame = frame;
        _heat.Clear();
        _acPowered.Clear();
        _takesHeat.Clear();
    }
}
