## IMPORTANT STUFF

This mod contains a collection of patches from various sources (all MIT/GNU GPL licensed sources) in order to bring you the best collection of QOL and tweaks in one spot


# Changelog (latest patch listed first)
##### 0.6.0
* Compile against latest game version
	* Fix issues with the save data paths after patch.
  * Toggle the new modded variable.
##### 0.5.2
* Update Signs Code
	* Cache default material for sign text before modifications so we can swap back to vanilla at anytime.
  * Rich text being on/off now dictates vanilla or custom colors. (Rich text must be on for default color to work, otherwise it's vanilla color.)
##### 0.5.1
* PR from Blaxxun
	* Fix version check when installed with other mods that do it similarly.
##### 0.5.0
* Update Signs Code
	* Add config to change deafult color of signs
	* Change default font setting to actual default font from game. (Not sure why it wasn't to start)
	* Note: Default color is a string like "red" or "black". It doesn't touch signs that have already been changed by richtext markup. (a.k.a. <color=red></color>)
* Update ServerSync version internally
* Update dependency strings in manifest.json
* Update readme's features listing
##### 0.4.11
* Took PR from Azumatt
	* Removed MMHook Dependency
	* Added Black Metal Chest to configurable storage sizes
	* Updated github project to be able to compile on any machine
##### 0.4.1
* Fixed for caves patch
##### 0.4.0
* Update to latest ServerSync.dll. This should fix some syncing errors.
##### 0.3.9
* HoverText issue fixed.
##### 0.3.8
* Fix a derp. Forgot to include CraftFromContainers
##### 0.3.7
* MapDetails and ShowContainer contents updated
##### 0.3.6
* Make dungeon stuff configurable
##### 0.3.5
* Update stamina drain values to be vanilla defaults. Note: They are direct drain values, not percent, some people were asking about that.
##### 0.3.3
* Added our name to the version string check, not just the version number.
* Addition of the Connection Panel configuration. You can now add servers to the Join Game menu on the main screen. Examples provided; Option is off by default.
  * These should be separated by commas! This option is client side only, they load right before the main menu shows.
  * The previous hard coded values were removed, it was felt that it should be a configurable option if you choose to use it like we did.
##### 0.3.2
  * Fix UI bug with QuickSlots (Thank you Metréu and ZurielRedux for reporting it!)
    * It is highly recommended that you regenerate your configuration file!! (Delete the old one and reboot the server or client with the new DLL loaded)
##### 0.3.1
  * Fix crashing bug
##### 0.3.0
  * Fix a *really* weird issue with RRR compatibility. This should fix the "Unable to find prefab" issues with this mod and RRR.
  * Added more information to the configs to help you tell what is synced with server and what is not.
  * Change some configs to be actual KeyCodes and not the string values for extra quality of life in configuring the mod

##### 0.2.0 
  * Quickslots fix
  * Move all related quickslot and Extended Inventory to it's own section. Update some config values to be vanilla defaults.

##### 0.1.0 
  * Updated config values and descriptions
  * Add extended inventory
  * EAQs added
  * Moveable chest inventory in case you make your slots too large
  * Option to configure crafting time added
  * Made some hotkeys not server sync. Ask questions in the discord if you have issues or concerns.

##### 0.0.11
  * Fixing some issues that H&H introduced
  *Added HookGenPatcher as the appropriate dependency for some of the patches. Forgot to in the past.

##### 0.0.10 
  * Fixing Thunderstore to match mod version and updating readme with features

##### 0.0.9 
  * Fixing Thunderstore to match mod version and updating readme with features

##### 0.0.1 
  * Initial release

# Features

[Auto Storage]
- Set range to pull dropped items for Chests, Personal chests, Re-inforced Chests, Carts, Ships
- Set item types to allow/disallow pulling for.

[Building HUD]
- Show Amount per item, change color of amount text, and show how much of an item can be build with current inventory.

[Clock]
- Display a clock on screen, and/or when it will be displayed.
  -settings to adjust duration/font/hotkey/display type

[Connection Panel]
- Add server(s) to the Join Game panel on the main menu.
  - Set color of server name in list

[Containers]
- Add/remove additonal Rows/Columns for Karve, Longboat, Cart, Personal Chest, Wood Chest, Iron Chest

[Dungeon/Camps]
- Set min/max number of rooms placed by dungeons
- Set min/max radious of Goblin Camps

[Extended Inventory]
- Add additional Rows to Player inventory
- Ability to add special row for equipped items and quick slots
- Ability to display equipment and quick slots in separate area outside of inventory
  - Can create custom names for each equipment and quickslot
- Can adjust scale of quick access bar.
- Adjust position of quick access bar

[Game]
- Disable I have arrived message
- Allow Building Inside Protected Locations
- Change Crafting Duration time.
- Disable Guardian Animation for the players
- Skip Tutorials
- Honey Production Speed and Honey count per hive

[General]
- Lock Configuration
- Enable the entire mod
- Show debug messages in log
- custom spawn chest location x,y and capacity

[Inventory Discard]
- Enable Inventory Discard
- Return resources if recipe is unknown
- Return resources for Epic Loot enchantments
- partial resources to return on discard 

[Items]
- Adjust weight of items
- Adjust stack size
- Disable Teleport check for items
- Change item sort from inventory to chest
- Auto repair your equipment when interacting with build station

[Map Details]
- Range around player to show details
- Distance to move before automatically updating the map details
- Show building pieces
- Color of one's own build pieces
- Color of other players' build pieces
- Color of npc build pieces

[Maps]
- Share Map Progress with others
- Share pins/all pins with other players
- Show Boats and carts on the map
- Adjustable explore radius

[Names]
- Override your player name shown in-game and in the chat box

[PlantGrowth]
- Display growth progress in hover text
- Don't require cultivated ground to plant anything
- Allow planting anything in any biome.
- Allow planting under roofs.
- Prevent plants from being planted if they are too close together to grow.
- Prevent destruction of plants that normally are destroyed if they can't grow.
- Adjustable growth time
- Adjustable growth radius needed
- Adjustable plant scale

[Player]
- Re-equip Items After Swimming
- Automatically repair build pieces within the repair radius
- Adjustable Repair Radius for build pieces
- Meginjord buff amount
- Auto pickup range
- Cam Shake factor
- Base max weight addition for player
- Stamina alterations
  - Dodge Stamina Usage
  - Encumbered Stamina drain
  - Sneak stamina drain
  - Run Stamina Drain
  - Delay before stamina regeneration starts
  - Stamina regen factor
  - Stamina drain from swimming
  - Stamina drain factor for jumping


[Server]
- Max number of Players to allow in a server

[Show Chest Contents]
- Number of entries to show
- Sort type (value, ascending, etc)


[Signs]
- Sign Font/Scale/Font size

[Skills]
- Toggeable Display notifications for skills gained
- Change skill gain factor for Unarmed, Weapons, Tools, Running, Swimming, Jumping, Death penalty

[WearNTear_Patches]
- No Weather Damage to buildings
- Disable Structural Integrity system
- Disable Boat Damage
- No Damage to player buildings
- Wood Structural Integrity
- Stone Structural Integrity
- Iron Structural Integrity
- Hardwood Structural Integrity

[WorkBench]
- Range you can build from workbench
- Workbench PlayerBase radius
- Show building pieces
- Range for workbench extensions
- Build Distance

* #### ... AND MORE!

Feel free to discuss things here:

https://discord.gg/nsEYWab3AY