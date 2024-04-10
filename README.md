# VIP-Plugin

## Description
VIP-Plugin is a simple plugin designed for CS2 server owners, enabling them to establish VIP groups through the CSS Permission system for authentication.

## Dependencies
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp/releases) v201
- [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master)

## Instalation
1. Download the [latest release](https://github.com/CS-GEJMERZY/VIP-Plugin/releases/latest)
2. Unzip the package and upload files to **_csgo/addons/counterstrikesharp/plugins_**

## Configuration 
Upon the first launch, the **_VIP-Plugin.json_**  file will be automatically created in **_csgo/addons/counterstrikesharp/configs/plugins/VIP-Plugin_**
```
{
  "Settings": {
    "Prefix": "{lightred}VIP " // // Text displayed before plugin's chat messages
  },
  "VIPGroups": [
    {
      "Permissions": "@vip-plugin/vip", // CSS permission required for this VIP group
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
        "Smoke": {
          "Enabled": false, // Enable colored smoke
          "Type": 0, // 0 = Fixed color, 1 = Random color
          "Color": "#FF0000" // Smoke color in hex code
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
    "StartHour": 22, // The hours can be 8-22 or 22-8(which mean from 22 to 24 and 00 to 08)
    "EndHour": 8,
    "RequiredNickPhrase": "YourSite.com", // Required nickname phrase(empty = pass)
    "RequiredScoreboardTag": "YourSite.com", // Required scoreboard tag(empty = pass)
    "PermissionsGranted ": [], // List of CSS permissions
    "PermissionExclude": [] // Exclude players with these permissions
  },
  "ConfigVersion": 1
}
```
