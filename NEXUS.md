Name: Game Rules
Summary: Change Survival Log's own rules: no restart limit, slower spoilage, stronger solar and batteries, milder mold, rats, sickness and cold, no power cut, fewer stray zombies, a neighbour who never dies and more. About 70 settings for food, power, zombies, visitors, morale and weather. Every setting starts at the game's own value, so turn on only what you want.

Description (BBCode):
Change some of Survival Log's own rules: restarts, food and mold, rats and sickness, power and the house, the cold, stray zombies and hordes, night visitors, the neighbour, strangers you support, morale, locked areas, the weather and how far heaters reach.

Every setting starts at the game's own value, so with a fresh config the mod changes nothing. Turn on only what you want.

[size=5]Main features[/size]
[list]
[*][b]No restart limit[/b]: restarts never run out. However many you use, the game's full number stays available, so the restart screen still shows a normal row of marks.[/*]
[*][b]Stop mold spreading[/b]: mold no longer jumps to the food next to it. Mold that is already there still clears as usual.[/*]
[*][b]Solar multiplier[/b]: solar panels make more (or less) power, from 0.1x to 10x.[/*]
[*][b]Solar in bad weather[/b]: in storms and heavy rain the game's panels make nothing. This gives them a share of a cloudy day's power instead.[/*]
[*][b]Spoilage speed[/b]: food spoils slower or faster, from 0.1x to 5x.[/*]
[*][b]Battery multiplier[/b]: batteries hold more (or less) power, from 0.1x to 10x.[/*]
[*][b]No door-smasher[/b]: the angry neighbour's "Midnight Door Pounding" never happens.[/*]
[*][b]No debt collector[/b]: the loan shark's night visits, warning texts, revenge and stalking nights never happen.[/*]
[*][b]Start with all areas open[/b]: the second floor and basement (Wage Slave, Student), or the second floor, cold storage, garage and both side rooms (Warehouse Manager), open as soon as the disaster hits. You can walk there and build there right away. Works on a save already past the disaster, and on one from before it once the disaster comes.[/*]
[*][b]Random weather[/b]: the weather is rolled from chances you set, for example less rain late in the game. Cold waves and the game's fixed story days keep the game's weather.[/*]
[*][b]Food[/b]: raw food sickness chance, and Appetite Loss from eating the same food: turn it off, or make it shorter, longer, milder or harsher.[/*]
[*][b]Mold[/b]: Disinfectant Spray radius and cost, how fast moldy food spoils, how often mold spreads, how many items it reaches, and how many items start moldy.[/*]
[*][b]Rats[/b]: how often rats visit, and how long a rat stays before it escapes.[/*]
[*][b]Sickness[/b]: turn off the Outbreak crisis, or only its worsening; set the Wind Chill and High Temperature chances in a cold wave.[/*]
[*][b]Power and the house[/b]: how much power devices use, a later city power cut or none at all, no Blackout panic, and no heavy rain leaks.[/*]
[*][b]Cold[/b]: how cold cold waves are, and extra levels of Personal Cold Resistance.[/*]
[*][b]Stray zombies[/b]: turn them off, or set their start day, the time between visits, visits per day, how many at once, how often a crawler leads them, and what they drop.[/*]
[*][b]Hordes[/b]: story hordes always at full strength, and more or less horde loot.[/*]
[*][b]Neighbour and companion[/b]: they never die, or the rescue deadline comes later.[/*]
[*][b]Strangers you support[/b]: how much trust they lose, how often mishaps hit them, and which mishaps can happen.[/*]
[*][b]Commissions[/b]: how often new commissions and Aid Platform requests arrive.[/*]
[*][b]Thoughts, morale, crafting and reach[/b]: how often your character thinks out loud, morale from music and perfect harvests, the vase morale limit, full crafting proficiency from lower-level recipes, and how far away you can pick up items.[/*]
[*][b]Whole-floor heat[/b]: running heaters, AC units and stoves can warm the Indoor zone and the Sunroom together, and the Terrace and the Basement can join. This is the Whole Floor Climate mod, moved in here. Its settings are copied over the first time Game Rules starts; then delete [font=Courier New]WholeFloorClimate.dll[/font].[/*]
[/list]

Game Rules adds no data of its own to your save, and you can remove it at any time. Areas opened by [b]Start with all areas open[/b] stay open in that save after the mod is removed, the same as areas opened in the game.

Works in English and Chinese.

[size=5]Known issues[/size]
[list]
[*][b]Start with all areas open[/b] only opens areas after the disaster. Before the disaster, the house you play in is technically a separate map from the one after it, and that map has no way onto the other floors or into the locked rooms. Making them work there would be a monumental effort, so the areas open at the start of the post-disaster game instead.[/*]
[*][b]Start with all areas open[/b] skips the story events that would open those areas (finding the keys, clearing the stairs and the cellar), so their rewards and story are skipped too. An event already under way in a loaded save stays in the event list. Opening both floors may unlock the matching achievement.[/*]
[*][b]Spoilage speed[/b]: a powered fridge that keeps food slower than the room keeps its own speed. Food already lying somewhere picks up the new speed the next time it is moved or stored.[/*]
[*][b]Solar in bad weather[/b] has no effect at night, and none on a panel that no light reaches.[/*]
[*][b]Random weather[/b] changes the weather from the next hour on, and the forecast shows the new weather. The same seed always rolls the same weather, so a reload keeps it.[/*]
[*][b]No debt collector[/b] works best turned on before the loan shark's first visit. A visit already under way finishes as usual.[/*]
[*][b]Rat visit chance[/b] does not apply during a Rat Infestation.[/*]
[*][b]Device power use[/b] does not change cooking and shredding jobs, the compressor, defenses firing or the signal station.[/*]
[*][b]No power cut[/b] also means the events that follow the cut, and the diesel generator start, never come. A cut that already happened is not undone.[/*]
[*][b]Cold wave strength[/b] also changes whether strangers you support count the day as cold.[/*]
[/list]

[size=5]Requirements[/size]
[list]
[*]Survival Log 1.1.19131. A game update can break it until the mod is updated.[/*]
[*][url=https://www.nexusmods.com/survivallog/mods/12]BepInEx for Survival Log[/url]. Install it first.[/*]
[*]Optional: [url=https://www.nexusmods.com/survivallog/mods/61]Exiled's Toolkit[/url] 0.4.0 or newer. With it, settings changed on the Mod Manager's Mods screen take effect without restarting the game. Without it, they take effect at the next game start.[/*]
[/list]

[size=5]Installation[/size]
[list=1]
[*]Install [url=https://www.nexusmods.com/survivallog/mods/12]BepInEx[/url] [i](if no other mods were installed before, start the game once so BepInEx finishes setup, then quit)[/i].[/*]
[*]Optional, to change settings without a restart: install Exiled's Toolkit the same way. Download its zip and extract it into the game folder.[/*]
[*]Download the zip from the Files tab.[/*]
[*]Extract it into the game folder (the folder with [font=Courier New]SurvivalLog.exe[/font]). The zip holds a BepInEx folder, so the DLL lands in the right place. When asked, merge the folders. The result should be:
[code]C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\GameRules.dll[/code][/*]
[/list]

[size=5]Uninstall[/size]
Delete [font=Courier New]GameRules.dll[/font] from the [font=Courier New]BepInEx\plugins[/font] folder.

[size=5]Configuration[/size]
To change settings, edit [font=Courier New]BepInEx\config\com.dev.survivallog.gamerules.cfg[/font] while the game is closed, or do it in game on the Mod Manager's Mods screen, where changes take effect at once, all but [font=Courier New]NoRestartLimit[/font], [font=Courier New]BatteryMultiplier[/font], [font=Courier New]StartWithAllAreasOpen[/font] and the [font=Courier New]Weather[/font] settings, which take effect at the next game start (see [url=https://github.com/AnExiledDev/SurvivalLog-GameRules/blob/main/CONFIG.md]CONFIG.md[/url] for every setting; the cfg file explains each setting in English and Chinese). Without Exiled's Toolkit installed, changes made on the Mods screen take effect at the next game start instead.

[size=5]Feedback[/size]
Found a bug or have an idea? Come say hi on the [url=https://discord.gg/trTsyxUFqg]Core Directive Discord[/url], or post in this page's Posts tab. For bugs, please include [font=Courier New]BepInEx\LogOutput.log[/font] and [font=Courier New]BepInEx\game-rules.log[/font].
To reach me directly, message [b]exiled.dev[/b] on Discord; you can find me in the [url=https://discord.gg/MZGg4gQAUe]official Survival Log Discord[/url]. I'm also on X as [url=https://x.com/AnExiledDev]@AnExiledDev[/url].
Check out my [url=https://sldb.core-directive.com/en/]Survival Log wiki[/url] for items, crafting, maps and more.

[size=5]Source and license[/size]
Open source under MIT: [url=https://github.com/AnExiledDev/SurvivalLog-GameRules]github.com/AnExiledDev/SurvivalLog-GameRules[/url]

[line]
[size=5]简体中文[/size]

修改 Survival Log 自身的一些规则：重生次数、食物和霉菌、老鼠和疾病、电力和房屋、寒冷、零散丧尸和尸潮、夜间访客、邻居、你支援的陌生人、心态、锁住的区域、天气，以及暖气能覆盖多大范围。

每项设置的默认值都是游戏原本的数值，所以全新的配置不会改变任何东西。只开启你想要的即可。

[size=5]主要功能[/size]
[list]
[*][b]重生次数无限[/b]：重生次数不会用完。无论用了多少次，始终保留游戏原本的全部次数，所以重生界面仍显示正常的一排标记。[/*]
[*][b]霉菌不再蔓延[/b]：霉菌不再蔓延到旁边的食物。已有的霉菌仍会照常消退。[/*]
[*][b]太阳能倍率[/b]：太阳能板发电更多（或更少），从 0.1 倍到 10 倍。[/*]
[*][b]恶劣天气太阳能[/b]：在暴雨等恶劣天气中，游戏原本的太阳能板不发电。此项让它们发出阴天发电量的一部分。[/*]
[*][b]腐败速度[/b]：食物腐败得更慢或更快，从 0.1 倍到 5 倍。[/*]
[*][b]蓄电池倍率[/b]：蓄电池容量更大（或更小），从 0.1 倍到 10 倍。[/*]
[*][b]没有砸门的邻居[/b]：愤怒的邻居不会在深夜来砸门（「深夜砸门」）。[/*]
[*][b]没有催债人[/b]：放贷人的深夜上门、警告短信、报复和跟踪的夜晚都不会发生。[/*]
[*][b]所有区域都打开[/b]：打工仔和大学生的二楼和地下室，或仓库管理员的二楼、冷库、车库和两侧的小屋，在灾难发生时立即打开。可以直接走过去并在那里建造。对已经过了灾难的存档同样生效；灾难前的存档在灾难到来时打开。[/*]
[*][b]随机天气[/b]：天气按你设置的概率生成，比如游戏后期少下雨。寒潮和剧情固定天气的日子与游戏原本相同。[/*]
[*][b]食物[/b]：生吃食物后生病的几率；重复进食导致的「厌食」可以关闭，也可以调整持续时间和对食物效果的影响。[/*]
[*][b]霉菌[/b]：「消毒喷雾」的清理范围和消耗、发霉食物的腐坏速度、「霉菌」的扩散间隔、每次扩散的物品数和初始发霉物品数。[/*]
[*][b]老鼠[/b]：老鼠来访的几率，以及老鼠逃走前停留的时间。[/*]
[*][b]疾病[/b]：关闭「疫病」危机，或只关闭「疫病加重」；调整寒潮中患上「风寒」和「发烧」的几率。[/*]
[*][b]电力与房屋[/b]：电器耗电倍率、推迟停电或永不停电、关闭「停电恐慌」、不再出现「暴雨侵袭」漏雨。[/*]
[*][b]寒冷[/b]：「寒潮」等寒冷天气的强度倍率，以及额外的「个人抗寒」等级。[/*]
[*][b]零散丧尸[/b]：可以关闭，也可以设置开始天数、间隔、每天次数、同时数量、由「爬行者丧尸」带领的几率和击杀掉落。[/*]
[*][b]尸潮[/b]：剧情「尸潮」总是全部强度，以及「尸潮」战利品倍率。[/*]
[*][b]邻居和同行者[/b]：她们不会死亡，或者推迟救助期限。[/*]
[*][b]你支援的陌生人[/b]：每天失去的「信赖」、「遭灾」的几率，以及哪些灾祸可以发生。[/*]
[*][b]委托[/b]：新委托和「救助平台」新求助出现的频率。[/*]
[*][b]想法、心态、生产和拾取[/b]：角色自言自语的频率、听歌和完美收获带来的「心态」、花瓶的心态上限、低等级配方给予全部「熟练度」，以及拾取掉落物的距离。[/*]
[*][b]整层供暖[/b]：运行中的电暖器、空调和暖炉可以同时温暖室内和阳光房，露台和地下室也可以加入。这就是 Whole Floor Climate 模组，现在合并到这里。Game Rules 第一次启动时会复制它的设置，之后请删除 [font=Courier New]WholeFloorClimate.dll[/font]。[/*]
[/list]

Game Rules 不会往存档里写入自己的数据，你可以随时移除它。由「所有区域都打开」打开的区域，在移除模组后仍保持打开，与游戏中正常打开的区域一样。

支持英文和中文。

[size=5]已知问题[/size]
[list]
[*][b]所有区域都打开[/b]只在灾难之后打开区域。灾难前你所在的房子在技术上是另一张独立的地图，那张地图没有通往其他楼层或锁住房间的路。要让它们在那里可用，工作量巨大，所以这些区域改为在灾难后的游戏开始时打开。[/*]
[*][b]所有区域都打开[/b]会跳过原本用来打开这些区域的剧情事件（找到钥匙、清理楼梯和地窖），所以这些事件的奖励和剧情也会被跳过。读档时已经在进行中的事件仍会留在事件列表里。两层楼都打开后可能会解锁对应的成就。[/*]
[*][b]腐败速度[/b]：保鲜比房间更好的通电冰箱保持自己的速度。已经放好的食物在下次移动或存放时才会采用新的速度。[/*]
[*][b]恶劣天气太阳能[/b]在夜间没有效果，对照不到光的太阳能板也没有效果。[/*]
[*][b]随机天气[/b]从下一个小时起改变天气，天气预报也会显示新的天气。相同的种子总是生成相同的天气，所以读档后保持不变。[/*]
[*][b]没有催债人[/b]最好在放贷人第一次上门前开启。已经在进行中的上门会照常结束。[/*]
[*][b]老鼠来访几率[/b]在「鼠患」期间不生效。[/*]
[*][b]电器耗电倍率[/b]不影响烹饪和粉碎工作、压缩机、防御设施开火和信号站。[/*]
[*][b]永不停电[/b]停电之后的事件和柴油发电机的启动也不会出现。已经发生的停电不会恢复。[/*]
[*][b]寒潮强度[/b]也会影响你支援的陌生人是否把当天算作寒冷。[/*]
[/list]

[size=5]前置要求[/size]
[list]
[*]Survival Log 1.1.19131。游戏更新后模组可能暂时失效，直到模组更新为止。[/*]
[*][url=https://www.nexusmods.com/survivallog/mods/12]BepInEx for Survival Log[/url]，请先安装。[/*]
[*]可选：[url=https://www.nexusmods.com/survivallog/mods/61]Exiled's Toolkit[/url] 0.4.0 或更新版本。装了它，在 Mod Manager 的「模组」界面改动的设置无需重启游戏即可生效；没装的话，改动在下次启动游戏时生效。[/*]
[/list]

[size=5]安装[/size]
[list=1]
[*]安装 [url=https://www.nexusmods.com/survivallog/mods/12]BepInEx[/url] [i]（如果之前没有装过其他模组，先启动一次游戏，让 BepInEx 完成初始化，然后退出）[/i]。[/*]
[*]可选（改动设置无需重启游戏）：用同样的方式安装 Exiled's Toolkit，下载它的压缩包并解压到游戏目录。[/*]
[*]在「文件」页下载压缩包。[/*]
[*]把压缩包解压到游戏目录（[font=Courier New]SurvivalLog.exe[/font] 所在的文件夹）。压缩包里带有 BepInEx 文件夹，DLL 会自动放到正确位置。如果提示合并文件夹，选择合并。解压后应为：
[code]C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\GameRules.dll[/code][/*]
[/list]

[size=5]卸载[/size]
从 [font=Courier New]BepInEx\plugins[/font] 文件夹中删除 [font=Courier New]GameRules.dll[/font]。

[size=5]配置[/size]
如需修改设置，可以在游戏内 Mod Manager 的「模组」界面修改（改动立即生效，[font=Courier New]NoRestartLimit[/font]、[font=Courier New]BatteryMultiplier[/font]、[font=Courier New]StartWithAllAreasOpen[/font] 和 [font=Courier New]Weather[/font] 各项除外，它们在下次启动游戏时生效）；也可以在游戏关闭时编辑 [font=Courier New]BepInEx\config\com.dev.survivallog.gamerules.cfg[/font]。没装 Exiled's Toolkit 时，在「模组」界面的改动在下次启动游戏时生效。配置文件中每项设置都有中文说明，完整说明见 [url=https://github.com/AnExiledDev/SurvivalLog-GameRules/blob/main/CONFIG.md]CONFIG.md[/url]（英文）。

[size=5]反馈[/size]
发现 bug 或有好点子？欢迎来 [url=https://discord.gg/trTsyxUFqg]Core Directive Discord[/url] 打个招呼，或在本页的「Posts」标签里发帖。报告 bug 时，请附上 [font=Courier New]BepInEx\LogOutput.log[/font] 和 [font=Courier New]BepInEx\game-rules.log[/font]。
想直接联系我，可以在 Discord 上给 [b]exiled.dev[/b] 发消息，我在 [url=https://discord.gg/MZGg4gQAUe]Survival Log 官方 Discord[/url] 里。也可以在 X 上找我：[url=https://x.com/AnExiledDev]@AnExiledDev[/url]。
也欢迎看看我做的 [url=https://sldb.core-directive.com/zh/]Survival Log 维基[/url]，收录物品、制作、地图等资料。

[size=5]源代码与许可[/size]
基于 MIT 许可开源：[url=https://github.com/AnExiledDev/SurvivalLog-GameRules]github.com/AnExiledDev/SurvivalLog-GameRules[/url]
