## IMPORTANT STUFF

This mod contains a collection of patches from various sources (all MIT/GNU GPL licensed sources) in order to bring you
the best collection of QOL and tweaks in one spot

`Install on all clients and the server.`

`This mod uses ServerSync, if installed on the server and the config is set to be forced, it will sync all configs to client`

`This mod uses a file watcher. If the configuration file is not changed with BepInEx Configuration manager, but changed in the file directly on the server, upon file save, it will sync the changes to all clients. Please keep in mind that logging out or rebooting might be required for some configuration changes, as not everything can update immediately`

### Having issues? Post them on my GitHub for me to not forget! (https://github.com/AzumattDev/OdinsQOL/issues)

# Author Information

### Azumatt

`DISCORD:` Azumatt#2625

`STEAM:` https://steamcommunity.com/id/azumatt/

### Thank you Blaxxun! - [Blaxxun YAML contribution](https://github.com/AzumattDev/FastLink/commit/bfcf290c83e785ced14e3c2d93da2739e86b3102)

For Questions or Comments, find me in the Odin Plus Team Discord or in mine:

[![https://i.imgur.com/XXP6HCU.png](https://i.imgur.com/XXP6HCU.png)](https://discord.gg/Pb6bVMnFb2)
<a href="https://discord.gg/pdHgy6Bsng"><img src="https://i.imgur.com/Xlcbmm9.png" href="https://discord.gg/pdHgy6Bsng" width="175" height="175"></a>




# Changelog (latest patch listed first)

### 0.9.3/0.9.4
- Hotfix for current game version. Update ServerSync internally.
- Previous comments about last patch apply here as well. Only updating because it's needed. The changes are not in this codebase.

#### 0.9.2

- `FUTURE WARNING: READ CLOSE!`
    - `This patch will be the last patch that will be compatible with the old config system. I will be making a massive change to the configurations for this mod. Forcing a regeneration of the new config file mandatory. I plan on making the configs make more sense, things that were hard coded to have configs, and to have options to enable or disable entire sections.`
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