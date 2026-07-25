#!/usr/bin/env python3
"""Generate default_vehicles.json from Project Zomboid vehicle scripts (Build 42)."""

from __future__ import annotations

import argparse
import json
import re
from collections import defaultdict
from datetime import date
from pathlib import Path

MODULE_RE = re.compile(r"^\s*module\s+(\w+)")
VEHICLE_START_RE = re.compile(r"^\s*vehicle\s+(\w+)")
TEMPLATE_RE = re.compile(r"template!\s*=\s*(\w+)")
EXCLUDED_NAME_PARTS = ("Burnt", "Smashed", "Template")

GROUP_DISPLAY_SUFFIXES = {
    "van_seats": "6-Seater",
    "trailer_advert": "Advert",
    "van_ambulance": "Ambulance",
    "van_radio": "Radio",
}

CHASSIS_RULES: list[tuple[str, str]] = [
    ("CarLightsPolice", "car_normal"),
    ("CarLights", "car_normal"),
    ("CarTaxi2", "car_normal"),
    ("CarTaxi", "car_normal"),
    ("CarNormal", "car_normal"),
    ("CarStationWagon2", "car_stationwagon"),
    ("CarStationWagon", "car_stationwagon"),
    ("CarLuxury", "car_luxury"),
    ("ModernCar02", "modern_car02"),
    ("ModernCar", "modern_car"),
    ("SmallCar02", "small_car02"),
    ("SmallCar", "small_car"),
    ("SportsCar", "sports_car"),
    ("PickUpVanLights", "pickupvan"),
    ("PickUpVan", "pickupvan"),
    ("PickUpTruckLights", "pickuptruck"),
    ("PickUpTruck", "pickuptruck"),
    ("StepVan", "stepvan"),
    ("VanSeats", "van_seats"),
    ("VanAmbulance", "van_ambulance"),
    ("VanRadio_3N", "van_radio"),
    ("VanRadio", "van_radio"),
    ("Van", "van"),
    ("OffRoad", "offroad"),
    ("SUV", "suv"),
    ("TrailerCover", "trailer"),
    ("TrailerAdvert", "trailer_advert"),
    ("Trailer", "trailer"),
    ("RaceCar", "race_car"),
]


def humanize(vehicle_id: str) -> str:
    return re.sub(r"(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])", " ", vehicle_id)


def load_vehicle_names(pz_root: Path) -> dict[str, str]:
    path = pz_root / "media" / "lua" / "shared" / "Translate" / "EN" / "IG_UI.json"
    if not path.exists():
        return {}
    data = json.loads(path.read_text(encoding="utf-8"))
    names: dict[str, str] = {}
    for key, value in data.items():
        if key.startswith("IGUI_VehicleName"):
            names[key.removeprefix("IGUI_VehicleName")] = str(value).strip()
    return names


def should_exclude(vehicle_id: str) -> bool:
    return any(part in vehicle_id for part in EXCLUDED_NAME_PARTS)


def chassis_group_key(vehicle_id: str) -> str:
    for prefix, group in CHASSIS_RULES:
        if vehicle_id == prefix or vehicle_id.startswith(prefix):
            return group
    return vehicle_id.lower()


def infer_group_key(vehicle_id: str, template: str | None, script_path: Path) -> str:
    return chassis_group_key(vehicle_id)


def parse_vehicle_block(lines: list[str], start_index: int) -> tuple[str, str, int]:
    first_line = lines[start_index]
    match = VEHICLE_START_RE.match(first_line)
    if not match:
        raise ValueError("Not a vehicle block")

    vehicle_id = match.group(1)
    block_lines = [first_line[first_line.index("{") :] if "{" in first_line else lines[start_index + 1]]
    brace_depth = first_line.count("{") - first_line.count("}")
    index = start_index + 1
    if "{" not in first_line:
        block_lines = [lines[index].strip()]
        brace_depth = lines[index].count("{") - lines[index].count("}")
        index += 1

    while index < len(lines) and brace_depth > 0:
        block_lines.append(lines[index])
        brace_depth += lines[index].count("{") - lines[index].count("}")
        index += 1

    return vehicle_id, "\n".join(block_lines), index


def collect_vehicles(pz_root: Path) -> list[dict]:
    scripts_dir = pz_root / "media" / "scripts"
    if not scripts_dir.exists():
        raise FileNotFoundError(f"Scripts directory not found: {scripts_dir}")

    vehicles: list[dict] = []
    for script_file in sorted(scripts_dir.rglob("*.txt")):
        text = script_file.read_text(encoding="utf-8", errors="replace")
        lines = text.splitlines()
        current_module = "Base"
        index = 0
        while index < len(lines):
            line = lines[index]
            module_match = MODULE_RE.match(line)
            if module_match:
                current_module = module_match.group(1)
                index += 1
                continue

            vehicle_match = VEHICLE_START_RE.match(line)
            if not vehicle_match:
                index += 1
                continue

            vehicle_id, block, index = parse_vehicle_block(lines, index)
            if current_module != "Base" or should_exclude(vehicle_id):
                continue

            template_match = TEMPLATE_RE.search(block)
            vehicles.append(
                {
                    "id": vehicle_id,
                    "variantId": f"Base.{vehicle_id}",
                    "template": template_match.group(1) if template_match else None,
                    "groupKey": infer_group_key(
                        vehicle_id,
                        template_match.group(1) if template_match else None,
                        script_file,
                    ),
                    "script": str(script_file.relative_to(scripts_dir)),
                }
            )

    return vehicles


def choose_group_name(group_entries: list[dict], translations: dict[str, str]) -> str:
    preferred_ids = {
        "car_normal": "CarNormal",
        "car_stationwagon": "CarStationWagon",
        "car_luxury": "CarLuxury",
        "modern_car": "ModernCar",
        "modern_car02": "ModernCar02",
        "small_car": "SmallCar",
        "small_car02": "SmallCar02",
        "sports_car": "SportsCar",
        "pickupvan": "PickUpVan",
        "pickuptruck": "PickUpTruck",
        "stepvan": "StepVan",
        "van": "Van",
        "van_seats": "VanSeats",
        "van_ambulance": "VanAmbulance",
        "van_radio": "VanRadio",
        "offroad": "OffRoad",
        "suv": "SUV",
        "trailer": "Trailer",
        "trailer_advert": "TrailerAdvert",
        "race_car": "RaceCar12",
    }

    group_key = group_entries[0]["groupKey"]
    preferred_vehicle_id = preferred_ids.get(group_key)
    if preferred_vehicle_id:
        preferred_name = translations.get(preferred_vehicle_id)
        if preferred_name:
            suffix = GROUP_DISPLAY_SUFFIXES.get(group_key)
            if suffix and suffix not in preferred_name:
                return f"{preferred_name} {suffix}"
            return preferred_name

    for entry in group_entries:
        translation = translations.get(entry["id"])
        if translation:
            return translation

    primary = group_entries[0]
    return humanize(primary["groupKey"])


def choose_variant_title(vehicle_id: str, group_name: str, translation: str | None) -> str:
    if translation:
        if translation != group_name:
            return translation
        if len(translation.split()) > 3:
            return translation
    return humanize(vehicle_id)


def build_catalog(vehicles: list[dict], translations: dict[str, str]) -> tuple[list[dict], list[str]]:
    grouped: dict[str, list[dict]] = defaultdict(list)
    for vehicle in vehicles:
        grouped[vehicle["groupKey"]].append(vehicle)

    warnings: list[str] = []
    output_groups: list[dict] = []

    for group_key in sorted(grouped):
        entries = sorted(grouped[group_key], key=lambda item: item["id"])
        if len(entries) > 30:
            warnings.append(f"Large group '{group_key}' has {len(entries)} variants; keeping together")

        group_name = choose_group_name(entries, translations)
        variants = []
        for entry in entries:
            translation = translations.get(entry["id"])
            title = choose_variant_title(entry["id"], group_name, translation)
            variants.append(
                {
                    "title": title,
                    "variantId": entry["variantId"],
                }
            )

        if len(variants) == 1 and variants[0]["title"] == group_name:
            variants[0]["title"] = "Normal"

        output_groups.append(
            {
                "name": group_name,
                "variants": variants,
            }
        )

    output_groups.sort(key=lambda group: group["name"].lower())
    return output_groups, warnings


def generate(pz_root: Path, build: str) -> dict:
    raw_vehicles = collect_vehicles(pz_root)
    translations = load_vehicle_names(pz_root)
    vehicles, warnings = build_catalog(raw_vehicles, translations)

    variant_count = sum(len(group["variants"]) for group in vehicles)
    return {
        "build": build,
        "generated": date.today().isoformat(),
        "source": str(pz_root),
        "vehicleGroupCount": len(vehicles),
        "vehicleCount": variant_count,
        "warnings": warnings,
        "vehicles": vehicles,
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
        default=Path("ZomboidRCON/Resources/default_vehicles.json"),
        help="Output JSON path",
    )
    parser.add_argument(
        "--build",
        default="42",
        help="Build number to write into output metadata (default: 42)",
    )
    args = parser.parse_args()

    pz_root = args.pz_root
    if (pz_root / "projectzomboid" / "media").exists():
        pz_root = pz_root / "projectzomboid"

    data = generate(pz_root, args.build)
    args.output.parent.mkdir(parents=True, exist_ok=True)

    output = {key: value for key, value in data.items() if key != "warnings"}
    args.output.write_text(json.dumps(output, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")

    print(f"Wrote {data['vehicleCount']} variants in {data['vehicleGroupCount']} groups to {args.output}")
    for warning in data["warnings"]:
        print(f"  warning: {warning}")


if __name__ == "__main__":
    main()
