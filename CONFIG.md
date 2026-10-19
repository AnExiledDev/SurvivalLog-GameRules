# Game Rules settings

Settings live in `BepInEx\config\com.dev.survivallog.gamerules.cfg`, created the first time the game starts with the mod. Change them in game on the Mod Manager's Mods screen, or edit the file while the game is closed. Changed on the Mods screen, every setting takes effect at once except `NoRestartLimit`, `BatteryMultiplier`, `StartWithAllAreasOpen` and the `Weather` settings, which take effect at the next game start. Delete the file to get the defaults back. Every default is the game's own rule.

## Restarts

| Setting | Default | What it does |
| --- | --- | --- |
| `NoRestartLimit` | `false` | Restarts never run out: however many you use, the game's full number stays available. |

## Mold

| Setting | Default | What it does |
| --- | --- | --- |
| `StopMoldSpread` | `false` | Mold no longer spreads to the food next to it. Mold that is already there still clears as usual. |

## Solar

| Setting | Default | What it does |
| --- | --- | --- |
| `SolarMultiplier` | `1` | Solar panels make this many times the game's power. 0.1 to 10. |
| `BadWeatherOutput` | `0` | In storms and heavy rain the game's panels make nothing. This gives them a share of a cloudy day's power: 0.5 is half, 1 is a full cloudy day. 0 to 1. The multiplier applies on top. |

## Spoilage

| Setting | Default | What it does |
| --- | --- | --- |
| `SpoilageSpeed` | `1` | How fast food spoils, times the game's speed: 0.5 is half as fast, 2 is twice as fast. 0.1 to 5. A powered fridge that keeps food slower than the room keeps its own speed. Food already lying somewhere picks up the new speed the next time it is moved or stored. |

## Power

| Setting | Default | What it does |
| --- | --- | --- |
| `BatteryMultiplier` | `1` | Batteries hold this many times the game's power. 0.1 to 10. |

## Visitors

| Setting | Default | What it does |
| --- | --- | --- |
| `NoDoorSmasher` | `false` | The angry neighbour's "Midnight Door Pounding" never happens. |
| `NoDebtCollector` | `false` | The loan shark never comes to collect: no warning texts, no "Midnight Visitors", no revenge or stalking nights, no feeding task. Best turned on before the first visit; a visit already under way finishes as usual. |

## Areas

| Setting | Default | What it does |
| --- | --- | --- |
| `StartWithAllAreasOpen` | `false` | Every area you would open with a key or a repair is open from the start, before and after the disaster: the second floor and basement for the Wage Slave and the Student; the second floor, cold storage, garage and both side rooms for the Warehouse Manager. You can walk there and build there right away. The story events that would open them are skipped, along with their rewards. Also opens them in a save already under way. |

## Weather

| Setting | Default | What it does |
| --- | --- | --- |
| `RandomWeather` | `false` | Rolls new weather for the 100-day story from the chances below. Cold waves and the story's fixed-weather days stay as in the game, and the difficulty still makes weather milder or harsher. Endless mode is not changed. Takes effect at the next game start, from the first hour played; the forecast follows it. |
| `WeatherSeed` | `1` | Any whole number. The same number always rolls the same weather; change it for different weather. |
| `ClearChance` | `34` | Share of Clear weather. |
| `CloudyChance` | `28` | Share of Cloudy weather. |
| `RainChance` | `20` | Share of Rainy weather. |
| `HeavyRainChance` | `4` | Share of Heavy Rain. |
| `ContinuousRainChance` | `9` | Share of Continuous Rain. |
| `SmogChance` | `5` | Share of Heavy Smog. |

The chances are shares, 0 to 100 each, so they need not add up to 100; 0 means never. The defaults are the game's own mix once cold waves are set aside. Each stretch of one weather within a day gets one roll, so the weather does not flicker hour to hour.

## Climate

| Setting | Default | What it does |
| --- | --- | --- |
| `IndoorAndSunroom` | `false` | Running AC units, heaters and stoves in the Indoor zone or the Sunroom warm both. Off: each warms only its own zone, as in the game. |
| `Terrace` | `false` | The Terrace shares heat with the Indoor zone. The game gives the Terrace no device heat at all; on, it gets the device heat of every zone sharing with Indoor, and devices on the Terrace warm those zones too. |
| `Basement` | `false` | The Basement shares heat with the Indoor zone: its devices warm the zones sharing with Indoor, and their devices warm the Basement. Off: the Basement keeps its own heat, as in the game. |

The Indoor zone is always the anchor, and each setting adds its zone to it. Cold storage never joins. The greenhouse on the terrace counts as the Sunroom in the game, so `IndoorAndSunroom` covers it, not `Terrace`.

The first time Game Rules starts, it copies these three from Whole Floor Climate's config if that file exists.
