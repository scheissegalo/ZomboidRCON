"""Extract vehicle render metadata from Project Zomboid scripts."""

from __future__ import annotations

import re
from dataclasses import dataclass
from pathlib import Path

from generate_vehicles import EXCLUDED_NAME_PARTS, MODULE_RE, VEHICLE_START_RE, parse_vehicle_block, should_exclude

MODEL_BLOCK_RE = re.compile(
    r"model\s+(\w+)\s*\{[^}]*?mesh\s*=\s*([^,\n|]+)",
    re.DOTALL,
)
TEMPLATE_VEHICLE_RE = re.compile(r"^\s*template\s+vehicle\s+(\w+)")
SKIN_TEXTURE_RE = re.compile(
    r"skin\s*\{[^}]*?texture\s*=\s*([^,\n]+)",
    re.DOTALL,
)
MODEL_FILE_RE = re.compile(
    r"model\s*\{[^}]*?file\s*=\s*([^,\n]+)",
    re.DOTALL,
)
MODEL_SCALE_RE = re.compile(
    r"model\s*\{[^}]*?scale\s*=\s*([-+]?\d*\.?\d+(?:[eE][-+]?\d+)?)",
    re.DOTALL,
)
MODEL_OFFSET_RE = re.compile(
    r"model\s*\{[^}]*?offset\s*=\s*([-+]?\d*\.?\d+(?:[eE][-+]?\d+)?)\s+"
    r"([-+]?\d*\.?\d+(?:[eE][-+]?\d+)?)\s+([-+]?\d*\.?\d+(?:[eE][-+]?\d+)?)",
    re.DOTALL,
)
TEMPLATE_REF_RE = re.compile(r"template!\s*=\s*(\w+)")


@dataclass
class VehicleRenderSpec:
    vehicle_id: str
    variant_id: str
    model_file: str | None
    mesh_ref: str | None
    scale: float
    offset: tuple[float, float, float]
    shell_texture: str | None


def _mesh_ref_base(mesh_ref: str) -> str:
    return mesh_ref.strip().split("|")[0].split("/")[-1]


def _parse_block_value(block: str, pattern: re.Pattern[str]) -> str | None:
    match = pattern.search(block)
    return match.group(1).strip() if match else None


def _parse_template_blocks(text: str) -> dict[str, str]:
    lines = text.splitlines()
    templates: dict[str, str] = {}
    index = 0
    while index < len(lines):
        match = TEMPLATE_VEHICLE_RE.match(lines[index])
        if not match:
            index += 1
            continue

        template_name = match.group(1)
        block_lines: list[str] = []
        brace_depth = 0
        start = index
        if "{" in lines[index]:
            brace_depth = lines[index].count("{") - lines[index].count("}")
            block_lines.append(lines[index])
            index += 1
        else:
            index += 1
            brace_depth = lines[index].count("{") - lines[index].count("}")
            block_lines.append(lines[index])
            index += 1

        while index < len(lines) and brace_depth > 0:
            block_lines.append(lines[index])
            brace_depth += lines[index].count("{") - lines[index].count("}")
            index += 1

        templates[template_name] = "\n".join(block_lines)
        if index == start:
            index += 1

    return templates


def build_model_registry(scripts_dir: Path) -> dict[str, str]:
    registry: dict[str, str] = {}
    for script_file in sorted(scripts_dir.rglob("*.txt")):
        text = script_file.read_text(encoding="utf-8", errors="replace")
        for match in MODEL_BLOCK_RE.finditer(text):
            registry[match.group(1)] = match.group(2).strip()
    return registry


def build_template_textures(scripts_dir: Path) -> dict[str, str]:
    textures: dict[str, str] = {}
    for script_file in sorted(scripts_dir.rglob("*.txt")):
        text = script_file.read_text(encoding="utf-8", errors="replace")
        for template_name, block in _parse_template_blocks(text).items():
            shell = _parse_block_value(block, SKIN_TEXTURE_RE)
            if shell:
                textures[template_name] = shell
    return textures


def collect_render_specs(pz_root: Path) -> list[VehicleRenderSpec]:
    scripts_dir = pz_root / "media" / "scripts"
    if not scripts_dir.exists():
        raise FileNotFoundError(f"Scripts directory not found: {scripts_dir}")

    model_registry = build_model_registry(scripts_dir)
    template_textures = build_template_textures(scripts_dir)
    specs: list[VehicleRenderSpec] = []

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

            model_file = _parse_block_value(block, MODEL_FILE_RE)
            mesh_ref = model_registry.get(model_file) if model_file else None
            mesh_name = _mesh_ref_base(mesh_ref) if mesh_ref else None

            scale_match = MODEL_SCALE_RE.search(block)
            scale = float(scale_match.group(1)) if scale_match else 1.0

            offset_match = MODEL_OFFSET_RE.search(block)
            if offset_match:
                offset = (
                    float(offset_match.group(1)),
                    float(offset_match.group(2)),
                    float(offset_match.group(3)),
                )
            else:
                offset = (0.0, 0.0, 0.0)

            shell_texture = _parse_block_value(block, SKIN_TEXTURE_RE)
            if not shell_texture:
                template_name = _parse_block_value(block, TEMPLATE_REF_RE)
                if template_name:
                    shell_texture = template_textures.get(template_name)

            specs.append(
                VehicleRenderSpec(
                    vehicle_id=vehicle_id,
                    variant_id=f"Base.{vehicle_id}",
                    model_file=model_file,
                    mesh_ref=mesh_name,
                    scale=scale,
                    offset=offset,
                    shell_texture=shell_texture,
                )
            )

    return specs


def specs_by_variant_id(specs: list[VehicleRenderSpec]) -> dict[str, VehicleRenderSpec]:
    return {spec.variant_id: spec for spec in specs}
