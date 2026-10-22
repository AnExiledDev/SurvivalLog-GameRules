# Game Rules settings

Settings live in `BepInEx\config\com.dev.survivallog.gamerules.cfg`, created the first time the game starts with the mod. Change them in game on the Mod Manager's Mods screen, or edit the file while the game is closed. Changed on the Mods screen, every setting takes effect at once except `NoRestartLimit`, `BatteryMultiplier`, `StartWithAllAreasOpen` and the `Weather` settings, which take effect at the next game start. Delete the file to get the defaults back. Every default is the game's own rule. Without Exiled's Toolkit installed, changes made on the Mods screen take effect at the next game start instead.

## Restarts

| Setting | Default | What it does |
| --- | --- | --- |
| `NoRestartLimit` | `false` | Restarts never run out: however many you use, the game's full number stays available. |

## Mold

| Setting | Default | What it does |
| --- | --- | --- |
| `StopMoldSpread` | `false` | Mold no longer spreads to the food next to it. Mold that is already there still clears as usual. |
| `MoldSprayRadius` | `1` | How many cells around the sprayed cell the Disinfectant Spray also cleans. 0 to 5. |
| `MoldSprayCost` | `1` | Disinfectant Spray uses needed to clean one moldy item. 1 to 5. |
| `MoldyFoodSpoilSpeed` | `1` | How fast moldy food spoils, times the game's speed. 0.2 is about the speed of fresh food. 0.2 to 2. |
| `MoldSpreadInterval` | `1` | Time between mold spreads, times the game's time: 2 is half as often. 0.5 to 4. |
| `MoldSpreadItems` | `2` | The most items mold spreads to each time (severe mold adds more). 1 to 6. |
| `MoldSeedsDay1` | `3` | Items that turn moldy when mold first appears (severe mold adds more). 1 to 10. |

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
| `DevicePowerUse` | `1` | Power that switched-on devices use, times the game's. 0 to 3. Cooking and shredding jobs, the compressor, defenses firing and the signal station are unchanged. |
| `PowerCutDelay` | `0` | Days later than in the game that the city power is cut. 0 to 100. |
| `NoPowerCut` | `false` | The city power is never cut. The events that follow the cut, and the diesel generator start, never come. A cut that already happened is not undone. |
| `NoBlackoutPanic` | `false` | Power cuts at night no longer bring Blackout panic; a panic already running clears within ten in-game minutes. |

## Visitors

| Setting | Default | What it does |
| --- | --- | --- |
| `NoDoorSmasher` | `false` | The angry neighbour's "Midnight Door Pounding" never happens. |
| `NoDebtCollector` | `false` | The loan shark never comes to collect: no warning texts, no "Midnight Visitors", no revenge or stalking nights, no feeding task. Best turned on before the first visit; a visit already under way finishes as usual. |

## Areas

| Setting | Default | What it does |
| --- | --- | --- |
| `StartWithAllAreasOpen` | `false` | Every area you would open with a key or a repair is open as soon as the disaster hits: the second floor and basement for the Wage Slave and the Student; the second floor, cold storage, garage and both side rooms for the Warehouse Manager. You can walk there and build there right away. The story events that would open them are skipped, along with their rewards. Before the disaster they stay locked: the pre-disaster house is a separate map with no way onto the other floors. Also opens them in a save already past the disaster. |

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

## Food

| Setting | Default | What it does |
| --- | --- | --- |
| `RawFoodSickChance` | `1` | Chance that eating raw food makes you ill, times the game's. 0 is never. 0 to 5. |
| `NoFoodFatigue` | `false` | Eating the same food never causes Appetite Loss, and Appetite Loss you already have clears. |
| `FoodFatigueLength` | `1` | How long Appetite Loss lasts and how much each repeat meal adds to it, times the game's. 0.1 to 5. |
| `FoodFatiguePenalty` | `1` | How much Appetite Loss cuts what a food gives, times the game's cut (half). 0 is no cut, 2 means the food gives nothing. 0 to 2. |

## Rats

| Setting | Default | What it does |
| --- | --- | --- |
| `RatVisitChance` | `1` | Daily chance of a rat visit, times the game's, including the stored-food bonus and its cap. 0 is never. 0 to 3. Does not apply during a Rat Infestation. |
| `RatEscapeTime` | `1` | How long a visiting rat stays before it escapes, times the game's time. 0.5 to 5. |

## Sickness

| Setting | Default | What it does |
| --- | --- | --- |
| `NoPlague` | `false` | The Outbreak crisis never comes. One already running gets no worse and still ends. |
| `NoPlagueWorsening` | `false` | The Outbreak still comes but never reaches Disease Worsens. |
| `WindChillChance` | `1` | Chance of Wind Chill in a cold wave without heating, times the game's. 0 is never. 0 to 2. |
| `FeverChance` | `1` | Chance of a High Temperature in a cold wave without heating, times the game's. 0 is never. 0 to 2. |

## House

| Setting | Default | What it does |
| --- | --- | --- |
| `NoStormLeaks` | `false` | Heavy rain never leaks in through the windows (no Heavy Rain event). |

## Cold

| Setting | Default | What it does |
| --- | --- | --- |
| `ColdWaveStrength` | `1` | How cold cold waves, frosty days and cold rain are, times the game's, rounded to whole steps. 0 to 3. The weather and forecast are unchanged. Also changes whether strangers you support count the day as cold. |
| `ColdResistBonus` | `0` | Extra levels of Personal Cold Resistance. 0 to 3. |

## Zombies

| Setting | Default | What it does |
| --- | --- | --- |
| `NoStrayZombies` | `false` | Stray zombies never come between hordes. |
| `StrayZombieStartDay` | `7` | The day stray zombies start coming. 1 to 100. |
| `StrayZombieCooldownHours` | `4` | In-game hours between stray zombie visits. 0 to 48. |
| `StrayZombieMaxPerDay` | `3` | Most stray zombie visits in a day. 1 to 10. |
| `StrayZombieMaxAlive` | `3` | Most stray zombies around the house at once. 1 to 20. |
| `StrayZombieLeaderChance` | `1` | Chance a crawler leads a visit, times the game's 30% from about day 15. 0 to 4. |
| `StrayZombieLoot` | `1` | What a killed stray zombie drops, times the game's one item. 0 drops nothing; 1.5 gives one or two. 0 to 5. |

## Waves

| Setting | Default | What it does |
| --- | --- | --- |
| `NoAdaptiveWaves` | `false` | Story hordes always come at full strength. The game would otherwise send a weaker horde to an unready house, so this makes hordes harder. |
| `WaveLoot` | `1` | Horde loot for the zombies killed, times the game's, limits included. 0.5 to 5. |

## Companions

| Setting | Default | What it does |
| --- | --- | --- |
| `NeverDie` | `false` | The neighbour and your companion never die. When the game would let one die, she stays at her weakest instead and can still be rescued. |
| `RescueDeadline` | `21` | The day after the disaster on which the neighbour and your companion die if you have not started helping them. 21 to 120. |

## Strangers

| Setting | Default | What it does |
| --- | --- | --- |
| `TrustLoss` | `1` | Trust that strangers you support lose each day when they go hungry or their letters go unanswered, times the game's. 0 means trust never drops. 0 to 3. |
| `MishapChance` | `1` | How likely strangers you support are to suffer a mishap each day, times the game's chance. 0 is never. 0 to 3. |
| `NoRatMishap` | `false` | Rats never get into their food. |
| `NoFireMishap` | `false` | A fire never burns their materials. |
| `NoTheftMishap` | `false` | Nobody steals their materials. |
| `NoFacilityMishap` | `false` | Nothing in their shelter breaks. |
| `NoPestMishap` | `false` | Pests never eat their seeds. |
| `NoInjuryMishap` | `false` | They never get hurt and use up medicine. |

## Commissions

| Setting | Default | What it does |
| --- | --- | --- |
| `CommissionChance` | `1` | How likely a new commission is to arrive each day, times the game's chance. 1.4 is close to every day. 0.1 to 1.4. |
| `AidPlatformChance` | `1` | How likely a new request is to appear on the Aid Platform each day, times the game's chance. 1.4 is close to every day. 0.1 to 1.4. |

## Thoughts

| Setting | Default | What it does |
| --- | --- | --- |
| `ThoughtFrequency` | `1` | How often your character thinks out loud and the guide chats, times the game's rate: 2 is twice as often. 0.1 to 4. |

## Morale

| Setting | Default | What it does |
| --- | --- | --- |
| `MoraleGains` | `1` | Morale from finishing a song on the music player and from a perfect harvest, times the game's. 0 gives none. 0 to 5. |
| `VaseMoraleCap` | `1` | The limit on morale from vases, times the game's: 2 lets twice as many vases count. 0.5 to 5. |

## Crafting

| Setting | Default | What it does |
| --- | --- | --- |
| `FullCraftingXp` | `false` | Recipes below your crafting level still give full proficiency. Off: the game gives less for recipes one to four levels below yours. |

## Reach

| Setting | Default | What it does |
| --- | --- | --- |
| `PickupReach` | `1` | How far away you can pick up dropped items, times the game's distance. 1 to 5. |
