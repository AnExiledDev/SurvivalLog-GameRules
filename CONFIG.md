# Game Rules settings

Settings live in `BepInEx\config\com.dev.survivallog.gamerules.cfg`, created the first time the game starts with the mod. Edit the file while the game is closed; changes apply at the next game start. Delete the file to get the defaults back. Every default is the game's own rule.

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

## Pool

| Setting | Default | What it does |
| --- | --- | --- |
| `IndoorAndSunroom` | `false` | Running AC units, heaters and stoves in the Indoor zone or the Sunroom warm both. Off: each warms only its own zone, as in the game. |
| `Terrace` | `false` | The Terrace joins the Indoor zone's pool. The game gives the Terrace no device heat at all; on, it gets the whole pool's device heat, and devices on the Terrace warm the pool. |
| `Basement` | `false` | The Basement joins the Indoor zone's pool: its devices warm the pool and the pool's devices warm the Basement. Off: the Basement keeps its own heat, as in the game. |

The Indoor zone is always the anchor, and each setting adds its zone to it. Cold storage never joins. The greenhouse on the terrace counts as the Sunroom in the game, so `IndoorAndSunroom` covers it, not `Terrace`.

The first time Game Rules starts, it copies these three from Whole Floor Climate's config if that file exists.
