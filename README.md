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
    "Prefix": "{lightred}VIP "
  },
  "VIPGroups": [
    {
      "Permissions": "@vip-plugin/vip",
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
          "ExtraMoney": 2000,
          "ExtraMoneyOnPistolRound": false,
          "Grenades": {
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
          "Amount": 10,
          "VelocityZ": 260,
          "NoFallDamage": false
        },
        "NoFallDamageGlobal": false
      },
      "Messages": {
        "ConnectChat": "VIP {playername} joined the server",
        "DisconnectChat": "VIP {playername} left the server"
      }
    }
  ],
  "RandomVIP": {
    "Enabled ": false,
    "AfterRound ": 3,
    "MinimumPlayers ": 2,
    "RepeatPicking ": 3,
    "PermissionsGranted ": [],
    "PermissionExclude": []
  },
  "NightVIP": {
    "Enabled": false,
    "StartHour": 22,
    "EndHour": 8,
    "RequiredNickPhrase": "Phrase",
    "RequiredScoreboardTag": "Phrase",
    "PermissionsGranted ": [],
    "PermissionExclude": []
  },
  "ConfigVersion": 1
}
```
