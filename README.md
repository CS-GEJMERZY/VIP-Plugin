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
// This configuration was automatically generated by CounterStrikeSharp for plugin 'VIP-Plugin', at 2024.03.29 04:29:15
{
  "Settings": {
    "Prefix": "{lightred}VIP " // Plugin prefix, ATM only shown in RandomVIP messages
  },
  "VIPGroups": [
    {
      "Permissions": "@vip-plugin/vip", // CSS permission
      "Name": "VIP",
      "Events": {
        "Spawn": {
          "HP": 105,
          "ArmorValue": 100,
          "Helmet": true,
          "HelmetOnPistolRound": false,
          "DefuseKit": true,
          "Zeus": true,
          "ZeusOnPistolRound": true,
          "HealthshotAmount": 1,
          "HealthshotOnPistolRound": true,
          "ExtraMoney": 2000,
          "ExtraMoneyOnPistolRound": false,
          "Grenades": { // Enter the amount for each grenade. 0 = don't give
            "Smoke": 1,
            "HE": 1,
            "Flashbang": 1,
            "FireGrenade": 1,
            "Decoy": 0
          }
        },
        "Kill": {
          "HP": 2,
          "HeadshotHP": 3,
          "Money": 200,
          "HeadshotMoney": 300
        },
        "Bomb": {
          "PlantMoney": 500,
          "DefuseMoney": 500
        },
        "Round": {
          "WinMoney": 1000,
          "LoseMoney": 0
        }
      },
      "Limits": {
        "MaxHP": 120,
        "MaxMoney": 16000
      },
      "Misc": {
        "ExtraJumps": {
          "Amount": 0, // Amount = 1 means double jump
          "VelocityZ": 260, // Basically the height of a jump
          "NoFallDamage": true // Whether NoFallDamage after extra jump is enabled
        },
        "Smoke": {
          "Enabled": false, // Whether it is enabled
          "Type": 0, // Type: 0 - fixed color from 'Color', 1 - random
          "Color": "#FF0000" // Color in hex
        },
        "NoFallDamageGlobal": false, // Global 
        "Gravity": 1,
        "Speed": 1
      },
      "Messages": {
        "Chat": {
          "Connect": {
            "Enabled": true,
            "Message": "VIP {playername} joined the server",
            "DontBroadcast": true // Whether native message will be show
          },
          "Disconnect": {
            "Enabled": true,
            "Message": "VIP {playername} left the server",
            "DontBroadcast": true // Whether native message will be show
          }
        }
      }
    }
  ],
  "RandomVIP": {
    "Enabled ": false,
    "AfterRound ": 3,
    "MinimumPlayers ": 2,
    "RepeatPickingMessage": 3,
    "PermissionsGranted ": [], // List of CSS permissions
    "PermissionExclude": [] // List of CSS permissions
  },
  "NightVIP": {
    "Enabled": false,
    "StartHour": 22,
    "EndHour": 8,
    "RequiredNickPhrase": "YourSite.com",
    "RequiredScoreboardTag": "YourSite.com",
    "PermissionsGranted ": [], // List of CSS permissions
    "PermissionExclude": [] // List of CSS permissions
  },
  "ConfigVersion": 1
}
```
