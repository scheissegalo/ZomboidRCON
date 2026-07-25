#!/usr/bin/env python3
"""Generate pz_items.json from Project Zomboid game scripts (Build 42 by default)."""

from __future__ import annotations

import argparse
import json
import re
import sys
from collections import defaultdict
from datetime import date
from pathlib import Path

TYPE_CATEGORY_MAP = {
    "Clothing": "Clothing",
    "AlarmClockClothing": "Clothing",
    "Weapon": "Weapons",
    "Food": "Food",
    "Container": "Containers",
    "WeaponPart": "Weapons",
    "Drainable": "Materials",
    "Normal": "Miscellaneous",
    "Map": "Literature",
}

DISPLAY_CATEGORY_MAP = {
    "Clothing": "Clothing",
    "Appearance": "Clothing",
    "Accessory": "Clothing",
    "Bag": "Clothing",
    "MaleBody": "Clothing",
    "ProtectiveGear": "Clothing",
    "Ears": "Clothing",
    "Tail": "Clothing",
    "Weapon": "Weapons",
    "WeaponCrafted": "Weapons",
    "ToolWeapon": "Weapons",
    "Ammo": "Weapons",
    "Explosives": "Weapons",
    "WeaponPart": "Weapons",
    "CookingWeapon": "Weapons",
    "MaterialWeapon": "Weapons",
    "HouseholdWeapon": "Weapons",
    "JunkWeapon": "Weapons",
    "SportsWeapon": "Weapons",
    "GardeningWeapon": "Weapons",
    "InstrumentWeapon": "Weapons",
    "AnimalPartWeapon": "Weapons",
    "FishingWeapon": "Weapons",
    "BrokenWeapon": "Weapons",
    "Food": "Food",
    "Cooking": "Food",
    "Water": "Food",
    "FirstAid": "Medical",
    "Bandage": "Medical",
    "Wound": "Medical",
    "Material": "Materials",
    "Paint": "Materials",
    "VehicleMaintenance": "Materials",
    "RecipeResource": "Materials",
    "AnimalPart": "Materials",
    "Tool": "Tools",
    "Household": "Tools",
    "Gardening": "Tools",
    "Fishing": "Tools",
    "Instrument": "Tools",
    "FireSource": "Tools",
    "Electronics": "Electronics",
    "Communications": "Electronics",
    "Literature": "Literature",
    "SkillBook": "Literature",
    "Entertainment": "Literature",
    "Cartography": "Literature",
    "Memento": "Literature",
    "Camping": "Camping",
    "Trapping": "Trapping",
    "WaterContainer": "Containers",
    "Container": "Containers",
    "LightSource": "Tools",
    "Security": "Miscellaneous",
    "Sports": "Miscellaneous",
    "Furniture": "Miscellaneous",
    "Junk": "Miscellaneous",
    "Corpse": "Miscellaneous",
    "Hidden": "Miscellaneous",
    "ZedDmg": "Miscellaneous",
    "Bug": "Miscellaneous",
    "Teddy": "Miscellaneous",
    "Duck": "Miscellaneous",
}

ANIMAL_CATEGORIES = {
    "Badger", "Beaver", "Bunny", "Fox", "Hedgehog", "Mole", "Raccoon", "Squirrel"
}

ITEM_NAME_RE = re.compile(r'ItemName_([A-Za-z0-9_.]+)\s*=\s*"([^"]+)"')
DISPLAY_CATEGORY_RE = re.compile(r"DisplayCategory\s*=\s*(\w+)")
TYPE_RE = re.compile(r"Type\s*=\s*(\w+)")
DISPLAY_NAME_RE = re.compile(r"DisplayName\s*=\s*(.+?),?\s*$")
MODULE_RE = re.compile(r"^\s*module\s+(\w+)")
ITEM_START_RE = re.compile(r"^\s*item\s+(\w+)")


def humanize(item_id: str) -> str:
    name = item_id.split(".", 1)[-1]
    return re.sub(r"(?<=[a-z])(?=[A-Z])|(?=[A-Z])(?=[A-Z][a-z])", " ", name)


def load_item_names_txt(path: Path) -> dict[str, str]:
    names: dict[str, str] = {}
    if not path.exists():
        return names
    for line in path.read_text(encoding="utf-8", errors="replace").splitlines():
        match = ITEM_NAME_RE.search(line)
        if match:
            key = match.group(1)
            if not key.startswith("Base."):
                key = f"Base.{key}"
            names[key] = match.group(2).strip()
    return names


def load_item_names_json(path: Path) -> dict[str, str]:
    if not path.exists():
        return {}
    data = json.loads(path.read_text(encoding="utf-8"))
    if not isinstance(data, dict):
        return {}
    return {str(key): str(value).strip() for key, value in data.items() if value}


def load_item_names(pz_root: Path) -> dict[str, str]:
    translate_dir = pz_root / "media" / "lua" / "shared" / "Translate" / "EN"
    json_path = translate_dir / "ItemName.json"
    txt_path = translate_dir / "ItemName_EN.txt"

    names = load_item_names_json(json_path)
    if names:
        return names
    return load_item_names_txt(txt_path)


def extract_field(block: str, pattern: re.Pattern[str]) -> str | None:
    match = pattern.search(block)
    return match.group(1) if match else None


def categorize(display_category: str | None, item_type: str | None) -> str:
    if item_type and item_type in TYPE_CATEGORY_MAP:
        category = TYPE_CATEGORY_MAP[item_type]
        if item_type == "Drainable" and display_category:
            mapped = DISPLAY_CATEGORY_MAP.get(display_category)
            if mapped in {"Food", "Medical", "Containers"}:
                return mapped
        if item_type == "Normal" and display_category:
            mapped = DISPLAY_CATEGORY_MAP.get(display_category)
            if mapped:
                return mapped
        return category

    if display_category:
        if display_category in ANIMAL_CATEGORIES:
            return "Miscellaneous"
        return DISPLAY_CATEGORY_MAP.get(display_category, "Miscellaneous")

    return "Miscellaneous"


def parse_script_file(path: Path, module: str, items: dict[str, dict]) -> None:
    text = path.read_text(encoding="utf-8", errors="replace")
    current_module = module

    i = 0
    lines = text.splitlines()
    while i < len(lines):
        line = lines[i]

        module_match = MODULE_RE.match(line)
        if module_match:
            current_module = module_match.group(1)
            i += 1
            continue

        item_match = ITEM_START_RE.match(line)
        if item_match and "{" in line:
            item_name = item_match.group(1)
            block_lines = [line[line.index("{") :]]
            brace_depth = line.count("{") - line.count("}")
            i += 1
            while i < len(lines) and brace_depth > 0:
                block_lines.append(lines[i])
                brace_depth += lines[i].count("{") - lines[i].count("}")
                i += 1
            block = "\n".join(block_lines)
            full_id = f"{current_module}.{item_name}"
            display_category = extract_field(block, DISPLAY_CATEGORY_RE)
            item_type = extract_field(block, TYPE_RE)
            display_name = extract_field(block, DISPLAY_NAME_RE)
            if display_name:
                display_name = display_name.strip().strip('"')

            items[full_id] = {
                "id": full_id,
                "displayCategory": display_category,
                "itemType": item_type,
                "scriptDisplayName": display_name,
                "category": categorize(display_category, item_type),
            }
            continue

        if item_match:
            item_name = item_match.group(1)
            i += 1
            if i >= len(lines) or "{" not in lines[i]:
                continue
            block_lines = [lines[i].strip()]
            brace_depth = lines[i].count("{") - lines[i].count("}")
            i += 1
            while i < len(lines) and brace_depth > 0:
                block_lines.append(lines[i])
                brace_depth += lines[i].count("{") - lines[i].count("}")
                i += 1
            block = "\n".join(block_lines)
            full_id = f"{current_module}.{item_name}"
            display_category = extract_field(block, DISPLAY_CATEGORY_RE)
            item_type = extract_field(block, TYPE_RE)
            display_name = extract_field(block, DISPLAY_NAME_RE)
            if display_name:
                display_name = display_name.strip().strip('"')

            items[full_id] = {
                "id": full_id,
                "displayCategory": display_category,
                "itemType": item_type,
                "scriptDisplayName": display_name,
                "category": categorize(display_category, item_type),
            }
            continue

        i += 1


def choose_name(full_id: str, item: dict, translations: dict[str, str]) -> str:
    if full_id in translations:
        return translations[full_id]
    script_name = item.get("scriptDisplayName")
    if script_name:
        return script_name
    return humanize(full_id)


def generate(pz_root: Path, build: str) -> dict:
    scripts_dir = pz_root / "media" / "scripts"
    if not scripts_dir.exists():
        raise FileNotFoundError(f"Scripts directory not found: {scripts_dir}")

    translations = load_item_names(pz_root)
    raw_items: dict[str, dict] = {}

    for script_file in sorted(scripts_dir.rglob("*.txt")):
        parse_script_file(script_file, "Base", raw_items)

    base_items = {item_id: item for item_id, item in raw_items.items() if item_id.startswith("Base.")}

    output_items = []
    for full_id in sorted(base_items):
        item = base_items[full_id]
        output_items.append(
            {
                "name": choose_name(full_id, item, translations),
                "id": full_id,
                "category": item["category"],
            }
        )

    category_counts = defaultdict(int)
    for item in output_items:
        category_counts[item["category"]] += 1

    return {
        "build": build,
        "generated": date.today().isoformat(),
        "source": str(pz_root),
        "itemCount": len(output_items),
        "categoryCounts": dict(sorted(category_counts.items())),
        "items": output_items,
    }


def validate_presets(items_path: Path, presets_path: Path) -> list[tuple[str, str]]:
    items_data = json.loads(items_path.read_text(encoding="utf-8"))
    item_ids = {item["id"] for item in items_data["items"]}
    presets_data = json.loads(presets_path.read_text(encoding="utf-8"))

    missing: list[tuple[str, str]] = []
    for preset in presets_data.get("presets", []):
        preset_name = preset.get("name", preset.get("id", "unknown"))
        for entry in preset.get("items", []):
            item_id = entry.get("id", "")
            if item_id and item_id not in item_ids:
                missing.append((preset_name, item_id))
    return missing


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "pz_root",
        type=Path,
        help="Path to Project Zomboid install (folder containing media/)",
    )
    parser.add_argument(
        "-o",
        "--output",
        type=Path,
        default=Path("ZomboidRCON/Resources/pz_items.json"),
        help="Output JSON path",
    )
    parser.add_argument(
        "--build",
        default="42",
        help="Build number to write into output metadata (default: 42)",
    )
    parser.add_argument(
        "--validate-presets",
        type=Path,
        default=Path("ZomboidRCON/Resources/default_item_presets.json"),
        help="Validate preset item IDs against generated output (default: bundled presets)",
    )
    parser.add_argument(
        "--skip-preset-validation",
        action="store_true",
        help="Skip preset validation after generation",
    )
    args = parser.parse_args()

    pz_root = args.pz_root
    if (pz_root / "projectzomboid" / "media").exists():
        pz_root = pz_root / "projectzomboid"

    data = generate(pz_root, args.build)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(data, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(f"Wrote {data['itemCount']} items to {args.output}")
    for category, count in sorted(data["categoryCounts"].items()):
        print(f"  {category}: {count}")

    if not args.skip_preset_validation and args.validate_presets.exists():
        missing = validate_presets(args.output, args.validate_presets)
        if missing:
            print("\nPreset validation failed:", file=sys.stderr)
            for preset_name, item_id in missing:
                print(f"  {preset_name}: missing {item_id}", file=sys.stderr)
            sys.exit(1)
        print(f"\nPreset validation passed ({args.validate_presets})")


if __name__ == "__main__":
    main()
