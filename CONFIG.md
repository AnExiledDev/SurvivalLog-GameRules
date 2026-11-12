# Game Rules settings

Settings live in `BepInEx\config\com.dev.survivallog.gamerules.cfg`, which is made the first time the game starts with the mod. Change them in game on the Mod Manager's Mods screen, or edit the file while the game is closed. A setting you change on the Mods screen works right away, except `NoRestartLimit`, `StartWithAllAreasOpen` and the `Weather` settings, which work from the next game start. Delete the file to get the defaults back. Every default leaves the game as it is.

The solar, battery, device power, power cut and Blackout panic settings are now in the Power Manager mod, which carries over your values the first time it starts.

## Restarts

| Setting | Default | What it does |
| --- | --- | --- |
| `NoRestartLimit` | `false` | Restarts never run out. However many you use, you always have the full number left. |

## Mold

| Setting | Default | What it does |
| --- | --- | --- |
| `StopMoldSpread` | `false` | Mold no longer spreads to the food next to it. Mold that is already there still clears as usual. |
| `MoldSprayRadius` | `1` | How many cells around the sprayed cell the Disinfectant Spray also cleans. 0 to 5. |
| `MoldSprayCost` | `1` | Disinfectant Spray uses needed to clean one moldy item. 1 to 5. |
| `MoldyFoodSpoilSpeed` | `1` | How fast moldy food spoils, times the normal speed. 0.2 is about the speed of fresh food. 0.2 to 2. |
| `MoldSpreadInterval` | `1` | Time between mold spreads, times the normal time: 2 is half as often. 0.5 to 4. |
| `MoldSpreadItems` | `2` | The most items mold spreads to each time (severe mold adds more). 1 to 6. |
| `MoldSeedsDay1` | `3` | Items that turn moldy when mold first appears (severe mold adds more). 1 to 10. |

## Spoilage

| Setting | Default | What it does |
| --- | --- | --- |
| `SpoilageSpeed` | `1` | How fast food spoils, times the normal speed: 0.5 is half as fast, 2 is twice as fast. 0.1 to 5. A powered fridge that keeps food fresh longer than the room does keeps its own speed. Food that is already sitting somewhere switches to the new speed the next time you move or store it. |

## Visitors

| Setting | Default | What it does |
| --- | --- | --- |
| `NoDoorSmasher` | `false` | The angry neighbour's "Midnight Door Pounding" never happens. |
| `NoDebtCollector` | `false` | The loan shark never comes to collect: no warning texts, no "Midnight Visitors", no revenge or stalking nights, no feeding task. Best turned on before the first visit. A visit already under way finishes as usual. |

## Areas

| Setting | Default | What it does |
| --- | --- | --- |
| `StartWithAllAreasOpen` | `false` | Every area you would open with a key or a repair is open as soon as the disaster hits: the second floor and basement for the Wage Slave and the Student, and the second floor, cold storage, garage and both side rooms for the Warehouse Manager. You can walk there and build there right away. The story events that would open them are skipped, along with their rewards. Before the disaster they stay locked, because until then you play in a separate copy of the house with no way onto the other floors. It also works on a save that is already past the disaster. |

## Weather

| Setting | Default | What it does |
| --- | --- | --- |
| `RandomWeather` | `false` | Rolls new weather for the 100-day story from the chances below. Cold waves and story days with set weather don't change, and the difficulty still makes weather milder or harsher. Endless mode is not changed. Takes effect at the next game start, from the first hour you play, and the forecast shows the new weather. |
| `WeatherSeed` | `1` | Any whole number. The same number always rolls the same weather. Change it to get different weather. |
| `ClearChance` | `34` | Share of Clear weather. |
| `CloudyChance` | `28` | Share of Cloudy weather. |
| `RainChance` | `20` | Share of Rainy weather. |
| `HeavyRainChance` | `4` | Share of Heavy Rain. |
| `ContinuousRainChance` | `9` | Share of Continuous Rain. |
| `SmogChance` | `5` | Share of Heavy Smog. |

The chances are shares, 0 to 100 each, so they don't have to add up to 100. 0 means never. The defaults match the normal weather, leaving out cold waves. Each stretch of one weather within a day gets one roll, so the weather does not flicker hour to hour.

## Climate

| Setting | Default | What it does |
| --- | --- | --- |
| `IndoorAndSunroom` | `false` | Running AC units, heaters and stoves in the Indoor zone or the Sunroom warm both. Off leaves each one warming only its own zone. |
| `Terrace` | `false` | The Terrace shares heat with the Indoor zone. Normally the Terrace gets no heat from devices at all. With this on, it gets the device heat of every zone sharing with Indoor, and devices on the Terrace warm those zones too. |
| `Basement` | `false` | The Basement shares heat with the Indoor zone: its devices warm the zones sharing with Indoor, and their devices warm the Basement. Off leaves the Basement with its own heat. |

The Indoor zone is always the anchor, and each setting adds its zone to it. Cold storage never joins. The greenhouse on the terrace counts as the Sunroom, so `IndoorAndSunroom` is the setting that covers it.

The first time Game Rules starts, it copies these three from Whole Floor Climate's config if that file exists.

## Food

| Setting | Default | What it does |
| --- | --- | --- |
| `RawFoodSickChance` | `1` | How likely raw food is to make you sick, times the normal chance. 0 is never. 0 to 5. |
| `NoFoodFatigue` | `false` | Eating the same food never causes Appetite Loss, and Appetite Loss you already have clears. |
| `FoodFatigueLength` | `1` | How long Appetite Loss lasts and how much each repeat meal adds to it, times the normal amount. 0.1 to 5. |
| `FoodFatiguePenalty` | `1` | How much Appetite Loss cuts what food gives you, times the normal cut. Normally food gives half. 0 is no cut, and 2 means the food gives nothing. 0 to 2. |

## Rats

| Setting | Default | What it does |
| --- | --- | --- |
| `RatVisitChance` | `1` | Daily chance of a rat visit, times the normal chance, including the stored-food bonus and its cap. 0 is never. 0 to 3. Does not apply during a Rat Infestation. |
| `RatEscapeTime` | `1` | How long a visiting rat stays before it escapes, times the normal time. 0.5 to 5. |

## Sickness

| Setting | Default | What it does |
| --- | --- | --- |
| `NoPlague` | `false` | The Outbreak crisis never comes. If one has already started, it gets no worse and ends like it normally would. |
| `NoPlagueWorsening` | `false` | The Outbreak still comes but never reaches Disease Worsens. |
| `WindChillChance` | `1` | Chance of Wind Chill in a cold wave without heating, times the normal chance. 0 is never. 0 to 2. |
| `FeverChance` | `1` | Chance of a High Temperature in a cold wave without heating, times the normal chance. 0 is never. 0 to 2. |

## House

| Setting | Default | What it does |
| --- | --- | --- |
| `NoStormLeaks` | `false` | Heavy rain never leaks in through the windows (no Heavy Rain event). |

## Cold

| Setting | Default | What it does |
| --- | --- | --- |
| `ColdWaveStrength` | `1` | How cold cold waves, frosty days and cold rain are, times the normal cold, rounded to whole steps. 0 to 3. The weather and forecast are unchanged. Also changes whether strangers you support count the day as cold. |
| `ColdResistBonus` | `0` | Extra levels of Personal Cold Resistance. 0 to 3. |

## Zombies

| Setting | Default | What it does |
| --- | --- | --- |
| `NoStrayZombies` | `false` | Stray zombies never come between hordes. |
| `StrayZombieStartDay` | `7` | The day stray zombies start coming. 1 to 100. |
| `StrayZombieCooldownHours` | `4` | In-game hours between stray zombie visits. 0 to 48. |
| `StrayZombieMaxPerDay` | `3` | Most stray zombie visits in a day. 1 to 10. |
| `StrayZombieMaxAlive` | `3` | Most stray zombies around the house at once. 1 to 20. |
| `StrayZombieLeaderChance` | `1` | Chance a crawler leads a visit, times the normal chance (30% from about day 15 on). 0 to 4. |
| `StrayZombieLoot` | `1` | What a killed stray zombie drops. Normally it drops one item. 1.5 drops one or two, and 0 drops nothing. 0 to 5. |

## Waves

| Setting | Default | What it does |
| --- | --- | --- |
| `NoAdaptiveWaves` | `false` | Story hordes always come at full strength. Normally a weaker horde comes when your defenses are not ready, so this makes hordes harder. |
| `WaveLoot` | `1` | Horde loot for the zombies killed, times the normal amount, limits included. 0.5 to 5. |

## Companions

| Setting | Default | What it does |
| --- | --- | --- |
| `NeverDie` | `false` | The neighbour and your companion never die. When one of them would die, she stays at her weakest instead and can still be rescued. |
| `RescueDeadline` | `21` | The day after the disaster on which the neighbour and your companion die if you have not started helping them. 21 to 120. |
| `DeclineTime` | `1` | How long the neighbour and your companion take to drop one condition level when you send nothing, times the normal 48 game hours. 0.5 to 5. |
| `BasketReturnTime` | `1` | How long the basket takes to come back after you send it, times the normal time (16 to 18 game hours, less when she is weak). 0.5 to 3. |
| `CrisisChance` | `1` | How likely the neighbour is to ask for help with a crisis after two days without a basket, times the normal 25%. 0 to 3. |
| `DislikePenalty` | `1` | Affinity the neighbour and your companion lose when you send food they dislike. 0 to 5. |
| `StruggleCount` | `3` | How many times the neighbour and your companion can pull through when near death, if you send enough food in time. 0 to 10. |
| `ReturnGifts` | `1` | How many gifts the neighbour sends back in the basket, times the normal one gift per 10 value you send. 0.5 to 5. |

## Strangers

| Setting | Default | What it does |
| --- | --- | --- |
| `TrustLoss` | `1` | Trust that strangers you support lose each day when they go hungry or their letters go unanswered, times the normal loss. 0 means trust never drops. 0 to 3. |
| `MishapChance` | `1` | How likely strangers you support are to suffer a mishap each day, times the normal chance. 0 is never. 0 to 3. |
| `NoRatMishap` | `false` | Rats never get into their food. |
| `NoFireMishap` | `false` | A fire never burns their materials. |
| `NoTheftMishap` | `false` | Nobody steals their materials. |
| `NoFacilityMishap` | `false` | Nothing in their shelter breaks. |
| `NoPestMishap` | `false` | Pests never eat their seeds. |
| `NoInjuryMishap` | `false` | They never get hurt and use up medicine. |

## Commissions

| Setting | Default | What it does |
| --- | --- | --- |
| `CommissionChance` | `1` | How likely a new commission is to arrive each day, times the normal chance. 1.4 is close to every day. 0.1 to 1.4. |
| `AidPlatformChance` | `1` | How likely a new request is to appear on the Aid Platform each day, times the normal chance. 1.4 is close to every day. 0.1 to 1.4. |

## Thoughts

| Setting | Default | What it does |
| --- | --- | --- |
| `ThoughtFrequency` | `1` | How often your character thinks out loud and the guide chats, times the normal rate: 2 is twice as often. 0.1 to 4. |

## Morale

| Setting | Default | What it does |
| --- | --- | --- |
| `MoraleGains` | `1` | Morale from finishing a song on the music player and from a perfect harvest, times the normal amount. 0 gives none. 0 to 5. |
| `VaseMoraleCap` | `1` | The limit on morale from vases, times the normal limit. 2 lets twice as many vases count. 0.5 to 5. |

## Crafting

| Setting | Default | What it does |
| --- | --- | --- |
| `FullCraftingXp` | `false` | Recipes below your crafting level still give full proficiency. With it off, recipes one to four levels below yours give less, like they normally do. |

## Skills

| Setting | Default | What it does |
| --- | --- | --- |
| `SkillXp` | `1` | How much proficiency every skill gains, times the normal amount: 2 levels skills twice as fast and 0.5 half as fast. 0 means skills gain nothing. You can use it together with `FullCraftingXp`. 0 to 10. |

## Stamina

| Setting | Default | What it does |
| --- | --- | --- |
| `ActionStaminaCost` | `1` | How much stamina your actions cost (crafting, building, searching, working at furniture and the like), times the normal cost. 0.5 is half the cost, and 0 means actions cost no stamina. Stamina you lose over time or from a buff doesn't change. The game still uses the full cost to check whether you have enough stamina, and shows the full cost. 0 to 3. |

## Stimulants

| Setting | Default | What it does |
| --- | --- | --- |
| `StimulantStrength` | `1` | How much stamina coffee, Sports Drink, Adrenaline and Stimulants give, times the normal amount, including what they keep giving over the next hours. 2 is twice as much, and 0 gives no stamina. Item tooltips still show the normal numbers. 0 to 5. |
| `StimulantDailyLimit` | `0` | How many times a day you can use each kind of coffee, Sports Drink, Adrenaline and Stimulants. Each kind has its own count, so Coffee (Good) and Coffee (Perfect) are counted separately. The count starts over every day. Uses from before you turned it on that day do not count. 0 means no limit. 0 to 10. |

## Stats

| Setting | Default | What it does |
| --- | --- | --- |
| `ExtraMaxSatiety` | `0` | Points added to your maximum satiety. At 50 you can eat 50 more before you are full. A negative number lowers it. -50 to 400. |
| `ExtraMaxMorale` | `0` | Points added to your maximum morale. A negative number lowers it. -50 to 400. |

When you lower a maximum, a satiety or morale above the new maximum comes down to it the next time that stat changes.

## Reach

| Setting | Default | What it does |
| --- | --- | --- |
| `PickupReach` | `1` | How far away you can pick up dropped items, times the normal distance. 1 to 5. |
