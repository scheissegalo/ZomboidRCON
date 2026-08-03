"""Extract item render metadata from Project Zomboid scripts."""

from __future__ import annotations

import json
import re
from dataclasses import dataclass, field
from pathlib import Path

MODULE_RE = re.compile(r"^\s*module\s+(\w+)")
ITEM_START_RE = re.compile(r"^\s*item\s+(\w+)")
MODEL_BLOCK_RE = re.compile(
    r"model\s+(\w+)\s*\{([^}]+(?:\{[^}]*\}[^}]*)*)\}",
    re.DOTALL,
)
WORLD_STATIC_MODEL_RE = re.compile(r"WorldStaticModel\s*=\s*([^,\n;]+)")
STATIC_MODEL_RE = re.compile(r"StaticModel\s*=\s*([^,\n;]+)")
WEAPON_SPRITE_RE = re.compile(r"WeaponSprite\s*=\s*([^,\n;]+)")
WEAPON_SPRITES_BY_INDEX_RE = re.compile(r"WeaponSpritesByIndex\s*=\s*([^,\n;]+)")
ICON_RE = re.compile(r"Icon\s*=\s*([^,\n;]+)")

# Default scale for meshes resolved via Icon/item-name fallback (not in model registry).
FALLBACK_MESH_SCALE = 0.4


@dataclass
class ModelRegistryEntry:
    mesh: str | None
    texture: str | None
    scale: float


@dataclass
class ItemRenderSpec:
    item_id: str
    model_key: str | None
    mesh_ref: str | None
    texture_ref: str | None
    scale: float
    icon: str | None = None
    issues: list[str] = field(default_factory=list)


def _first_token(value: str) -> str:
    return value.strip().split(";")[0].strip()


def build_model_registry(scripts_dir: Path) -> dict[str, ModelRegistryEntry]:
    registry: dict[str, ModelRegistryEntry] = {}
    for script_file in sorted(scripts_dir.rglob("models_*.txt")):
        text = script_file.read_text(encoding="utf-8", errors="replace")
        for match in MODEL_BLOCK_RE.finditer(text):
            name = match.group(1)
            block = match.group(2)
            mesh_match = re.search(r"mesh\s*=\s*([^,\n]+)", block)
            texture_match = re.search(r"texture\s*=\s*([^,\n]+)", block)
            scale_match = re.search(r"scale\s*=\s*([-+]?\d*\.?\d+(?:[eE][-+]?\d+)?)", block)
            registry[name] = ModelRegistryEntry(
                mesh=mesh_match.group(1).strip() if mesh_match else None,
                texture=texture_match.group(1).strip() if texture_match else None,
                scale=float(scale_match.group(1)) if scale_match else 1.0,
            )
    return registry


def _parse_item_block(lines: list[str], index: int) -> tuple[str, str, int]:
    item_match = ITEM_START_RE.match(lines[index])
    if not item_match:
        raise ValueError(f"Expected item at line {index}")

    item_name = item_match.group(1)
    if "{" in lines[index]:
        block_lines = [lines[index][lines[index].index("{") :]]
        brace_depth = lines[index].count("{") - lines[index].count("}")
        index += 1
    else:
        index += 1
        if index >= len(lines) or "{" not in lines[index]:
            return item_name, "", index
        block_lines = [lines[index].strip()]
        brace_depth = lines[index].count("{") - lines[index].count("}")
        index += 1

    while index < len(lines) and brace_depth > 0:
        block_lines.append(lines[index])
        brace_depth += lines[index].count("{") - lines[index].count("}")
        index += 1

    return item_name, "\n".join(block_lines), index


def _model_key_from_block(block: str) -> str | None:
    for pattern in (WORLD_STATIC_MODEL_RE, STATIC_MODEL_RE, WEAPON_SPRITE_RE, WEAPON_SPRITES_BY_INDEX_RE):
        match = pattern.search(block)
        if match:
            return _first_token(match.group(1))
    return None


def _icon_from_block(block: str) -> str | None:
    match = ICON_RE.search(block)
    return _first_token(match.group(1)) if match else None


def parse_item_script_fields(pz_root: Path, item_ids: set[str]) -> dict[str, dict[str, str | None]]:
    scripts_dir = pz_root / "media" / "scripts"
    fields_by_id: dict[str, dict[str, str | None]] = {}

    for script_file in sorted(scripts_dir.rglob("*.txt")):
        if script_file.name.startswith("models_"):
            continue

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

            item_match = ITEM_START_RE.match(line)
            if not item_match:
                index += 1
                continue

            item_name, block, index = _parse_item_block(lines, index)
            full_id = f"{current_module}.{item_name}"
            if full_id not in item_ids:
                continue

            fields_by_id[full_id] = {
                "model_key": _model_key_from_block(block),
                "icon": _icon_from_block(block),
                "item_name": item_name,
            }

    return fields_by_id


def parse_item_model_keys(pz_root: Path, item_ids: set[str]) -> dict[str, str]:
    return {
        item_id: fields["model_key"]
        for item_id, fields in parse_item_script_fields(pz_root, item_ids).items()
        if fields.get("model_key")
    }


def load_catalog_item_ids(catalog_path: Path) -> list[str]:
    data = json.loads(catalog_path.read_text(encoding="utf-8"))
    item_ids: list[str] = []
    for item in data.get("items", []):
        item_id = item.get("id")
        if item_id:
            item_ids.append(item_id)
    return item_ids


def collect_item_render_specs(pz_root: Path, catalog_path: Path) -> list[ItemRenderSpec]:
    from pz_preview.item_mesh_resolver import ItemMeshResolver

    scripts_dir = pz_root / "media" / "scripts"
    if not scripts_dir.exists():
        raise FileNotFoundError(f"Scripts directory not found: {scripts_dir}")

    item_ids = load_catalog_item_ids(catalog_path)
    item_id_set = set(item_ids)
    registry = build_model_registry(scripts_dir)
    script_fields = parse_item_script_fields(pz_root, item_id_set)
    mesh_resolver = ItemMeshResolver(pz_root)

    specs: list[ItemRenderSpec] = []
    for item_id in item_ids:
        issues: list[str] = []
        fields = script_fields.get(item_id, {})
        model_key = fields.get("model_key")
        icon = fields.get("icon")
        item_name = fields.get("item_name")
        if isinstance(item_name, str):
            short_name = item_name
        else:
            short_name = item_id.split(".", 1)[-1]

        if not model_key:
            fallback = mesh_resolver.resolve_icon_fallback(
                icon if isinstance(icon, str) else None,
                short_name,
            )
            if fallback:
                texture_ref = f"WorldItems/{Path(fallback.mesh_ref).name}"
                specs.append(
                    ItemRenderSpec(
                        item_id=item_id,
                        model_key=None,
                        mesh_ref=fallback.mesh_ref,
                        texture_ref=texture_ref,
                        scale=FALLBACK_MESH_SCALE,
                        icon=icon if isinstance(icon, str) else None,
                        issues=[f"icon fallback via {fallback.mesh_ref}"],
                    )
                )
                continue

            issues.append("no model field")
            specs.append(
                ItemRenderSpec(
                    item_id=item_id,
                    model_key=None,
                    mesh_ref=None,
                    texture_ref=None,
                    scale=1.0,
                    icon=icon if isinstance(icon, str) else None,
                    issues=issues,
                )
            )
            continue

        entry = registry.get(model_key)
        if not entry:
            issues.append(f"model {model_key!r} not in registry")
            specs.append(
                ItemRenderSpec(
                    item_id=item_id,
                    model_key=model_key,
                    mesh_ref=None,
                    texture_ref=None,
                    scale=1.0,
                    icon=icon if isinstance(icon, str) else None,
                    issues=issues,
                )
            )
            continue

        if not entry.mesh:
            issues.append(f"model {model_key!r} has no mesh")

        specs.append(
            ItemRenderSpec(
                item_id=item_id,
                model_key=model_key,
                mesh_ref=entry.mesh,
                texture_ref=entry.texture,
                scale=entry.scale,
                icon=icon if isinstance(icon, str) else None,
                issues=issues,
            )
        )

    return specs


def specs_by_item_id(specs: list[ItemRenderSpec]) -> dict[str, ItemRenderSpec]:
    return {spec.item_id: spec for spec in specs}
