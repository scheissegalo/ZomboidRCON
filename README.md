# ZomboidRCON

A cross-platform GUI tool to administrate Project Zomboid **Build 42** servers via RCON.

Runs on **Windows** and **Linux** — no .NET runtime installation required (self-contained builds).

## Features

- **Player Management** — view online players, kick, set access levels (Admin/GM/Moderator/Overseer/None), enable/disable godmode, invisible, and noclip, set player password, add to whitelist
- **Vehicle Spawning** — spawn vehicles from a Build 42 catalog (~157 variants in 20 model groups); preview images where available
- **Item Spawning** — searchable categorized item picker with profession/survival presets and custom preset editor (~5,100 `Base.*` items)
- **Teleportation** — teleport players to other players or to coordinates
- **Experience Management** — add experience to player skills (including B42 crafting perks)
- **Command Console** — send arbitrary RCON commands to the server (type `help` to list server commands)
- **Weather Controls** — start/stop rain and storms, stop all weather from the Server Controls menu
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

![Main window](https://i.ibb.co/99z2Kz3L/Screenshot-20260803-174746.png "Main window")

#### Spawn Items:

![Spawn Items](https://i.ibb.co/nsBymM0c/Screenshot-20260803-174832.png "Spawn Items")

#### Vehicle spawning:

![Spawn Vehicle](https://i.ibb.co/9H22Bt1p/Screenshot-20260803-174906.png "Vehicle spawning")

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

## Updating bundled game data

The app embeds Build 42 item and vehicle catalogs. Regenerate them from a local Project Zomboid install after a game update:

```bash
# Items (~5,100 Base.* entries)
python3 tools/generate_items.py "/path/to/projectzomboid"

# Vehicles (~157 spawnable variants)
python3 tools/generate_vehicles.py "/path/to/projectzomboid"
```

On Linux Steam installs, game files are usually under `.../steamapps/common/ProjectZomboid/projectzomboid/`.

- **Items** — reads `media/scripts/**/*.txt` and English names from `media/lua/shared/Translate/EN/ItemName.json`
- **Vehicles** — reads vehicle scripts and display names from `IG_UI.json`; excludes burnt/smashed wrecks

Rebuild the app after regenerating so the new data is embedded in the executable. The item generator validates bundled presets automatically; if a preset references a missing item ID, generation fails with a list of broken references.

## Item presets

Give Item supports **presets** — bundled item kits for professions and survival, plus custom groups you create.

- **24 built-in presets**: one kit per standard profession (Carpenter, Mechanic, Doctor, etc.) plus Basic Survival, Extended Survival, and Medical Kit — updated for Build 42 item IDs
- **Give Preset** in the Give Item window delivers all items in one action
- **Manage Presets** opens the editor to create, duplicate, edit, or delete custom presets
- **Server Controls → Item Presets…** opens the editor without selecting a player first

Custom presets and edits to built-in kits are saved to:

- Linux: `~/.config/ZomboidRCON/item_presets.json`
- Windows: `%AppData%/ZomboidRCON/item_presets.json`

Built-in presets can be reset to defaults from the editor. Use **Reset to Default** on a selected built-in preset.

If you upgraded from an older version, custom presets may still reference removed Build 41 item IDs — edit or reset those presets in the editor.

## Vehicle previews

Preview images are bundled for common vanilla vehicles (about 40 variants). Spawning works for all catalog entries even when no preview PNG exists — the spawn dialog simply leaves the image area empty.

Regenerate preview PNGs from a local Project Zomboid install:

```bash
python3 -m venv .venv-preview
source .venv-preview/bin/activate
pip install -r tools/requirements-preview.txt

python3 tools/generate_vehicle_previews.py "/path/to/projectzomboid" \
  --catalog ZomboidRCON/Resources/default_vehicles.json \
  --output ZomboidRCON/Assets/Vehicles \
  --size 512x320 \
  --blender /usr/bin/blender
```

- Reads spawnable variants from `default_vehicles.json` and writes `{variantId}.png` files (for example `Base.CarNormal.png`)
- Uses PZ mesh `.txt` files plus vehicle shell textures for most variants (via `trimesh` + `pyglet`)
- Uses **assimp-utils** (`assimp export`) or headless **Blender** for the 5 trailer FBX meshes; other FBX-only variants use the closest chassis `.txt` mesh with the correct livery texture
- Install `assimp-utils` (Debian/Ubuntu) for trailer previews when Blender FBX import is unavailable
- Useful flags: `--dry-run`, `--only Base.CarNormal,Base.Van`, `--force`, `--skip-existing`
- On headless Linux, if rendering fails to open a window, run under `xvfb-run`

Rebuild the app after generating previews so new PNGs are embedded in the executable.

## Item previews

Preview JPEGs are bundled for catalog items that have resolvable 3D meshes (~4,200 items). The Give Item dialog shows a preview when a JPEG exists; items without a 3D model leave the image area empty.

Regenerate preview JPEGs from a local Project Zomboid install:

```bash
python3 -m venv .venv-preview
source .venv-preview/bin/activate
pip install -r tools/requirements-preview.txt

python3 tools/generate_item_previews.py "/path/to/projectzomboid" \
  --catalog ZomboidRCON/Resources/pz_items.json \
  --output ZomboidRCON/Assets/Items \
  --size 256x256 \
  --format jpeg --quality 85
```

- Reads item IDs from `pz_items.json` and writes `{itemId}.jpg` files (for example `Base.Apple.jpg`)
- Resolves meshes from `media/models_X/` (WorldItems, weapons, held `.X` models) via **assimp-utils**
- Looks up textures from item model registry entries and weapon/WorldItems texture folders
- Items with a mesh but no texture render as neutral gray (common for shared clothing ground piles)
- Useful flags: `--dry-run`, `--only Base.Apple,Base.Pistol`, `--limit 50`, `--force`, `--skip-existing`
- On headless Linux, if rendering fails to open a window, run under `xvfb-run`

Commit generated JPEGs under `ZomboidRCON/Assets/Items/` before tagging a release so CI embeds them in published builds.

Rebuild the app after generating previews so new JPEGs are embedded in the executable.

## Testing against a Build 42 server

After connecting to a B42 RCON server, verify:

1. Player list refresh (`players` command)
2. Give Item — search for a B42 item (e.g. stone tool, animal product) and spawn it
3. Give Preset — deliver a profession or survival kit
4. Spawn Vehicle — try a classic model (e.g. Chevalier Nyala) and a branded livery variant
5. Teleport, godmode, invisible, noclip, access level changes, add XP on a B42 perk
6. Server Controls — start/stop rain and weather
7. Command Console — run `help` and an arbitrary admin command

RCON command syntax for player-targeting admin commands uses quoted usernames (e.g. `godmodeplayer "name" -true`, `invisibleplayer "name" -true`, `noclip "name" -true`, `setpassword "name" "password"`). Other commands this tool uses include `additem`, `addvehicle`, `teleport`, `teleportto`, `setaccesslevel`, `addxp`, `startrain`, `startstorm`, `stoprain`, and `stopweather`. Use the in-app Command Console `help` output on your server if a patch changes command names.

## Tech Stack

- [.NET 8](https://dotnet.microsoft.com/) — target framework
- [Avalonia UI 11](https://avaloniaui.net/) — cross-platform UI framework (replaces WinForms)
- [RconSharp](https://github.com/nickvdyck/rconsharp) — RCON client library
- [LiteDB](https://www.litedb.org/) — embedded database for player history
- [GitHub Actions](https://docs.github.com/en/actions) — automated CI/CD builds and releases

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to test against a Build 42 server before submitting.

## License

[MIT](https://choosealicense.com/licenses/mit/)
