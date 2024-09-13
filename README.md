# VIP-Plugin

## Description
VIP-Plugin is a simple plugin designed for CS2 server owners, enabling them to establish VIP groups through the CSS Permission system for authentication.

## Features
- Database Integration: Utilize a SQL database to store VIP services.
- VIP Group Management: Define and configure VIP groups with various perks and limitations.
- Custom Event Bonuses: Grant special bonuses to VIPs upon events like spawning, kills, or bomb interactions.
- Dynamic Configuration: Customize VIP settings such as health, armor, money, grenades, and more.
- Connect/Disconnect Messages: Display custom messages when VIPs join or leave the server.
- Random VIP Selection: Automatically select a VIP after a specified number of rounds.
- Night VIP System: Implement a special VIP status based on server time conditions.
- Player Information Commands: Retrieve detailed information about players and their services.
- Service Management Commands: Enable, disable, delete, and retrieve information about services.

## Dependencies
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp/releases) v201
- [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master)

## Instalation
1. Download the [latest release](https://github.com/CS-GEJMERZY/VIP-Plugin/releases/latest)
2. Unzip the package and upload files to **_csgo/addons/counterstrikesharp/plugins_**

## Configuration 
<details>
<summary>Upon the first launch, the <b>VIP-Plugin.json</b>  file will be automatically created in <b>csgo/addons/counterstrikesharp/configs/plugins/VIP-Plugin</b> </summary>

```
{
  "Settings": {
    "Prefix": "{lightred}VIP " // // Text displayed before plugin's chat messages
    "Database": { 
      "Enabled": false, // disabling this will deactivate all features reliant on the database
      "SqlServer": {
        "host": "www.site.com",
        "port": 3306,
        "database": "vip-plugin",
        "username": "user",
        "password": "password",
        "prefix": "" // table prefix 
      }
    },
    "DatabaseVipsConfig": {
      "Enabled": false,
      "Commands": {
        "css_vp_service_enable": {
          "Enabled": true
        },
        "css_vp_service_disable": {
          "Enabled": true
        },
        "css_vp_service_delete": {
          "Enabled": true
        },
        "css_vp_service_info": {
          "Enabled": true
        },
        "css_vp_player_info": {
          "Enabled": true
        },
        "css_vp_player_info": {
          "Enabled": true
        },
        "css_vp_player_addflags": {
          "Enabled": true
        },
        "css_vp_player_addgroup ": {
          "Enabled": true
        }
      }
    }
  },
  "VIPGroups": [
    {
      "Permissions": "@vip-plugin/vip", // CSS permission required for this VIP group
      "Priority": 1, // if a player has multiple groups, one with higher priority will be chosen
      "UniqueId": "vip1", // used for storing services in DB, must be unique
      "Name": "VIP", // Name of the VIP group
      "Events": {
        "Spawn": {// Bonuses given to VIPs upon spawn
          "HP": 105, // Player's HP
          "ArmorValue": 100, // Armor value (0 = no armor) 
          "Helmet": true, // Whether to give a helmet
          "HelmetOnPistolRound": false, // Give helmet on pistol round?
          "DefuseKit": true, // Give defuse kit?
          "Zeus": true, // Give Zeus x27?
          "ZeusOnPistolRound": true, // Give Zeus on pistol round?
          "HealthshotAmount": 1,  // Number of healthshots
          "HealthshotOnPistolRound": true, // Give healthshot on pistol round?
          "ExtraMoney": 2000, // Bonus money
          "ExtraMoneyOnPistolRound": false, // Give extra money on pistol round?
          "Grenades": { // Grenade amounts(0 = don't give)
            "Smoke": 1,
            "HE": 1,
            "Flashbang": 1,
            "FireGrenade": 1,
            "Decoy": 0
          }
        },
        "Kill": { // Rewards for kills
          "HP": 2, // HP gained on normal kill
          "HeadshotHP": 3, // HP gained on headshot kill
          "Money": 200, // Money for normal kill
          "HeadshotMoney": 300 // Money for headshot kill
        },
        "Bomb": { // Rewards for bomb interactions
          "PlantMoney": 500, // Money for planting the bomb
          "DefuseMoney": 500 // Money for defusing the bomb
        },
        "Round": {  // Rewards based on round outcome
          "WinMoney": 1000, // Money for winning a round
          "LoseMoney": 0 // Money for losing a round
        }
      },
      "Limits": {
        "MaxHP": 120, // Maximum health
        "MaxMoney": 16000 // Maximum money
      },
      "Misc": {
        "ExtraJumps": { // Extra jump options
          "Amount": 0, // Number of extra jumps (0 = disabled, 1 = double jump etc.)
          "VelocityZ": 260, // Jump height
          "NoFallDamage": true // // Disable fall damage after extra jumps
        },
        "Bhop": {
          "Enabled": false, 
          "VelocityZ": 260, // the height of a jump
        },
        "Smoke": {
          "Enabled": false, // Enable colored smoke
          "Type": 0, // 0 = Fixed color, 1 = Random color
          "Color": "#FF0000" // Smoke color in hex code
        },
        "HealthRegen": {
          "Enabled": false,
          "Interval": 5, // add HP every 'Interval"
          "Delay": 5, // delay after round start
          "Amount": 5 // amount of HP, limited by Limits.MaxHp
        },
        "ArmorRegen": {
          "Enabled": false,
          "Interval": 5, // add Armor every 'Interval"
          "Delay": 5, // delay after round start
          "Amount": 5 // amount of Armor, limited by 100
        },        
        "FastPlant": {
          "Enabled": false,
          "Modifier": 0.5, // 1 = normal speed, 0.5 = 50% faster, 0.1 = 10% of the normal speed required etc.
          "TimeAfterRoundStart" : 0 // time after round start for the feature to start working
        },
        "FastDefuse": {
          "Enabled": false,
          "Modifier": 1, // 1 = normal speed, 0.5 = 50% faster, 0.1 = 10% of the normal speed required etc.
          "TimeAfterRoundStart" : 0 // time after round start for the feature to start working
        },
        "NoFallDamageGlobal": false, // Disable fall damage globally for the VIP group
        "Gravity": 1, // Gravity level (1 = normal)
        "Speed": 1 // Movement speed multiplier (1 = normal, 1.05 = 5 % faster)
      },
      "Messages": {
        "Chat": {
          "Connect": {
            "Enabled": true,
            "Message": "{darkred}VIP {default}{playername} joined the server",
            "DontBroadcast": true // Hide default connect message
          },
          "Disconnect": {
            "Enabled": true,
            "Message": "{darkred}VIP {default}{playername} left the server",
            "DontBroadcast": true // Hide default disconnect message
          }
        }
      }
    }
  ],
  "RandomVIP": {
    "Enabled ": false,
    "AfterRound ": 3, // Round after which to choose a VIP
    "MinimumPlayers ": 2, // Minimum players required
    "RepeatPickingMessage": 3, // repeat the 'picking random vip' message
    "PermissionsGranted ": [], // List of CSS permissions
    "PermissionExclude": [] // Exclude players with these permissions
  },
  "NightVIP": {
    "Enabled": false,
    "SendMessageOnVIPReserved": false, // res
    "StartHour": 22, // The hours can be 8-22 or 22-8(which mean from 22 to 24 and 00 to 08)
    "EndHour": 8,
    "TimeZone": "UTC", // TZ indentifier
    "RequiredNickPhrase": "YourSite.com", // Required nickname phrase(empty = pass)
    "RequiredScoreboardTag": "YourSite.com", // Required scoreboard tag(empty = pass)
    "PermissionsGranted ": [], // List of CSS permissions
    "PermissionExclude": [] // Exclude players with these permissions
  },
  "ConfigVersion": 1
}
```
</details>

##  Commands
<details>
<summary><strong>css_vp_service_enable</strong></summary>

Set the availability status of a service to **Enabled**.

- `<service_id>`: The ID of the service to enable.

**Syntax**: `css_vp_service_enable <service_id>`

**Example**: `css_vp_service_enable 1`
</details>

<details>
<summary><strong>css_vp_service_disable</strong></summary>

Set the availability status of a service to **Disabled**.

- `<service_id>`: The ID of the service to disable.

**Syntax**: `css_vp_service_disable <service_id>`

**Example**: `css_vp_service_disable 2`
</details>

<details>
<summary><strong>css_vp_service_delete</strong></summary>

Delete a service.

- `<service_id>`: The ID of the service to delete.

**Syntax**: `css_vp_service_delete <service_id>`

**Example**: `css_vp_service_delete 3`
</details>

<details>
<summary><strong>css_vp_service_info</strong></summary>

View detailed information about a service.

- `<service_id>`: The ID of the service to get information about.

**Syntax**: `css_vp_service_info <service_id>`

**Example**: `css_vp_service_info 4`
</details>

<details>
<summary><strong>css_vp_player_info</strong></summary>

View service info menu.

**Syntax**: `css_vp_player_info`

**Example**: `css_vp_player_info`
</details>

<details>
<summary><strong>css_vp_player_info</strong></summary>

Get information about a player.

- `<steamid64>`: The Steam ID of the player to retrieve information for.

**Syntax**: `css_vp_player_info <steamid64>`

**Example**: `css_vp_player_info 76561198012345678`
</details>

<details>
<summary><strong>css_vp_player_addflags</strong></summary>

Add flags to a player for a specified duration.

- `<steamid64>`: The Steam ID of the player to add flags to.
- `<duration>`: Duration in minutes for which the flags will be active.
- `<flag1> ...`: Flags to add to the player.

**Syntax**: `css_vp_player_addflags <steamid64> <duration> <flag1> ...`

**Example**: `css_vp_player_addflags 76561198012345678 30 @css/root @vip-plugin/vip`
</details>

<details>
<summary><strong>css_vp_player_addgroup</strong></summary>

Add a group to a player for a specified duration.

- `<steamid64>`: The Steam ID of the player to add the group to.
- `<duration>`: Duration in minutes for which the group will be active.
- `<group_id>`: The Unique ID of the group to add.

**Syntax**: `css_vp_player_addgroup <steamid64> <duration> <group_id>`

**Example**: `css_vp_player_addgroup 76561198012345678 60 vip1`
</details>

##  Timezone
<details>
<summary><strong>Available Timezones</strong></summary>

- You can find the list of available timezones on the wikipedia: [wiki](https://en.wikipedia.org/wiki/List_of_tz_database_time_zones).
- The value in config shall match *TZ identifier*, e.g. **Africa/Abidjan**.
</details>
