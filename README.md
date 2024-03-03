# VIP-Plugin

## Description
VIP-Plugin is a simple plugin designed for CS2 server owners, enabling them to establish VIP groups through the CSS Permission system for authentication.

## Dependencies
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp/releases)
- [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master)

## Instalation
1. Download the [latest release](https://github.com/CS-GEJMERZY/VIP-Plugin/releases/latest)
2. Unzip the package and upload files to **_csgo/addons/counterstrikesharp/plugins_**

## Configuration
Upon the first launch, the **_VIP-Plugin.json_**  file will be automatically created in **_csgo/addons/counterstrikesharp/configs/plugins/VIP-Plugin_**
```
{
  "Settings": {
    "Prefix": "[VIP-Plugin] "
  },
  "Groups": [
    {
      "Permissions": "@vip-plugin/vip",
      "Name": "VIP",
      "ExtraJumps": 1,
      "NoFallDamage": true,
      "SpawnHP": 105,
      "SpawnArmor": 100,
      "SpawnHelmet": true,
      "SpawnDefuser": true,
      "SpawnMoney": 2000,
      "RoundWonMoney": 1000,
      "KillMoney": 200,
      "HeadshotKillMoney": 300,
      "BombPlantMoney": 500,
      "BombDefuseMoney": 500,
      "KillHP": 2,
      "HeadshotKillHP": 5,
      "ConnectMessage": "VIP {playername} joined the server",
      "DisconnectMessage": "VIP {playername} left the server",
      "SmokeGrenade": 1,
      "HEGrenade": 1,
      "FlashGrenade": 1,
      "Molotov": 1,
      "DecoyGrenade": 0,
      "Zeus": true
    }
  ],
  "RandomVIP": {
    "enabled": false,
    "afterRound": 3,
    "repeatPicking": 3,
    "permissions": []
  },
  "ConfigVersion": 1
}
```
