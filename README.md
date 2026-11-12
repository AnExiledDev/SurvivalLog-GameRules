# Game Rules

Change some of Survival Log's rules: restarts, food and mold, rats and sickness, the house, the cold, stray zombies and hordes, night visitors, the neighbour, strangers you support, morale, skill XP, stamina, stimulants, locked areas, the weather and how far heaters reach.

Every setting starts at the game's value, so with a fresh config the mod changes nothing. Change only what you want.

The solar, battery, device power, power cut and Blackout panic settings are now in the Power Manager mod, which carries over your values the first time it starts.

## Features

- **No restart limit**: restarts never run out. However many you use, the game's full number stays available, so the restart screen still shows a normal row of marks.
- **Stop mold spreading**: mold no longer jumps to the food next to it. Mold that is already there still clears as usual.
- **Spoilage speed**: food spoils slower or faster, from 0.1x to 5x.
- **No door-smasher**: the angry neighbour's "Midnight Door Pounding" never happens.
- **No debt collector**: the loan shark's night visits, warning texts, revenge and stalking nights never happen.
- **Start with all areas open**: the second floor and basement (Wage Slave, Student), or the second floor, cold storage, garage and both side rooms (Warehouse Manager), open as soon as the disaster hits. You can walk there and build there right away. It works on a save that is already past the disaster too, and on an earlier save once the disaster hits.
- **Random weather**: the weather is rolled from chances you set, for example less rain late in the game. Cold waves and story days with set weather don't change.
- **Whole-floor heat**: running heaters, AC units and stoves can warm the Indoor zone and the Sunroom together, and the Terrace and the Basement can join. This used to be the Whole Floor Climate mod.
- **Food**: how likely raw food is to make you sick. You can also turn off the Appetite Loss you get from eating the same food, or make it shorter, longer, milder or harsher.
- **Mold**: Disinfectant Spray radius and cost, how fast moldy food spoils, how often mold spreads, how many items it reaches, and how many items start moldy.
- **Rats**: how often rats visit, and how long a rat stays before it escapes.
- **Sickness**: turn off the Outbreak crisis, or only its worsening, and set the Wind Chill and High Temperature chances in a cold wave.
- **The house**: no heavy rain leaks.
- **Cold**: how cold cold waves are, and extra levels of Personal Cold Resistance.
- **Stray zombies**: turn them off, or set their start day, the time between visits, visits per day, how many at once, how often a crawler leads them, and what they drop.
- **Hordes**: story hordes can always come at full strength, and you can set how much loot hordes leave.
- **Neighbour and companion**: they never die, or the rescue deadline comes later. They can also drop condition more slowly, pull through near death more often, and lose less affinity for food they dislike. The basket can come back sooner with more gifts, and the neighbour's crises can come less often.
- **Strangers you support**: how much trust they lose, how often mishaps hit them, and which mishaps can happen.
- **Commissions**: how often new commissions and Aid Platform requests arrive.
- **Thoughts, morale, crafting and reach**: how often your character thinks out loud, morale from music and perfect harvests, the vase morale limit, full crafting proficiency from lower-level recipes, and how far away you can pick up items.
- **Skills and stamina**: how much proficiency skills gain, and how much stamina actions cost.
- **Stimulants**: how much stamina coffee, Sports Drink, Adrenaline and Stimulants give, and how many times a day you can use each one.
- **Satiety and morale**: raise or lower your maximum satiety and morale.
- **Log file**: the mod writes what it changed, and any failure, to `BepInEx\game-rules.log`.

Game Rules adds no data of its own to your save, and you can remove it at any time. Areas opened by **Start with all areas open** stay open in that save after the mod is removed, just like areas you open yourself.

## Known issues and limits

- **Spoilage speed and fridges**: a powered fridge that keeps food fresh longer than the room does keeps its own speed. Food that is already sitting somewhere switches to the new speed the next time you move or store it.
- **Random weather** changes the weather from the next hour on, and the forecast shows the new weather. The same seed always rolls the same weather, so a reload keeps it. Turn it off and restart the game to get the usual weather back.
- **No debt collector** works best turned on before the loan shark's first visit. A visit already under way finishes as usual.
- **Start with all areas open** skips the story events that would open those areas (finding the keys, clearing the stairs and the cellar), so their rewards and story are skipped too. An event already under way in a loaded save stays in the event list. Opening both floors may unlock the matching achievement. Building spots unlocked with talents are not changed.
- **Start with all areas open** only opens areas after the disaster. Before the disaster you play in a separate copy of the house, and that copy has no way onto the other floors or into the locked rooms. So the areas open when the disaster hits, and before that they stay locked like they normally are.
- **Whole-floor heat** makes rooms warm up more easily, so anything that needs a warm room is easier to reach.
- **Rat visit chance** does not apply during a Rat Infestation.
- **Cold wave strength** also changes whether strangers you support count the day as cold.
- **Action stamina cost**: the game still uses the full cost to check whether you have enough stamina for an action, and shows the full cost. Buffs that cut stamina use, and the crash after Adrenaline, work like they normally do.
- **Stimulant strength**: item tooltips still show the normal numbers.
- **Stimulant daily limit**: each kind of stimulant has its own count, and uses from before you turned the limit on that day do not count.
- **Maximum satiety and morale**: when you lower a maximum, a satiety or morale above it comes down the next time that stat changes.
- **With Vital Signs**: its `StaminaDrain` multiplies with `ActionStaminaCost`, and its `StaminaGain` with `StimulantStrength`. At 0.5 in both, an action costs a quarter of the normal stamina.

## Moving from Whole Floor Climate

The first time it starts, Game Rules copies your heat sharing settings from Whole Floor Climate's config (its `[Pool]` section) into its own `[Climate]` section. Then delete `WholeFloorClimate.dll` from `BepInEx\plugins`. While it is still installed, Game Rules leaves the heat to it so nothing is counted twice, and says so in its log.

## Requirements

- Survival Log 1.1.20719. It is built for this version, and a game update can break it until the mod is updated.
- The [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12), the BepInEx 6 (IL2CPP) build for the game.
- [Exiled's Toolkit](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit) 0.20.0 or newer. Game Rules does not load without it. It applies settings changed on the Mod Manager's Mods screen without a restart, and it lets each world keep its own rules.
- Optional: [Mod Manager](https://www.nexusmods.com/survivallog/mods/73). With it, you can change every setting in game on its Mods screen and see what each one does, without opening the config file.

## Install

1. Install the [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12) (if no other mods were installed before, start the game once so BepInEx finishes setup, then quit).
2. Install Exiled's Toolkit. Download `ExiledToolkit-x.y.z.zip` from [its latest release](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit/releases/latest) or from [Nexus Mods](https://www.nexusmods.com/survivallog/mods/61), and extract it into the game folder (the folder with `SurvivalLog.exe`).
3. Download `GameRules-x.y.z.zip` from the [latest release](https://github.com/AnExiledDev/SurvivalLog-GameRules/releases/latest) or from [Nexus Mods](https://www.nexusmods.com/survivallog/mods/84).
4. Extract the zip into the game folder (the folder with `SurvivalLog.exe`). When asked, merge the folders. The result should be:
   `C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\GameRules.dll`

## Uninstall

Delete `GameRules.dll` from the `BepInEx\plugins` folder. For a clean slate, also delete `BepInEx\config\com.dev.survivallog.gamerules.cfg`, `BepInEx\game-rules.log` and `BepInEx\game-rules.log.1` if it is there.

## Configuration

Settings live in `BepInEx\config\com.dev.survivallog.gamerules.cfg`, which is made the first time the game starts with the mod. Change them in game on the Mod Manager's Mods screen, or edit the file while the game is closed. A setting you change on the Mods screen works right away, except `NoRestartLimit`, `StartWithAllAreasOpen` and the `Weather` settings, which work from the next game start. Each setting in the cfg file is explained in English and Simplified Chinese. See [CONFIG.md](CONFIG.md) for every setting. Each world can keep its own value for every setting that works right away. Pick them for a new world in Customizable Difficulty, or change them for the world you are playing on the Mods screen.

## Troubleshooting

Check `BepInEx\LogOutput.log` for `===== Game Rules 0.9.1 started`, followed by lines saying what each rule does. A setting changed on the Mods screen writes its line again. The same lines are in `BepInEx\game-rules.log`, which survives restarts. Past 1 MB it is moved to `game-rules.log.1` and started again. A line starting `could not hook` means that rule is off and the game works as normal. A line starting `failed` means one check went wrong, and the normal numbers were used that time.

## Feedback and bug reports

Found a bug, have an idea, or something feels off? Come say so on the Core Directive Discord: https://discord.gg/trTsyxUFqg . You can also post in the Posts tab of the mod's Nexus Mods page. When reporting a bug, attach `BepInEx\LogOutput.log`, `BepInEx\game-rules.log` and `BepInEx\game-rules.log.1` if it is there.

To reach me directly, message `exiled.dev` on Discord; you can find me in the [official Survival Log Discord](https://discord.gg/MZGg4gQAUe). I'm also on X as [@AnExiledDev](https://x.com/AnExiledDev).

A Survival Log wiki with detailed documentation for all of my mods is coming soon.

## Build

This is a BepInEx 6 IL2CPP plugin. It compiles against BepInEx, the game's IL2CPP interop assemblies and Exiled's Toolkit, so a game install with BepInEx set up and started once is required. The .NET 8 SDK is required.

```
dotnet build src/GameRules -c Release -p:GameDir="C:\path\to\Survival Log" -p:ToolkitDll="C:\path\to\ExiledToolkit.dll"
```

The unit tests cover the rules themselves and need nothing from the game:

```
dotnet test tests/GameRules.Tests
```

## License

MIT. Copyright (c) 2026 AnExiledDev. See [LICENSE](LICENSE).
