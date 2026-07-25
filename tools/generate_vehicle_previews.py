#!/usr/bin/env python3
"""Generate vehicle preview PNGs from Project Zomboid mesh and texture assets."""

from __future__ import annotations

import argparse
import json
import sys
from dataclasses import dataclass
from pathlib import Path

TOOLS_DIR = Path(__file__).resolve().parent
if str(TOOLS_DIR) not in sys.path:
    sys.path.insert(0, str(TOOLS_DIR))

from pz_preview.mesh_resolver import MeshResolver, MeshSourceKind
from pz_preview.pz_vehicle_meta import VehicleRenderSpec, collect_render_specs, specs_by_variant_id
from pz_preview.render_blender import find_blender


@dataclass
class ResolvedPreview:
    variant_id: str
    mesh_kind: MeshSourceKind | None
    mesh_path: Path | None
    texture_path: Path | None
    scale: float
    offset: tuple[float, float, float]
    issues: list[str]


def normalize_pz_root(pz_root: Path) -> Path:
    if (pz_root / "projectzomboid" / "media").exists():
        return pz_root / "projectzomboid"
    return pz_root


def parse_size(value: str) -> tuple[int, int]:
    if "x" not in value.lower():
        raise argparse.ArgumentTypeError("Size must look like WIDTHxHEIGHT, e.g. 512x320")
    width_text, height_text = value.lower().split("x", 1)
    width = int(width_text)
    height = int(height_text)
    if width <= 0 or height <= 0:
        raise argparse.ArgumentTypeError("Size dimensions must be positive")
    return width, height


def load_catalog_variant_ids(catalog_path: Path) -> list[str]:
    data = json.loads(catalog_path.read_text(encoding="utf-8"))
    variant_ids: list[str] = []
    for group in data.get("vehicles", []):
        for variant in group.get("variants", []):
            variant_id = variant.get("variantId")
            if variant_id:
                variant_ids.append(variant_id)
    return variant_ids


def resolve_texture_path(pz_root: Path, shell_texture: str | None) -> Path | None:
    if not shell_texture:
        return None
    relative = shell_texture.replace("\\", "/").strip()
    if relative.lower().endswith(".png"):
        relative = relative[:-4]
    path = pz_root / "media" / "textures" / f"{relative}.png"
    if path.exists():
        return path
    return None


def resolve_preview(
    spec: VehicleRenderSpec,
    resolver: MeshResolver,
    pz_root: Path,
) -> ResolvedPreview:
    issues: list[str] = []
    mesh = resolver.resolve(spec.mesh_ref)
    if not mesh:
        issues.append(f"mesh not found for {spec.mesh_ref!r}")

    texture_path = resolve_texture_path(pz_root, spec.shell_texture)
    if not texture_path:
        issues.append(f"texture not found for {spec.shell_texture!r}")

    return ResolvedPreview(
        variant_id=spec.variant_id,
        mesh_kind=mesh.kind if mesh else None,
        mesh_path=mesh.path if mesh else None,
        texture_path=texture_path,
        scale=spec.scale,
        offset=spec.offset,
        issues=issues,
    )


def render_preview(
    resolved: ResolvedPreview,
    output_path: Path,
    *,
    size: tuple[int, int],
    blender_executable: str | None,
    blender_script: Path,
) -> None:
    if resolved.issues:
        raise RuntimeError("; ".join(resolved.issues))
    if resolved.mesh_path is None or resolved.texture_path is None or resolved.mesh_kind is None:
        raise RuntimeError(f"Missing assets for {resolved.variant_id}")

    if resolved.mesh_kind in {MeshSourceKind.TXT, MeshSourceKind.FBX_FALLBACK}:
        from pz_preview.render_preview import render_mesh_preview

        render_mesh_preview(
            resolved.mesh_path,
            resolved.texture_path,
            output_path,
            scale=resolved.scale,
            offset=resolved.offset,
            size=size,
        )
        return

    from pz_preview.fbx_loader import apply_transform, load_fbx_mesh
    from pz_preview.render_preview import render_trimesh_preview

    try:
        mesh = apply_transform(load_fbx_mesh(resolved.mesh_path), resolved.scale, resolved.offset)
        render_trimesh_preview(mesh, resolved.texture_path, output_path, size=size)
        return
    except RuntimeError:
        if not blender_executable:
            raise RuntimeError(
                f"{resolved.variant_id} requires assimp-utils or Blender for FBX rendering. "
                "Install assimp-utils, pass --blender, or rely on chassis fallback meshes."
            ) from None

    from pz_preview.render_blender import render_fbx_preview

    render_fbx_preview(
        blender_executable,
        blender_script,
        resolved.mesh_path,
        resolved.texture_path,
        output_path,
        scale=resolved.scale,
        offset=resolved.offset,
        size=size,
    )


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "pz_root",
        type=Path,
        help="Path to Project Zomboid install (folder containing media/)",
    )
    parser.add_argument(
        "--catalog",
        type=Path,
        default=Path("ZomboidRCON/Resources/default_vehicles.json"),
        help="Vehicle catalog JSON with variantId entries",
    )
    parser.add_argument(
        "--output",
        type=Path,
        default=Path("ZomboidRCON/Assets/Vehicles"),
        help="Directory for generated PNG previews",
    )
    parser.add_argument(
        "--size",
        type=parse_size,
        default="512x320",
        help="Output image size as WIDTHxHEIGHT (default: 512x320)",
    )
    parser.add_argument(
        "--blender",
        default=None,
        help="Blender executable for FBX meshes (default: search PATH)",
    )
    parser.add_argument(
        "--only",
        default="",
        help="Comma-separated variant IDs to render (default: all catalog variants)",
    )
    parser.add_argument(
        "--force",
        action="store_true",
        help="Overwrite existing PNG previews",
    )
    parser.add_argument(
        "--skip-existing",
        action="store_true",
        default=True,
        help="Skip variants that already have PNGs (default: enabled)",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Print resolved assets without rendering",
    )
    args = parser.parse_args()

    if args.force:
        args.skip_existing = False

    pz_root = normalize_pz_root(args.pz_root)
    if not (pz_root / "media").exists():
        raise SystemExit(f"Project Zomboid media directory not found under: {pz_root}")

    catalog_path = args.catalog
    if not catalog_path.is_absolute():
        catalog_path = Path.cwd() / catalog_path
    if not catalog_path.exists():
        raise SystemExit(f"Catalog not found: {catalog_path}")

    output_dir = args.output
    if not output_dir.is_absolute():
        output_dir = Path.cwd() / output_dir

    variant_ids = load_catalog_variant_ids(catalog_path)
    if args.only.strip():
        requested = {item.strip() for item in args.only.split(",") if item.strip()}
        variant_ids = [variant_id for variant_id in variant_ids if variant_id in requested]

    specs = specs_by_variant_id(collect_render_specs(pz_root))
    resolver = MeshResolver(pz_root)
    blender_executable = find_blender(args.blender)
    blender_script = TOOLS_DIR / "blender_export_vehicle_preview.py"

    rendered = 0
    skipped = 0
    failed = 0
    txt_count = 0
    fbx_count = 0
    missing = 0

    for variant_id in variant_ids:
        output_path = output_dir / f"{variant_id}.png"
        if args.skip_existing and output_path.exists():
            skipped += 1
            continue

        spec = specs.get(variant_id)
        if not spec:
            failed += 1
            print(f"FAIL {variant_id}: no vehicle script metadata found")
            continue

        resolved = resolve_preview(spec, resolver, pz_root)
        if args.dry_run:
            status = "OK" if not resolved.issues else "MISSING"
            if status == "OK" and resolved.mesh_kind in {MeshSourceKind.TXT, MeshSourceKind.FBX_FALLBACK}:
                txt_count += 1
            elif status == "OK" and resolved.mesh_kind == MeshSourceKind.FBX:
                fbx_count += 1
            else:
                missing += 1
            print(
                f"{status} {variant_id}: mesh={resolved.mesh_kind} "
                f"{resolved.mesh_path} texture={resolved.texture_path} issues={resolved.issues}"
            )
            continue

        if resolved.issues:
            failed += 1
            print(f"FAIL {variant_id}: {'; '.join(resolved.issues)}")
            continue

        try:
            render_preview(
                resolved,
                output_path,
                size=args.size,
                blender_executable=blender_executable,
                blender_script=blender_script,
            )
            rendered += 1
            if resolved.mesh_kind in {MeshSourceKind.TXT, MeshSourceKind.FBX_FALLBACK}:
                txt_count += 1
            elif resolved.mesh_kind == MeshSourceKind.FBX:
                fbx_count += 1
            print(f"OK   {variant_id} -> {output_path}")
        except Exception as exc:
            failed += 1
            print(f"FAIL {variant_id}: {exc}")

    if args.dry_run:
        print(
            f"Dry run complete: {len(variant_ids)} variants, "
            f"{txt_count} txt, {fbx_count} fbx, {missing} missing, {skipped} skipped existing"
        )
    else:
        print(
            f"Done: rendered={rendered}, skipped={skipped}, failed={failed}, "
            f"txt={txt_count}, fbx={fbx_count}"
        )

    if failed:
        raise SystemExit(1)


if __name__ == "__main__":
    main()
