#!/usr/bin/env python3
"""Generate pz_items.json from Project Zomboid Build 41 game scripts."""

from __future__ import annotations

import argparse
import json
import re
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
    "Weapon": "Weapons",
    "WeaponCrafted": "Weapons",
    "ToolWeapon": "Weapons",
    "Ammo": "Weapons",
    "Explosives": "Weapons",
    "WeaponPart": "Weapons",
    "Food": "Food",
    "Cooking": "Food",
    "Water": "Food",
    "FirstAid": "Medical",
    "Bandage": "Medical",
    "Wound": "Medical",
    "Material": "Materials",
    "Paint": "Materials",
    "VehicleMaintenance": "Materials",
    "Tool": "Tools",
    "Household": "Tools",
    "Gardening": "Tools",
    "Fishing": "Tools",
    "Instrument": "Tools",
    "Electronics": "Electronics",
    "Communications": "Electronics",
    "Literature": "Literature",
    "SkillBook": "Literature",
    "Entertainment": "Literature",
    "Cartography": "Literature",
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
    return re.sub(r"(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])", " ", name)


def load_item_names(path: Path) -> dict[str, str]:
    names: dict[str, str] = {}
    if not path.exists():
        return names
    for line in path.read_text(encoding="utf-8", errors="replace").splitlines():
        match = ITEM_NAME_RE.search(line)
        if match:
            names[match.group(1)] = match.group(2).strip()
    return names


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


def generate(pz_root: Path) -> dict:
    scripts_dir = pz_root / "media" / "scripts"
    item_name_path = pz_root / "media" / "lua" / "shared" / "Translate" / "EN" / "ItemName_EN.txt"
    if not scripts_dir.exists():
        raise FileNotFoundError(f"Scripts directory not found: {scripts_dir}")

    translations = load_item_names(item_name_path)
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
        "build": "41",
        "generated": date.today().isoformat(),
        "source": str(pz_root),
        "itemCount": len(output_items),
        "categoryCounts": dict(sorted(category_counts.items())),
        "items": output_items,
    }


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
    args = parser.parse_args()

    pz_root = args.pz_root
    if (pz_root / "projectzomboid" / "media").exists():
        pz_root = pz_root / "projectzomboid"

    data = generate(pz_root)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(data, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(f"Wrote {data['itemCount']} items to {args.output}")
    for category, count in sorted(data["categoryCounts"].items()):
        print(f"  {category}: {count}")


if __name__ == "__main__":
    main()
