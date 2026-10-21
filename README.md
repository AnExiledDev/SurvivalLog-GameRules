# Game Rules

Change some of Survival Log's own rules: restarts, mold, solar power, batteries, food spoilage, night visitors, locked areas, the weather and how far heaters reach.

Every setting starts at the game's own value, so with a fresh config the mod changes nothing. Turn on only what you want.

## Features

- **No restart limit**: restarts never run out. However many you use, the game's full number stays available, so the restart screen still shows a normal row of marks.
- **Stop mold spreading**: mold no longer jumps to the food next to it. Mold that is already there still clears as usual.
- **Solar multiplier**: solar panels make more (or less) power, from 0.1x to 10x.
- **Solar in bad weather**: in storms and heavy rain the game's panels make nothing. This gives them a share of a cloudy day's power instead.
- **Spoilage speed**: food spoils slower or faster, from 0.1x to 5x.
- **Battery multiplier**: batteries hold more (or less) power, from 0.1x to 10x.
- **No door-smasher**: the angry neighbour's "Midnight Door Pounding" never happens.
- **No debt collector**: the loan shark's night visits, warning texts, revenge and stalking nights never happen.
- **Start with all areas open**: the second floor and basement (Wage Slave, Student), or the second floor, cold storage, garage and both side rooms (Warehouse Manager), open as soon as the disaster hits. You can walk there and build there right away. Works on a save already past the disaster, and on one from before it once the disaster comes.
- **Random weather**: the weather is rolled from chances you set, for example less rain late in the game. Cold waves and the game's fixed story days keep the game's weather.
- **Whole-floor heat**: running heaters, AC units and stoves can warm the Indoor zone and the Sunroom together, and the Terrace and the Basement can join. This is the Whole Floor Climate mod, moved in here.
- **Log file**: the mod writes what it changed, and any failure, to `BepInEx\game-rules.log`.

Game Rules adds no data of its own to your save, and you can remove it at any time. Areas opened by **Start with all areas open** stay open in that save after the mod is removed, the same as areas opened in the game.

## Known issues and limits

- **Spoilage speed and fridges**: a powered fridge that keeps food slower than the room keeps its own speed. Food already lying somewhere picks up the new speed the next time it is moved or stored.
- **Solar in bad weather** has no effect at night, and none on a panel that no light reaches.
- **Random weather** changes the weather from the next hour on, and the forecast shows the new weather. The same seed always rolls the same weather, so a reload keeps it. Turning it off brings back the game's weather after a restart of the game.
- **No debt collector** works best turned on before the loan shark's first visit. A visit already under way finishes as usual.
- **Start with all areas open** skips the story events that would open those areas (finding the keys, clearing the stairs and the cellar), so their rewards and story are skipped too. An event already under way in a loaded save stays in the event list. Opening both floors may unlock the matching achievement. Building spots unlocked with talents are not changed.
- **Start with all areas open** only opens areas after the disaster. Before the disaster, the house you play in is technically a separate map from the one after it, and that map has no way onto the other floors or into the locked rooms. Making them work there would be a monumental effort, so the areas open at the start of the post-disaster game instead. Before the disaster they stay locked, the same as in the game.
- **Whole-floor heat** makes rooms warm up more easily, so warm-room progress is easier to reach.

## Moving from Whole Floor Climate

The first time it starts, Game Rules copies your heat sharing settings from Whole Floor Climate's config (its `[Pool]` section) into its own `[Climate]` section. Then delete `WholeFloorClimate.dll` from `BepInEx\plugins`. While it is still installed, Game Rules leaves the heat to it so nothing is counted twice, and says so in its log.

## Requirements

- Survival Log 1.1.18902. Built and tested on this version; a game update can break it until the mod is updated.
- The [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12), the BepInEx 6 (IL2CPP) build for the game.
- Optional: [Exiled's Toolkit](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit) 0.4.0 or newer. With it, settings changed on the Mod Manager's Mods screen take effect without restarting the game. Without it, they take effect at the next game start.

## Install

1. Install the [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12) (if no other mods were installed before, start the game once so BepInEx finishes setup, then quit).
2. Optional, to change settings without a restart: install Exiled's Toolkit. Download `ExiledToolkit-x.y.z.zip` from [its latest release](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit/releases/latest) or from [Nexus Mods](https://www.nexusmods.com/survivallog/mods/61), and extract it into the game folder (the folder with `SurvivalLog.exe`).
3. Download `GameRules-x.y.z.zip` from the [latest release](https://github.com/AnExiledDev/SurvivalLog-GameRules/releases/latest) or from Nexus Mods.
4. Extract the zip into the game folder (the folder with `SurvivalLog.exe`). When asked, merge the folders. The result should be:
   `C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\GameRules.dll`

## Uninstall

Delete `GameRules.dll` from the `BepInEx\plugins` folder. For a clean slate, also delete `BepInEx\config\com.dev.survivallog.gamerules.cfg` and `BepInEx\game-rules.log`.

## Configuration

Settings live in `BepInEx\config\com.dev.survivallog.gamerules.cfg`, created the first time the game starts with the mod. Change them in game on the Mod Manager's Mods screen, or edit the file while the game is closed. Changed on the Mods screen, every setting takes effect at once except `NoRestartLimit`, `BatteryMultiplier`, `StartWithAllAreasOpen` and the `Weather` settings, which take effect at the next game start. Each setting in the cfg file is explained in English and Simplified Chinese. See [CONFIG.md](CONFIG.md) for every setting. Without Exiled's Toolkit installed, changes made on the Mods screen take effect at the next game start instead.

## Troubleshooting

Check `BepInEx\LogOutput.log` for `===== Game Rules 0.4.2 started`, followed by lines saying what each rule does. A setting changed on the Mods screen writes its line again. The same lines are in `BepInEx\game-rules.log`, which survives restarts and trims itself past 1 MB. A line starting `could not hook` or `keep the game's` means that rule is off and the game's own numbers are used.

## Feedback and bug reports

Found a bug, have an idea, or something feels off? Come say so on the Core Directive Discord: https://discord.gg/trTsyxUFqg . You can also post in the Posts tab of the mod's Nexus Mods page. When reporting a bug, attach `BepInEx\LogOutput.log` and `BepInEx\game-rules.log`.

To reach me directly, message `exiled.dev` on Discord; you can find me in the [official Survival Log Discord](https://discord.gg/MZGg4gQAUe). I'm also on X as [@AnExiledDev](https://x.com/AnExiledDev).

A Survival Log wiki with detailed documentation for all of my mods is coming soon.

## Build

This is a BepInEx 6 IL2CPP plugin. It compiles against BepInEx and the game's IL2CPP interop assemblies, so a game install with BepInEx set up and started once is required. The .NET 8 SDK is required.

```
dotnet build src/GameRules -c Release -p:GameDir="C:\path\to\Survival Log"
```

The unit tests cover the rules themselves and need nothing from the game:

```
dotnet test tests/GameRules.Tests
```

## License

MIT. Copyright (c) 2026 AnExiledDev. See [LICENSE](LICENSE).
