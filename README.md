## IMPORTANT STUFF

This mod contains a collection of patches from various sources (all MIT/GNU GPL licensed sources) in order to bring you
the best collection of QOL and tweaks in one spot

`Install on all clients and the server.`

`This mod uses ServerSync, if installed on the server and the config is set to be forced, it will sync all configs to client`

`This mod uses a file watcher. If the configuration file is not changed with BepInEx Configuration manager, but changed in the file directly on the server, upon file save, it will sync the changes to all clients. Please keep in mind that logging out or rebooting might be required for some configuration changes, as not everything can update immediately`


### Features

`General`

Lock Configuration [Synced with Server]
* Lock Configuration
  * Default Value: true

Enabled [Synced with Server]
* Enable the entire mod
  * Default Value: true

`Dungeon`

Change Dungeons [Synced with Server]
* This, when false, will disable the mod from changing dungeon room counts and camp radius.
  * Default Value: false

Change Camp Radius [Synced with Server]
* This, when false, will disable the mod from changing camp radius's.
  * Default Value: false

Max Room Count [Synced with Server]
* This is the max number of rooms placed by dungeon gen higher numbers will cause lag
  * Default Value: 20

Min Room Count [Synced with Server]
* This is the Min number of rooms placed by dungeon gen higher numbers will cause lag
  * Default Value: 20

Camp Radius Min [Synced with Server]
* This is the minimum radius for goblin camps
  * Default Value: 5

Camp Radius Max [Synced with Server]
* This is the maximum radius for goblin camps
  * Default Value: 15

`Containers`

Container Section On [Synced with Server]
* Toggle this value to turn the entire Containers section off/on
  * Default Value: true

Chest Container Control [Synced with Server]
* Toggle this value to turn off this mod's control over chest container size
  * Default Value: true

Ship Container Control [Synced with Server]
* Toggle this value to turn off this mod's control over ship chest container size
  * Default Value: true

Karve Rows [Synced with Server]
* Rows for Karve
  * Default Value: 2

Karve Columns [Synced with Server]
* Columns for Karve
  * Default Value: 2

Longboat Rows [Synced with Server]
* Rows for longboat
  * Default Value: 3

Longboat Columns [Synced with Server]
* Columns for longboat
  * Default Value: 6

Cart Rows [Synced with Server]
* Rows for Cart
  * Default Value: 3

Cart Columns [Synced with Server]
* Columns for Cart
  * Default Value: 6

Personal Chest Rows [Synced with Server]
* Personal Chest Rows
  * Default Value: 2

Personal Chest Columns [Synced with Server]
* Personal Chest Columns
  * Default Value: 3

Wood Chest Rows [Synced with Server]
* Wood Chest Rows
  * Default Value: 2

Wood Chest Columns [Synced with Server]
* Wood Chest Columns
  * Default Value: 5

Iron Chest Rows [Synced with Server]
* Iron Chest Rows
  * Default Value: 4

Iron Chest Columns [Synced with Server]
* Iron Chest Columns
  * Default Value: 6

Blackmetal Chest Rows [Synced with Server]
* Blackmetal Chest Rows
  * Default Value: 4

Blackmetal Chest Columns [Synced with Server]
* Blackmetal Chest Columns
  * Default Value: 8

`WorkBench`

Allow Workbench Alterations [Synced with Server]
* Toggles the WorkBench section.
  * Default Value: false

WorkBenchRange [Synced with Server]
* Range you can build from workbench in meters
  * Default Value: 20

WorkBenchRange (Playerbase size) [Synced with Server]
* Workbench PlayerBase radius, this is how far away enemies spawn
  * Default Value: 20

Change No Roof Behavior [Synced with Server]
* Turns off the need for a roof to be above the workbench
  * Default Value: false

WorkBench Extension [Synced with Server]
* Range for workbench extensions
  * Default Value: 5

`Show Chest Contents`

MaxEntries [Not Synced with Server]
* Max number of entries to show (-1 means show all)
  * Default Value: -1

SortType [Not Synced with Server]
* Type by which to sort entries.
  * Default Value: Value

SortAsc [Not Synced with Server]
* Sort ascending?
  * Default Value: false

EntryText [Not Synced with Server]
* Entry text. {0} is replaced by the total amount, {1} is replaced by the item name.
  * Default Value: <color=#FFFFAAFF>{0}</color> <color=#AAFFAAFF>{1}</color>

OverFlowText [Not Synced with Server]
* Overflow text if more items than max entries.
  * Default Value: <color=#AAAAAAFF>...</color>

`General`

CapacityText [Not Synced with Server]
* Text to show capacity. {0} is replaced by number of full slots, {1} is replaced by total slots.
  * Default Value: <color=#FFFFAAFF> {0}/{1}</color>

`Items`

Item Weight Increase [Synced with Server]
* Multiplier for your item weight. This is a modifier value. 50 will increase it by 50%, -50 will reduce it by 50%.
  * Default Value: 1

Item Stack Increase [Synced with Server]
* Multiplier for your item stacks. Directly multiplies stack of the item by this value. (Stack of 50 with value of 10 here would turn into a stack of 500)
  * Default Value: 1

Disable Teleport check for items [Synced with Server]
* Disable Teleport check for items
  * Default Value: false

Fill your things top to bottom when moving from inv to chest [Not Synced with Server]
* Move your things top to bottom when changing from inv to chest
  * Default Value: true

Auto repair your things when interacting with build station [Synced with Server]
* Auto repair your things when interacting with build station
  * Default Value: false

`Map Details`

MapDetail On [Synced with Server]
* Toggle this whole section off/on
  * Default Value: false

Display Boats/Carts [Synced with Server]
* Show Boats and carts on the map
  * Default Value: true

Custom Boats & Carts [Synced with Server]
* Custom boats to show on the map. Format: "BoatName1,BoatName2,BoatName3,CartName1".
  * Default Value: CargoShip,Skuldelev,LittleBoat,MercantShip,BigCargoShip,FishingBoat,FishingCanoe,WarShip

ShowRange [Synced with Server]
* Range in metres around player to show details
  * Default Value: 50

UpdateDelta [Synced with Server]
* Distance in metres to move before automatically updating the map details
  * Default Value: 5

ShowBuildings [Synced with Server]
* Show building pieces
  * Default Value: true

PersonalBuildingColor [Not Synced with Server]
* Color of one's own build pieces
  * Default Value: 00FF00FF

OtherBuildingColor [Synced with Server]
* Color of other players' build pieces
  * Default Value: FF0000FF

UnownedBuildingColor [Not Synced with Server]
* Color of npc build pieces
  * Default Value: FFEB04FF

CustomPlayerColors [Synced with Server]
* Custom color list, comma-separated. Use either <name>:<colorCode> pair entries or just <colorCode> entries. E.g. Erinthe:FF0000 or just FF0000. The latter will assign a color randomly to each connected peer.
  * Default Value:

`Clock`

ShowClock [Synced with Server]
* Show the clock?
  * Default Value: true

ShowClockKeyMod [Not Synced with Server]
* Extra modifier key used to toggle the clock display. Leave blank to not require one. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html
  * Default Value:

ShowClockKey [Not Synced with Server]
* Key used to toggle the clock display. use https://docs.unity3d.com/Manual/ConventionalGameInput.html
  * Default Value: home

ClockLocationString [Not Synced with Server]
* Location on the screen to show the clock (x,y) or (x%,y%)
  * Default Value: 48%,0%

ShowClockOnChange [Not Synced with Server]
* Only show the clock when the time changes?
  * Default Value: false

ShowClockOnChangeFadeTime [Not Synced with Server]
* If only showing on change, length in seconds to show the clock before begining to fade
  * Default Value: 5

ShowClockOnChangeFadeLength [Not Synced with Server]
* How long fade should take in seconds
  * Default Value: 1

ClockUseOSFont [Not Synced with Server]
* Set to true to specify the name of a font from your OS; otherwise limited to fonts in the game resources
  * Default Value: false

ClockUseShadow [Not Synced with Server]
* Add a shadow behind the text
  * Default Value: false

ClockShadowOffset [Not Synced with Server]
* Shadow offset in pixels
  * Default Value: 2

ClockFontName [Not Synced with Server]
* Name of the font to use
  * Default Value: AveriaSerifLibre-Bold

ClockFontSize [Not Synced with Server]
* Location on the screen in pixels to show the clock
  * Default Value: 24

ClockFontColor [Not Synced with Server]
* Font color for the clock
  * Default Value: FFFFFFFF

ClockShadowColor [Not Synced with Server]
* Color for the shadow
  * Default Value: 000000FF

ShowClockKeyOnPress [Not Synced with Server]
* If true, limit clock display to when the hotkey is down
  * Default Value: false

ClockFormat [Not Synced with Server]
* Time format; set to 'fuzzy' for fuzzy time
  * Default Value: HH:mm

ClockString [Not Synced with Server]
* Formatted clock string - {0} is replaced by the actual time string, {1} is replaced by the fuzzy string, {2} is replaced by the current day
  * Default Value: <b>{0}</b>

ClockTextAlignment [Not Synced with Server]
* Clock text alignment.
  * Default Value: MiddleCenter

ClockFuzzyStrings [Not Synced with Server]
* Fuzzy time strings to split up the day into custom periods if ClockFormat is set to 'fuzzy'; comma-separated
  * Default Value: Midnight,Early Morning,Early Morning,Before Dawn,Before Dawn,Dawn,Dawn,Morning,Morning,Late Morning,Late Morning,Midday,Midday,Early Afternoon,Early Afternoon,Afternoon,Afternoon,Evening,Evening,Night,Night,Late Night,Late Night,Midnight

`Game`

I have arrived disable [Synced with Server]
* Disable the I have arrived message
  * Default Value: true

BuildInProtectedLocations [Synced with Server]
* Allow Building Inside Protected Locations
  * Default Value: false

Change Crafting Duration [Not Synced with Server]
* Change Crafting Duration time.
  * Default Value: 0.25

Disable Guardian Animation [Synced with Server]
* Disable Guardian Animation for the players
  * Default Value: true

Skip Tuts [Not Synced with Server]
* Skip Tutorials
  * Default Value: true

`Player`

Re Equip after Swimming [Not Synced with Server]
* Re-equip Items After Swimming
  * Default Value: true

Area Repair [Synced with Server]
* Automatically repair build pieces within the repair radius
  * Default Value: true

Area Repair Radius [Synced with Server]
* Area Repair Radius for build pieces
  * Default Value: 15

Base Meginjord Buff [Synced with Server]
* Meginjord buff amount (Base)
  * Default Value: 150

`Game`

Honey Speed [Synced with Server]
* Honey Production Speed
  * Default Value: 1200

Honey Count Per Hive [Synced with Server]
* Honey Count Per Hive
  * Default Value: 4

`Server`

Max Player Count [Synced with Server]
* Max number of Players to allow in a server
  * Default Value: 50

`Player`

Stamina alterations enabled [Synced with Server]
* Stamina alterations enabled
  Note: These are not percent drains. They are direct drain values.
  * Default Value: false

Dodge Stamina Usage [Synced with Server]
* Dodge Stamina Usage
  * Default Value: 10

Encumbered Stamina drain [Synced with Server]
* Encumbered Stamina drain
  * Default Value: 10

Sneak Stamina Drain [Synced with Server]
* Sneak stamina drain
  * Default Value: 5

Run Stamina Drain [Synced with Server]
* Run Stamina Drain
  * Default Value: 10

Delay before stamina regeneration starts [Synced with Server]
* Delay before stamina regeneration starts
  * Default Value: 1

Stamina regen factor [Synced with Server]
* Stamina regen factor
  * Default Value: 5

Stamina drain from swim [Synced with Server]
* Stamina drain from swim
  * Default Value: 5

Jump stamina drain factor [Synced with Server]
* Stamina drain factor for jumping
  * Default Value: 10

Auto pickup range adjustment [Synced with Server]
* Auto pickup range adjustment
  * Default Value: 2

Cam shake factor [Not Synced with Server]
* Cam Shake factor
  * Default Value: 0

Base maximum weight addition for player [Synced with Server]
* Base max weight addition for player
  * Default Value: 350

`WorkBench`

Build distance alteration [Synced with Server]
* Build Distance  (Maximum Placement Distance)
  * Default Value: 15

`Game`

Portal Tag on Hover [Synced with Server]
* Enabled Portal Tag message while hovering over Portal
  * Default Value: true

`Player`

ShowDamageFlash [Synced with Server]
* Show the flashing red screen when taking damage
  * Default Value: true

No Food Degrade [Synced with Server]
* Disables food degrading
  * Default Value: false

Modify food [Synced with Server]
* Ensuring the food lasts longer while maintaining the same rate of regeneration. Needed to be on for No Food Degrade to work
  * Default Value: false

`Building HUD`

Inventory Amount Format [Synced with Server]
* Format for the amount of items in the player inventory to show after the required amount. Uses standard C# format rules. Leave empty to hide altogether.
  * Default Value: ({0})

Inventory Amount Color [Not Synced with Server]
* Color to set the inventory amount after the requirement amount. Leave empty to set no color. You can use the #XXXXXX hex color format.
  * Default Value: lightblue

Can Build Amount Format [Not Synced with Server]
* Format for the amount of times you can build the currently selected item with your current inventory. Uses standard C# format rules. Leave empty to hide altogether.
  * Default Value: ({0})

Can Build Amount Color [Not Synced with Server]
* Color to set the can-build amount. Leave empty to set no color. You can use the #XXXXXX hex color format.
  * Default Value: white

`Signs`

SignScale [Synced with Server]
* Sign scale (w,h,d)
  * Default Value: {"x":1.0,"y":1.0,"z":1.0}

TextPositionOffset [Synced with Server]
* Default font size
  * Default Value: {"x":0.0,"y":0.0}

UseRichText [Synced with Server]
* Enable rich text. If this is disabled, the sign reverts back to vanilla functionality.
  * Default Value: true

FontName [Not Synced with Server]
* Font name
  * Default Value: Norsebold

SignDefaultColor [Synced with Server]
* This uses string values to set the default color every sign should have. The code runs when the sign loads in for the first time. If the sign doesn't have a color tag already, it will wrap the text in one. Use values like "red" here to specify a default color.
  * Default Value: black

`Auto Storage`

AutoStorageIsOn [Synced with Server]
* Behaviour is currently on or not
  * Default Value: false

Store Shortcut [Not Synced with Server]
* Keyboard shortcut/Hotkey to store your inventory into nearby containers.
  * Default Value: Period

Player Range [Synced with Server]
* The maximum distance from the player to store items when the Store Shortcut is pressed. Follows storage rules for allowed items.
  * Default Value: 5

DropRangeChests [Synced with Server]
* The maximum range to pull dropped items for Chests (Default chest)
  * Default Value: 5

DropRangePersonalChests [Synced with Server]
* The maximum range to pull dropped items for Personal chests
  * Default Value: 5

DropRangeReinforcedChests [Synced with Server]
* The maximum range to pull dropped items for Re-inforced Chests
  * Default Value: 5

DropRangeBlackmetalChests [Synced with Server]
* The maximum range to pull dropped items for Blackmetal Chests
  * Default Value: 5

DropRangeCustomChests [Synced with Server]
* The maximum range to pull dropped items for Custom Chests
  * Default Value: 5

DropRangeCarts [Synced with Server]
* The maximum range to pull dropped items for Carts
  * Default Value: 5

DropRangeShips [Synced with Server]
* The maximum range to pull dropped items for Ships
  * Default Value: 5

ItemDisallowTypes [Synced with Server]
* Types of item to disallow pulling for, comma-separated. Uses Prefab names.
  * Default Value:

ItemDisallowCategories [Synced with Server]
* Categories of item to disallow pulling for, comma-separated.
  Accepted Values are None,Material,Consumable,OneHandedWeapon,Bow,Shield,Helmet,Chest,Ammo,Customization,Legs,Hands,Trophie,TwoHandedWeapon,Torch,Misc,Shoulder,Utility,Tool,Attach_Atgeir
  * Default Value:

ItemAllowCategories [Synced with Server]
* Categories of item to only allow pulling for, comma-separated. Overrides ItemDisallowCategories.  
  Accepted Values are None,Material,Consumable,OneHandedWeapon,Bow,Shield,Helmet,Chest,Ammo,Customization,Legs,Hands,Trophie,TwoHandedWeapon,Torch,Misc,Shoulder,Utility,Tool,Attach_Atgeir
  * Default Value:

ItemDisallowCategoriesChests [Synced with Server]
* Categories of item to disallow pulling for, comma-separated.
  Accepted Values are None,Material,Consumable,OneHandedWeapon,Bow,Shield,Helmet,Chest,Ammo,Customization,Legs,Hands,Trophie,TwoHandedWeapon,Torch,Misc,Shoulder,Utility,Tool,Attach_Atgeir
  * Default Value:

ItemAllowCategoriesChests [Synced with Server]
* Categories of item to only allow pulling for, comma-separated. Overrides ItemDisallowCategoriesChests.
  Accepted Values are None,Material,Consumable,OneHandedWeapon,Bow,Shield,Helmet,Chest,Ammo,Customization,Legs,Hands,Trophie,TwoHandedWeapon,Torch,Misc,Shoulder,Utility,Tool,Attach_Atgeir
  * Default Value:

ItemDisallowCategoriesPersonalChests [Synced with Server]
* Categories of item to disallow pulling for, comma-separated.
  Accepted Values are None,Material,Consumable,OneHandedWeapon,Bow,Shield,Helmet,Chest,Ammo,Customization,Legs,Hands,Trophie,TwoHandedWeapon,Torch,Misc,Shoulder,Utility,Tool,Attach_Atgeir
  * Default Value:

ItemAllowCategoriesPersonalChests [Synced with Server]
* Categories of item to only allow pulling for, comma-separated. Overrides ItemDisallowCategoriesPersonalChests.
  Accepted Values are None,Material,Consumable,OneHandedWeapon,Bow,Shield,Helmet,Chest,Ammo,Customization,Legs,Hands,Trophie,TwoHandedWeapon,Torch,Misc,Shoulder,Utility,Tool,Attach_Atgeir
  * Default Value:

ItemDisallowCategoriesBlackMetalChests [Synced with Server]
* Categories of item to disallow pulling for, comma-separated.
  Accepted Values are None,Material,Consumable,OneHandedWeapon,Bow,Shield,Helmet,Chest,Ammo,Customization,Legs,Hands,Trophie,TwoHandedWeapon,Torch,Misc,Shoulder,Utility,Tool,Attach_Atgeir
  * Default Value:

ItemAllowCategoriesBlackMetalChests [Synced with Server]
* Categories of item to only allow pulling for, comma-separated. Overrides ItemDisallowCategoriesBlackMetalChests.
  Accepted Values are None,Material,Consumable,OneHandedWeapon,Bow,Shield,Helmet,Chest,Ammo,Customization,Legs,Hands,Trophie,TwoHandedWeapon,Torch,Misc,Shoulder,Utility,Tool,Attach_Atgeir
  * Default Value:

ItemDisallowCategoriesReinforcedChests [Synced with Server]
* Categories of item to disallow pulling for, comma-separated.
  Accepted Values are None,Material,Consumable,OneHandedWeapon,Bow,Shield,Helmet,Chest,Ammo,Customization,Legs,Hands,Trophie,TwoHandedWeapon,Torch,Misc,Shoulder,Utility,Tool,Attach_Atgeir
  * Default Value:

ItemAllowCategoriesReinforcedChests [Synced with Server]
* Categories of item to only allow pulling for, comma-separated. Overrides ItemDisallowCategoriesReinforcedChests.
  Accepted Values are None,Material,Consumable,OneHandedWeapon,Bow,Shield,Helmet,Chest,Ammo,Customization,Legs,Hands,Trophie,TwoHandedWeapon,Torch,Misc,Shoulder,Utility,Tool,Attach_Atgeir
  * Default Value:

ItemDisallowCategoriesCarts [Synced with Server]
* Categories of item to disallow pulling for, comma-separated.
  Accepted Values are None,Material,Consumable,OneHandedWeapon,Bow,Shield,Helmet,Chest,Ammo,Customization,Legs,Hands,Trophie,TwoHandedWeapon,Torch,Misc,Shoulder,Utility,Tool,Attach_Atgeir
  * Default Value:

ItemAllowCategoriesCarts [Synced with Server]
* Categories of item to only allow pulling for, comma-separated. Overrides ItemDisallowCategoriesCarts.
  Accepted Values are None,Material,Consumable,OneHandedWeapon,Bow,Shield,Helmet,Chest,Ammo,Customization,Legs,Hands,Trophie,TwoHandedWeapon,Torch,Misc,Shoulder,Utility,Tool,Attach_Atgeir
  * Default Value:

ItemDisallowCategoriesShips [Synced with Server]
* Categories of item to disallow pulling for, comma-separated.
  Accepted Values are None,Material,Consumable,OneHandedWeapon,Bow,Shield,Helmet,Chest,Ammo,Customization,Legs,Hands,Trophie,TwoHandedWeapon,Torch,Misc,Shoulder,Utility,Tool,Attach_Atgeir
  * Default Value:

ItemAllowCategoriesShips [Synced with Server]
* Categories of item to only allow pulling for, comma-separated. Overrides ItemDisallowCategoriesShips.
  Accepted Values are None,Material,Consumable,OneHandedWeapon,Bow,Shield,Helmet,Chest,Ammo,Customization,Legs,Hands,Trophie,TwoHandedWeapon,Torch,Misc,Shoulder,Utility,Tool,Attach_Atgeir
  * Default Value:

ItemAllowTypes [Synced with Server]
* Types of item to only allow pulling for, comma-separated. Uses Prefab names. Overrides ItemDisallowTypes
  * Default Value:

ItemDisallowTypesChests [Synced with Server]
* Types of item to disallow pulling for, comma-separated. Uses Prefab names. (For Chests)
  * Default Value:

ItemAllowTypesChests [Synced with Server]
* Types of item to only allow pulling for, comma-separated. Uses Prefab names. Overrides ItemDisallowTypesChests
  * Default Value:

ItemDisallowTypesPersonalChests [Synced with Server]
* Types of item to disallow pulling for, comma-separated. Uses Prefab names. (For Personal Chests)
  * Default Value:

ItemAllowTypesPersonalChests [Synced with Server]
* Types of item to only allow pulling for, comma-separated. Uses Prefab names. Overrides ItemDisallowTypesPersonalChests
  * Default Value:

ItemDisallowTypesReinforcedChests [Synced with Server]
* Types of item to disallow pulling for, comma-separated. Uses Prefab names. (For ReinforcedChests)
  * Default Value:

ItemAllowTypesReinforcedChests [Synced with Server]
* Types of item to only allow pulling for, comma-separated. Uses Prefab names. Overrides ItemDisallowTypesReinforcedChests
  * Default Value:

ItemDisallowTypesBlackMetalChests [Synced with Server]
* Types of item to disallow pulling for, comma-separated. Uses Prefab names.
  * Default Value:

ItemAllowTypesBlackMetalChests [Synced with Server]
* Types of item to only allow pulling for, comma-separated. Uses Prefab names. Overrides ItemDisallowTypesBlackMetalChests
  * Default Value:

CustomChests [Synced with Server]
* Custom Chests to use, comma-separated. Uses Prefab names.
  * Default Value:

ItemDisallowTypesCustomChests [Synced with Server]
* Types of item to disallow pulling for, comma-separated. Uses Prefab names.
  * Default Value:

ItemAllowTypesCustomChests [Synced with Server]
* Types of item to only allow pulling for, comma-separated. Uses Prefab names. Overrides ItemDisallowTypesCustomChests
  * Default Value:

ItemDisallowTypesCarts [Synced with Server]
* Types of item to disallow pulling for, comma-separated. Uses Prefab names. (For Carts)
  * Default Value:

ItemAllowTypesCarts [Synced with Server]
* Types of item to only allow pulling for, comma-separated. Uses Prefab names. Overrides ItemDisallowTypesCarts
  * Default Value:

ItemDisallowTypesShips [Synced with Server]
* Types of item to disallow pulling for, comma-separated. Uses Prefab names. (For Ships)
  * Default Value:

ItemAllowTypesShips [Synced with Server]
* Types of item to only allow pulling for, comma-separated. Uses Prefab names. Overrides ItemDisallowTypesShips
  * Default Value:

ToggleString [Synced with Server]
* Text to show on toggle. {0} is replaced with true/false
  * Default Value: Auto Pull: {0}

ToggleKey [Not Synced with Server]
* Key to toggle behaviour. Leave blank to disable the toggle key.
  * Default Value:

MustHaveItemToPull [Synced with Server]
* If true, a container must already have at least one of the item to pull.
  * Default Value: false

`PlantGrowth`

DisplayGrowth [Not Synced with Server]
* Display growth progress in hover text (applies to pickables as well)
  * Default Value: true

PlantAnywhere [Synced with Server]
* Don't require cultivated ground to plant anything
  * Default Value: false

IgnoreBiome [Synced with Server]
* Allow planting anything in any biome.
  * Default Value: false

IgnoreSun [Synced with Server]
* Allow planting under roofs.
  * Default Value: false

PreventPlantTooClose [Synced with Server]
* Prevent plants from being planted if they are too close together to grow.
  * Default Value: true

PreventDestroyIfCantGrow [Synced with Server]
* Prevent destruction of plants that normally are destroyed if they can't grow.
  * Default Value: false

GrowthTimeMultTree [Synced with Server]
* Multiply time taken to grow by this amount.
  * Default Value: 1

GrowthRadiusMultTree [Synced with Server]
* Multiply required space to grow by this amount.
  * Default Value: 1

MinScaleMultTree [Synced with Server]
* Multiply minimum size by this amount.
  * Default Value: 1

MaxScaleMultTree [Synced with Server]
* Multiply maximum size by this amount.
  * Default Value: 1

GrowthTimeMultPlant [Synced with Server]
* Multiply time taken to grow by this amount.
  * Default Value: 1

GrowthRadiusMultPlant [Synced with Server]
* Multiply required space to grow by this amount.
  * Default Value: 1

MinScaleMultPlant [Synced with Server]
* Multiply minimum size by this amount.
  * Default Value: 1

MaxScaleMultPlant [Synced with Server]
* Multiply maximum size by this amount.
  * Default Value: 1

`Fermenter`

ShowFermenterStatus [Not Synced with Server]
* Display time left in fermentation process in hover text
  * Default Value: true

`WearNTear_Patches`

Structural Integrity Control [Synced with Server]
* Set to true to enable the Structural Integrity settings in this mod.
  * Default Value: false

No Weather Damage to buildings [Synced with Server]
* No Weather Damage to buildings
  * Default Value: false

Disable Structural Integrity system [Synced with Server]
* Disable Structural Integrity system. Allows for placement of things in the air, does not prevent building damage.
  * Default Value: false

Disable Boat Damage [Synced with Server]
* Disable Boat Damage
  * Default Value: false

Disable Boat Water Damage [Synced with Server]
* Disable Boat Water Damage
  * Default Value: false

No Damage to player buildings [Synced with Server]
* No Damage to player buildings
  * Default Value: false

Wood Structural Integrity [Synced with Server]
* Wood Structural Integrity. Reduces the loss of structural integrity by distance by % less. The value 100 would result in disabled structural integrity over distance, does not allow for placement in free air without Disable Structural Integrity system.
  * Default Value: 0

Stone Structural Integrity [Synced with Server]
* Stone Structural Integrity. Reduces the loss of structural integrity by distance by % less. The value 100 would result in disabled structural integrity over distance, does not allow for placement in free air without Disable Structural Integrity system.
  * Default Value: 0

Iron Structural Integrity [Synced with Server]
* Iron Structural Integrity. Reduces the loss of structural integrity by distance by % less. The value 100 would result in disabled structural integrity over distance, does not allow for placement in free air without Disable Structural Integrity system.
  * Default Value: 0

Hardwood Structural Integrity [Synced with Server]
* Hardwood Structural Integrity. Reduces the loss of structural integrity by distance by % less. The value 100 would result in disabled structural integrity over distance, does not allow for placement in free air without Disable Structural Integrity system.
  * Default Value: 0

Marble Structural Integrity [Synced with Server]
* Hardwood Structural Integrity. Reduces the loss of structural integrity by distance by % less. The value 100 would result in disabled structural integrity over distance, does not allow for placement in free air without Disable Structural Integrity system.
  * Default Value: 0

`Skills`

Change the skill gain factor [Synced with Server]
* Change skill gain factor
  * Default Value: false

Display notifications for skills gained [Synced with Server]
* Display notifications for skills gained
  * Default Value: false

Sword Skill gain factor [Synced with Server]
* Sword skill gain factor. This is a modifier value. 50 will increase it by 50%, -50 will reduce it by 50%.
  * Default Value: 0

Knives Skill gain factor [Synced with Server]
* Knives skill gain factor. This is a modifier value. 50 will increase it by 50%, -50 will reduce it by 50%.
  * Default Value: 0

Clubs Skill gain factor [Synced with Server]
* Clubs skill gain factor. This is a modifier value. 50 will increase it by 50%, -50 will reduce it by 50%.
  * Default Value: 0

Polearm Skill gain factor [Synced with Server]
* Polearm skill gain factor. This is a modifier value. 50 will increase it by 50%, -50 will reduce it by 50%.
  * Default Value: 0

Spear Skill gain factor [Synced with Server]
* Spear skill gain factor. This is a modifier value. 50 will increase it by 50%, -50 will reduce it by 50%.
  * Default Value: 0

Block Skill gain factor [Synced with Server]
* Block skill gain factor. This is a modifier value. 50 will increase it by 50%, -50 will reduce it by 50%.
  * Default Value: 0

Axe Skill gain factor [Synced with Server]
* Axe skill gain factor. This is a modifier value. 50 will increase it by 50%, -50 will reduce it by 50%.
  * Default Value: 0

Bow Skill gain factor [Synced with Server]
* Bow skill gain factor. This is a modifier value. 50 will increase it by 50%, -50 will reduce it by 50%.
  * Default Value: 0

Unarmed Skill gain factor [Synced with Server]
* Unarmed skill gain factor. This is a modifier value. 50 will increase it by 50%, -50 will reduce it by 50%.
  * Default Value: 0

Pickaxe Skill gain factor [Synced with Server]
* Pickaxe skill gain factor. This is a modifier value. 50 will increase it by 50%, -50 will reduce it by 50%.
  * Default Value: 0

WoodCutting Skill gain factor [Synced with Server]
* WoodCutting skill gain factor. This is a modifier value. 50 will increase it by 50%, -50 will reduce it by 50%.
  * Default Value: 0

Jump Skill gain factor [Synced with Server]
* Jump skill gain factor. This is a modifier value. 50 will increase it by 50%, -50 will reduce it by 50%.
  * Default Value: 0

Run Skill gain factor [Synced with Server]
* Run skill gain factor. This is a modifier value. 50 will increase it by 50%, -50 will reduce it by 50%.
  * Default Value: 0

Sneak Skill gain factor [Synced with Server]
* Sneak skill gain factor. This is a modifier value. 50 will increase it by 50%, -50 will reduce it by 50%.
  * Default Value: 0

Swim Skill gain factor [Synced with Server]
* Swim skill gain factor. This is a modifier value. 50 will increase it by 50%, -50 will reduce it by 50%.
  * Default Value: 0

Death Penalty Factor Multiplier [Synced with Server]
* Change the death penalty in percentage, where higher will increase the death penalty and lower will reduce it. This is a modifier value. 50 will increase it by 50%, -50 will reduce it by 50%.
  * Default Value: 0

`Extended Inventory`

ExtraRows [Synced with Server]
* Number of extra ordinary rows. (This can cause overlap with chest GUI, make sure you hold CTRL (the default key) and drag to desired position)
  * Default Value: 0

AddEquipmentRow [Synced with Server]
* Add special row for equipped items and quick slots. (IF YOU ARE USING RANDY KNAPPS EAQs KEEP THIS VALUE OFF)
  * Default Value: false

DisplayEquipmentRowSeparate [Synced with Server]
* Display equipment and quickslots in their own area. (IF YOU ARE USING RANDY KNAPPS EAQs KEEP THIS VALUE OFF)
  * Default Value: false

HelmetText [Not Synced with Server]
* Text to show for helmet slot.
  * Default Value: Head

ChestText [Not Synced with Server]
* Text to show for chest slot.
  * Default Value: Chest

LegsText [Not Synced with Server]
* Text to show for legs slot.
  * Default Value: Legs

BackText [Not Synced with Server]
* Text to show for back slot.
  * Default Value: Back

UtilityText [Not Synced with Server]
* Text to show for utility slot.
  * Default Value: Utility

QuickAccessScale [Not Synced with Server]
* Scale of quick access bar.
  * Default Value: 1

HotKey1 [Not Synced with Server]
* Hotkey 1 - Use https://docs.unity3d.com/Manual/ConventionalGameInput.html
  * Default Value: Z

HotKey2 [Not Synced with Server]
* Hotkey 2 - Use https://docs.unity3d.com/Manual/ConventionalGameInput.html
  * Default Value: X

HotKey3 [Not Synced with Server]
* Hotkey 3 - Use https://docs.unity3d.com/Manual/ConventionalGameInput.html
  * Default Value: C

HotKey1 Text [Not Synced with Server]
* Hotkey 1 Display Text. Leave blank to use the hotkey itself.
  * Default Value:

HotKey2 Text [Not Synced with Server]
* Hotkey 2 Display Text. Leave blank to use the hotkey itself.
  * Default Value:

HotKey3 Text [Not Synced with Server]
* Hotkey 3 Display Text. Leave blank to use the hotkey itself.
  * Default Value:

ModKey1 [Not Synced with Server]
* First modifier key to move quick slots. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html format.
  * Default Value: Mouse0

ModKey2 [Not Synced with Server]
* Second modifier key to move quick slots. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html format.
  * Default Value: LeftControl

quickAccessX [Not Synced with Server]
* Current X of Quick Slots (Not Synced with server)
  * Default Value: 9999

quickAccessY [Not Synced with Server]
* Current Y of Quick Slots (Not Synced with server)
  * Default Value: 9999

`General`

ChestInventoryX [Not Synced with Server]
* Current X of chest (Not Synced with server)
  * Default Value: -1

ChestInventoryY [Not Synced with Server]
* Current Y of chest (Not Synced with server)
  * Default Value: -1

ModKeyOne [Not Synced with Server]
* First modifier key (to move the container). Use https://docs.unity3d.com/Manual/class-InputManager.html format.
  * Default Value: Mouse0

ModKeyTwo [Not Synced with Server]
* Second modifier key (to move the container). Use https://docs.unity3d.com/Manual/class-InputManager.html format.
  * Default Value: LeftControl

`Inventory Discard`

Enabled [Synced with Server]
* Enable Inventory Discard Section
  * Default Value: false

DiscardHotkey [Not Synced with Server]
* The hotkey to discard an item
  * Default Value: Delete

ReturnUnknownResources [Synced with Server]
* Return resources if recipe is unknown
  * Default Value: false

ReturnEnchantedResources [Synced with Server]
* Return resources for Epic Loot enchantments
  * Default Value: false

ReturnResources [Synced with Server]
* Fraction of resources to return (0.0 - 1.0)
  * Default Value: 1

`CraftFromContainers`

CFC Enabled [Synced with Server]
* Enable CraftFromContainers code
  * Default Value: true

ContainerRange [Synced with Server]
* The maximum range from which to pull items from
  * Default Value: 10

ResourceCostString [Not Synced with Server]
* String used to show required and available resources. {0} is replaced by how much is available, and {1} is replaced by how much is required. Set to nothing to leave it as default.
  * Default Value: {0}/{1}

FlashColor [Not Synced with Server]
* Resource amounts will flash to this colour when coming from containers
  * Default Value: FFEB04FF

UnFlashColor [Not Synced with Server]
* Resource amounts will flash from this colour when coming from containers (set both colors to the same color for no flashing)
  * Default Value: FFFFFFFF

PulledMessage [Not Synced with Server]
* Message to show after pulling items to player inventory
  * Default Value: Pulled items to inventory

FuelDisallowTypes [Synced with Server]
* Types of item to disallow as fuel (i.e. anything that is consumed), comma-separated. Uses Prefab names.
  * Default Value: RoundLog,FineWood

OreDisallowTypes [Synced with Server]
* Types of item to disallow as ore (i.e. anything that is transformed), comma-separated). Uses Prefab names.
  * Default Value: RoundLog,FineWood

ItemDisallowTypes [Synced with Server]
* Types of items to disallow pulling from chests, comma-separated. Uses Prefab names.
  * Default Value:

ShowConnections [Not Synced with Server]
* If true, will display connections to nearby workstations within range when building containers
  * Default Value: false

ConnectionStartOffset [Not Synced with Server]
* Height offset for the connection VFX start position
  * Default Value: 1.25

ConnectionRemoveDelay [Not Synced with Server]
*
  * Default Value: 0.05

SwitchPrevent [Not Synced with Server]
* If true, holding down the PreventModKey modifier key will allow this mod's behavior; If false, holding down the key will prevent it.
  * Default Value: false

PreventModKey [Not Synced with Server]
* Modifier key to toggle fuel and ore filling behaviour when down. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html
  * Default Value: LeftAlt

PullItemsKey [Not Synced with Server]
* Holding down this key while crafting or building will pull resources into your inventory instead of building. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html
  * Default Value: LeftControl

FillAllModKey [Not Synced with Server]
* Modifier key to pull all available fuel or ore when down. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html
  * Default Value: LeftShift

`BiFrost`

Position of the UI [Not Synced with Server]
* Sets the anchor position of the UI
  * Default Value: {"x":-900.0,"y":200.0}

Disable [Not Synced with Server]
* Disables the GUI for the BiFrost
  * Default Value: true

Show Password Prompt [Not Synced with Server]
* Set to true if you want to still show the password prompt to the user. This is for servers that have a password but don't wish to use the file to keep the password.
  * Default Value: false


### Having issues? Post them on my GitHub for me to not forget! (https://github.com/AzumattDev/OdinsQOL/issues)

# Author Information

### Azumatt

`DISCORD:` Azumatt#2625

`STEAM:` https://steamcommunity.com/id/azumatt/

For Questions or Comments, find me in the Odin Plus Team Discord or in mine:

[![https://i.imgur.com/XXP6HCU.png](https://i.imgur.com/XXP6HCU.png)](https://discord.gg/Pb6bVMnFb2)
<a href="https://discord.gg/pdHgy6Bsng"><img src="https://i.imgur.com/Xlcbmm9.png" href="https://discord.gg/pdHgy6Bsng" width="175" height="175"></a>




# Changelog (latest patch listed first)

### 0.9.6
- CFC fixes for default wards

### 0.9.5
- Mistlands update fix.

### 0.9.3/0.9.4
- Hotfix for current game version. Update ServerSync internally.
- Previous comments about last patch apply here as well. Only updating because it's needed. The changes are not in this codebase.

#### 0.9.2
- Add back the BiFrost!
- Add incompatibility checks to the needed mods.
- MapDetails bug fix. I was comparing the variables wrong, so it was always executing even when it shouldn't.
- Add configuration options to control the food degredation and the food regen.
  - Both can be found in the "Player" section of the config.
    - No Food Degrade: This is the amount of food that is lost per second. This configuration disables or enables
      food degrading. Modify food must be on.
    - Modify food: Ensuring the food lasts longer while maintaining the same rate of regeneration. Needed to be on
      for No Food Degrade to work
- Add configuration option to show the game's default screen flash when taking damage. Was previously hard coded to be
  off.
- Began collapsing super old change logs for the mod, to reduce the appeared size of the readme on the site. The old change logs can be found
  in the collapsed sections below.
#### 0.9.1

* Fix MaxPlayerCount not working as intended.

#### 0.9.0

* Updated to work with Game Version 0.211.7
  * BiFrost temporarily disabled due to issues with the new version, will work out a fix soon, until then, use their
    new server saving!
* Code change for Max Players allowed in the server to work with latest version.

#### 0.8.8

* Change auto repair default value to false and change to sync with server's setting. Requested by Tyson#28262
* Remove the ability to override your player name in the chatbox. Valid complaints with this feature have been made, and
  I have decided to remove it. I had forgotten that this was a feature, and I apologize for the inconvenience.
* Fixed a bug where crafting items while your inventory was full could cause them to disappear.
* Fix issue created in last version if you didn't have the BiFrost enabled. Reported by Tyson#28262. I forgot that the
  BiFrost is optional, and didn't code for that. My bad.
* Iron chest rows defaulted to 4
* Display Fermenter progress on hover
* Centered the clock on the screen.
* ShowBuildings under MapDetails now determines buildings are painted on the map. This allows for map details to be on,
  and show carts without buildings showing on the map. Previously this was a dead config (not sure why).
* MapDetails default value is now false
* Remove some unused code. Fixed some wording.
* Auto store now allows for custom chests to be added to a list. Affected by ItemDisallowCustomChests,
  ItemAllowCustomChests and DropRangeCustomChests values.
* CFC now has a configuration option for ItemDisallowTypes, to prevent certain items from being pulled into the player
  inventory.

#### 0.8.7

* Mimic FastLink v1.1.0 update. Allow prompting for password still. Disable GUI when in loading screen.
* Add config option to disable the portal hover tag (thanks to @wackymole)
* Keep an eye out for another update this weekend, I'm working on changing more things. This update was released to fix
  some compatiblity issues with other mods.

#### 0.8.6

* WardIsLove v 3.0.1 compatibility update.

#### 0.8.5

* Turn QuickAccessBar into a HotKeyBar (Thanks to [Blaxxun](https://github.com/blaxxun-boop) in
  a [GitHub commit](https://github.com/AzumattDev/OdinPlusQOLMods/commit/56f5d28a98c24b9a965463ae0720ab62582de25d)).
  Reflected in OdinQOL.
  * This provides some compatibility with Jewelcrafting and other mods that might need to modify the QuickAccessBar.
* Added new config options for hotkey texts and allow overflow of hotkey bindings.
  * You can now override the hotkey text for each hotkey. Should you need it to be different than the default.

##### 0.8.4

* Update SkillGainFactor config descriptions to make more sense.
* Potentially game breaking fixes
  * Fix a bug where the requirement was more than the max stack size or the item can't be stacked would cause a rouge
    item to be left in the chest. (Thanks to Lime18 for the bug report & Bjorn for the fix!)
  * Fix a bug where pulling resources while having the stack size multiplied would cause an increase in the stack size
    & sometimes the actual item count. (Thanks to Bjorn for the bug report & fix!)
  * Fix for Pulling resources into the player inventory not grabbing all resources needed to craft if the item doesn't
    stack. (Found when testing Bjorn's fix!)
* Remove incompatibility with Mod Settings mod. Will add back if issues with that mod arise.

##### 0.8.3

* Fixed CFC FillAllKey not working. Keyboardshortcut was always returning false, not sure why.
* Fix Item Stack bug if exact resources are used.
* Item Drawer stack compat fix.
* Add switch toggles for Dungeons and Camp alteration.
* Add switch toggle for workbench alteration.
* Fix issue with workbench radius disappearance when workbench range is set to 20.
* Fix some issues found in the legacy code while fixing other code for this patch version. (aka...ghost configs)

##### 0.8.2

* Update Structural integrity to be in percentage values, also update the config descriptions to make more sense.
* Fix issues with the BiFrost toggle
* Fix newly created issue that cause the clock to no longer appear
* Fix issue with MapDetails

##### 0.8.1

* Fix a mess up

##### 0.8.0

* Add FileWatcher to the code. Changes made directly to the configuration files on the server should sync with the
  clients upon saving the file. Keep in mind, depending on the setting changed, it might require a client reboot to see
  the change.
* Integrate CraftFromContainers and provide a little boost to the performance of it
  * Please note, `if CFC is installed, this mod will not load.`
  * The CFC code has been updated to provide compatibility with WardIsLove wards, no more pulling from chests you
    aren't allowed to. (I hope!)
  * CraftFromContainers section in the config file is the new location for all things CFC in OdinQOL.
* Remove ConnectionPanel code/configs. Integrated my FastLink code but renamed to Bifröst. Works the
  same! This is to allow for passwords, support of IPv6 and a quicker launch via UI.
  * `(It is highly recommended that you
    regenerate your configuration file due to
    the removal of the ConnectionPanel configs. BepInEx doesn't handle orphaned config entries)`
    * The file you can configure your servers for OdinQOL is `com.odinplusqol.mod_servers.yml`
      * If you happen to have FastLink installed, these are compatible and can be ran at the same time. Shouldn't
        be any
        issues. You can disable OdinQOLs version of it in the configs under BiFrost section.
* Fix the placement issue some people were having. Seems there was a rouge patch that didn't check for the configuration
  value.
* Beehive config value default changed to vanilla default of 1200
* Fix min_room count for dungeons to match vanilla value. Max rooms kept at 20 just in case there is lag associated with
  this value change.
* Auto Storage changes. Add in blackmetal chest configs. Optimize code.
* Stack size configuration option has been changed in how it functions. It's now a direct multiplier.
  * `Multiplier for your item stacks. Directly multiplies stack of the item by this value. (Stack of 50 with value of 10 here would turn into a stack of 500)`
* Fix some of the descriptions for configs.
* Various other internal code updates/refactors. Some updates provide better performance, so the mod should run smoother
  now.
* Incompatibility added to the new "Mod Settings" mod by Jules/Jotunn. There were issues in the past where this mod
  would allow bypassing configuration options while it was inside Jotunn codebase. Though this might not be the case
  anymore, I won't risk it happening once
  more.

<details>

<summary><b>0.7.1</b></summary>

* Compatibility with Shipyard by adding 3 configuration options to the "Containers" section.
  * Turning off the entire section, turning off the ships and chest container size individually.
* Adding back displaying ships and carts on the map (by request). The toggle for this has been moved to "Map Details"
  section in the configs.

</details>

<details>
<summary><b>0.7.0</b></summary>

* Remove Map Syncing code and all configs related
  * Benefits to this...you can now use the vanilla pin UI again for quick pins of certain types. This code blocked the
    addition of the pin UI Valheim devs added.
  * Downside, it removes the Ship and Cart display on the map since it was buried in that code and I'm a little lazy.
    If enough requests ask to add this feature back, I will.
  * Downside, this could cause issues with your configuration files having orphaned config sections since I have
    removed them. You might have to regenerate your config file on the server and client.
* Update to latest CraftFromContainers internally. Soon, I will absorb this code and the DLL will not be needed.
* I (Azumatt) will be the primary maintainer of OdinQOL. The code and all history of git have been moved to my github as
  a result. [OdinsQOL on My GitHub](https://github.com/AzumattDev/OdinsQOL)
</details>

<details>
<summary><b>0.6.0</b></summary>

* Compile against latest game version
  * Fix issues with the save data paths after patch.
  * Toggle the new modded variable.
</details>

<details>
<summary><b>0.5.2</b></summary>

* Update Signs Code
  * Cache default material for sign text before modifications so we can swap back to vanilla at anytime.
  * Rich text being on/off now dictates vanilla or custom colors. (Rich text must be on for default color to work,
    otherwise it's vanilla color.)
</details>

<details>
<summary><b>0.5.1</b></summary>

* PR from Blaxxun
  * Fix version check when installed with other mods that do it similarly.
</details>

<details>
<summary><b>0.5.0</b></summary>

* Update Signs Code
  * Add config to change deafult color of signs
  * Change default font setting to actual default font from game. (Not sure why it wasn't to start)
  * Note: Default color is a string like "red" or "black". It doesn't touch signs that have already been changed by
    richtext markup. (a.k.a. <color=red></color>)
* Update ServerSync version internally
* Update dependency strings in manifest.json
* Update readme's features listing
</details>

<details>
<summary><b>0.4.11</b></summary>

* Took PR from Azumatt
  * Removed MMHook Dependency
  * Added Black Metal Chest to configurable storage sizes
  * Updated github project to be able to compile on any machine
</details>

<details>
<summary><b>0.4.1</b></summary>

* Fixed for caves patch
</details>

<details>
<summary><b>0.4.0</b></summary>

* Update to latest ServerSync.dll. This should fix some syncing errors.
</details>

<details>
<summary><b>0.3.9</b></summary>

* HoverText issue fixed.
</details>

<details>
<summary><b>0.3.8</b></summary>

* Fix a derp. Forgot to include CraftFromContainers
</details>

<details>
<summary><b>0.3.7</b></summary>

* MapDetails and ShowContainer contents updated
</details>

<details>
<summary><b>0.3.6</b></summary>

* Make dungeon stuff configurable
</details>

<details>
<summary><b>0.3.5</b></summary>

* Update stamina drain values to be vanilla defaults. Note: They are direct drain values, not percent, some people were
  asking about that.
</details>

<details>
<summary><b>0.3.3</b></summary>

* Added our name to the version string check, not just the version number.
* Addition of the Connection Panel configuration. You can now add servers to the Join Game menu on the main screen.
  Examples provided; Option is off by default.
  * These should be separated by commas! This option is client side only, they load right before the main menu shows.
  * The previous hard coded values were removed, it was felt that it should be a configurable option if you choose to
    use it like we did.
</details>

<details>
<summary><b>0.3.2</b></summary>

* Fix UI bug with QuickSlots (Thank you Metréu and ZurielRedux for reporting it!)
  * It is highly recommended that you regenerate your configuration file!! (Delete the old one and reboot the server
    or client with the new DLL loaded)
</details>

<details>
<summary><b>0.3.1</b></summary>

* Fix crashing bug
</details>

<details>
<summary><b>0.3.0</b></summary>

* Fix a *really* weird issue with RRR compatibility. This should fix the "Unable to find prefab" issues with this mod
  and RRR.
* Added more information to the configs to help you tell what is synced with server and what is not.
* Change some configs to be actual KeyCodes and not the string values for extra quality of life in configuring the mod
</details>

<details>
<summary><b>0.2.0</b></summary>

* Quickslots fix
* Move all related quickslot and Extended Inventory to it's own section. Update some config values to be vanilla
  defaults.
</details>

<details>
<summary><b>0.1.0</b></summary>

* Updated config values and descriptions
* Add extended inventory
* EAQs added
* Moveable chest inventory in case you make your slots too large
* Option to configure crafting time added
* Made some hotkeys not server sync. Ask questions in the discord if you have issues or concerns.
</details>

<details>
<summary><b>0.0.11</b></summary>

* Fixing some issues that H&H introduced
  *Added HookGenPatcher as the appropriate dependency for some of the patches. Forgot to in the past.
</details>

<details>
<summary><b>0.0.10</b></summary>

* Fixing Thunderstore to match mod version and updating readme with features
</details>

<details>
<summary><b>0.0.9</b></summary>

* Fixing Thunderstore to match mod version and updating readme with features
</details>

<details>
<summary><b>0.0.1</b></summary>

* Initial release
</details>
