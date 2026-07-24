# ZomboidRCON

A cross-platform GUI tool to administrate Project Zomboid servers via RCON.

Runs on **Windows** and **Linux** — no .NET runtime installation required (self-contained builds).

## Features

- **Player Management** — view online players, kick, set access levels (Admin/GM/Moderator/Overseer/None), enable/disable godmode, add to whitelist
- **Vehicle Spawning** — spawn vehicles with preview images for each variant
- **Item Spawning** — searchable categorized item picker with profession/survival presets and custom preset editor
- **Teleportation** — teleport players to other players or to coordinates
- **Experience Management** — add experience to player skills
- **Command Console** — send arbitrary RCON commands to the server
- **Auto-Updater** — checks GitHub releases for updates and downloads the correct platform binary automatically
- **Persistent Settings** — connection details are saved and reloaded on next launch

## Installation

1. Go to [Releases](https://github.com/scheissegalo/ZomboidRCON/releases/latest)
2. Download the zip for your platform:
   - **Windows**: `ZomboidRCON-win-x64.zip`
   - **Linux**: `ZomboidRCON-linux-x64.zip`
3. Extract the zip
4. Run the executable:
   - Windows: `ZomboidRCON.exe`
   - Linux: `./ZomboidRCON`

No .NET runtime or additional dependencies needed — everything is bundled.

## Usage

1. Launch the application
2. Enter your server IP, RCON port, and password
3. Check **Save Details** to remember credentials for next time
4. Click **Connect**
5. Right-click on a player in the list to access admin actions

### Screenshots

#### Main window:

![Main window](https://i.ibb.co/hsPj4hj/s1.png "Main window")

#### Vehicle spawning:

![Spawn Vehicle](https://i.ibb.co/kGR2WLW/s2.png "Vehicle spawning")

## Building from Source

Requires [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

```bash
# Build
dotnet build ZomboidRCON/ZomboidRCON.csproj

# Run
dotnet run --project ZomboidRCON/ZomboidRCON.csproj

# Publish (self-contained, no runtime needed)
dotnet publish ZomboidRCON/ZomboidRCON.csproj -r linux-x64 --self-contained -c Release
dotnet publish ZomboidRCON/ZomboidRCON.csproj -r win-x64 --self-contained -c Release
```

## Updating the item list

The Give Item dialog loads a bundled Build 41 item database from `ZomboidRCON/Resources/pz_items.json` (~2200 `Base.*` items). To regenerate it after a game update, run the maintainer script against your local Project Zomboid install:

```bash
python3 tools/generate_items.py "/path/to/ProjectZomboid"
```

On Linux Steam installs, the game files are usually under `.../steamapps/common/ProjectZomboid/projectzomboid/`. The script reads `media/scripts/**/*.txt` and English display names from `media/lua/shared/Translate/EN/ItemName_EN.txt`, then writes categorized JSON to `ZomboidRCON/Resources/pz_items.json`.

Rebuild the app after regenerating the file so the new list is embedded in the executable.

## Item presets

Give Item supports **presets** — bundled item kits for professions and survival, plus custom groups you create.

- **24 built-in presets**: one kit per standard Build 41 profession (Carpenter, Mechanic, Doctor, etc.) plus Basic Survival, Extended Survival, and Medical Kit
- **Give Preset** in the Give Item window delivers all items in one action
- **Manage Presets** opens the editor to create, duplicate, edit, or delete custom presets
- **Server Controls → Item Presets…** opens the editor without selecting a player first

Custom presets and edits to built-in kits are saved to:

- Linux: `~/.config/ZomboidRCON/item_presets.json`
- Windows: `%AppData%/ZomboidRCON/item_presets.json`

Built-in presets can be reset to defaults from the editor. Use **Reset to Default** on a selected built-in preset.

## Tech Stack

- [.NET 8](https://dotnet.microsoft.com/) — target framework
- [Avalonia UI 11](https://avaloniaui.net/) — cross-platform UI framework (replaces WinForms)
- [RconSharp](https://github.com/nickvdyck/rconsharp) — RCON client library
- [LiteDB](https://www.litedb.org/) — embedded database for player/vehicle data
- [GitHub Actions](https://docs.github.com/en/actions) — automated CI/CD builds and releases

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to test before submitting.

## License

[MIT](https://choosealicense.com/licenses/mit/)
