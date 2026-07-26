#!/usr/bin/env python3
"""Generate item preview JPEGs from Project Zomboid mesh and texture assets."""

from __future__ import annotations

import argparse
import sys
from dataclasses import dataclass
from pathlib import Path

TOOLS_DIR = Path(__file__).resolve().parent
if str(TOOLS_DIR) not in sys.path:
    sys.path.insert(0, str(TOOLS_DIR))

from pz_preview.item_mesh_resolver import ItemMeshResolver
from pz_preview.pz_item_meta import ItemRenderSpec, collect_item_render_specs, specs_by_item_id
from pz_preview.render_preview import render_item_preview
from pz_preview.texture_resolver import ItemTextureResolver


@dataclass
class ResolvedItemPreview:
    item_id: str
    mesh_path: Path | None
    texture_path: Path | None
    scale: float
    issues: list[str]


def normalize_pz_root(pz_root: Path) -> Path:
    if (pz_root / "projectzomboid" / "media").exists():
        return pz_root / "projectzomboid"
    return pz_root


def parse_size(value: str) -> tuple[int, int]:
    if "x" not in value.lower():
        raise argparse.ArgumentTypeError("Size must look like WIDTHxHEIGHT, e.g. 256x256")
    width_text, height_text = value.lower().split("x", 1)
    width = int(width_text)
    height = int(height_text)
    if width <= 0 or height <= 0:
        raise argparse.ArgumentTypeError("Size dimensions must be positive")
    return width, height


def output_extension(image_format: str) -> str:
    if image_format.lower() in {"jpg", "jpeg"}:
        return ".jpg"
    return ".png"


def resolve_item_preview(
    spec: ItemRenderSpec,
    mesh_resolver: ItemMeshResolver,
    texture_resolver: ItemTextureResolver,
) -> ResolvedItemPreview:
    issues = list(spec.issues)
    mesh = mesh_resolver.resolve(spec.mesh_ref)
    if spec.mesh_ref and not mesh:
        issues.append(f"mesh file not found for {spec.mesh_ref!r}")

    texture_path = texture_resolver.resolve(
        spec.texture_ref,
        mesh_ref=spec.mesh_ref,
        model_key=spec.model_key,
    )

    return ResolvedItemPreview(
        item_id=spec.item_id,
        mesh_path=mesh.path if mesh else None,
        texture_path=texture_path,
        scale=spec.scale,
        issues=issues,
    )


def can_render(resolved: ResolvedItemPreview) -> bool:
    return resolved.mesh_path is not None and not any(
        issue.startswith("no model field") or issue.startswith("model ") and "not in registry" in issue
        for issue in resolved.issues
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
        default=Path("ZomboidRCON/Resources/pz_items.json"),
        help="Item catalog JSON with id entries",
    )
    parser.add_argument(
        "--output",
        type=Path,
        default=Path("ZomboidRCON/Assets/Items"),
        help="Directory for generated preview images",
    )
    parser.add_argument(
        "--size",
        type=parse_size,
        default="256x256",
        help="Output image size as WIDTHxHEIGHT (default: 256x256)",
    )
    parser.add_argument(
        "--format",
        dest="image_format",
        choices=["jpeg", "jpg", "png"],
        default="jpeg",
        help="Output image format (default: jpeg)",
    )
    parser.add_argument(
        "--quality",
        type=int,
        default=85,
        help="JPEG quality when --format jpeg (default: 85)",
    )
    parser.add_argument(
        "--only",
        default="",
        help="Comma-separated item IDs to render (default: all catalog items)",
    )
    parser.add_argument(
        "--limit",
        type=int,
        default=0,
        help="Render at most N items (default: 0 = no limit)",
    )
    parser.add_argument(
        "--force",
        action="store_true",
        help="Overwrite existing preview images",
    )
    parser.add_argument(
        "--skip-existing",
        action="store_true",
        default=True,
        help="Skip items that already have previews (default: enabled)",
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

    specs = specs_by_item_id(collect_item_render_specs(pz_root, catalog_path))
    item_ids = list(specs.keys())
    if args.only.strip():
        requested = {item.strip() for item in args.only.split(",") if item.strip()}
        item_ids = [item_id for item_id in item_ids if item_id in requested]
    if args.limit > 0:
        item_ids = item_ids[: args.limit]

    mesh_resolver = ItemMeshResolver(pz_root)
    texture_resolver = ItemTextureResolver(pz_root)
    extension = output_extension(args.image_format)

    rendered = 0
    skipped = 0
    failed = 0
    no_model = 0
    mesh_only = 0

    for item_id in item_ids:
        output_path = output_dir / f"{item_id}{extension}"
        if args.skip_existing and output_path.exists():
            skipped += 1
            continue

        spec = specs[item_id]
        resolved = resolve_item_preview(spec, mesh_resolver, texture_resolver)

        if not can_render(resolved):
            no_model += 1
            if args.dry_run:
                print(f"SKIP {item_id}: {'; '.join(resolved.issues) or 'no renderable mesh'}")
            continue

        if args.dry_run:
            texture_note = resolved.texture_path if resolved.texture_path else "none (gray fallback)"
            print(f"OK   {item_id}: mesh={resolved.mesh_path} texture={texture_note}")
            continue

        try:
            render_item_preview(
                resolved.mesh_path,
                resolved.texture_path,
                output_path,
                scale=resolved.scale,
                size=args.size,
                image_format=args.image_format,
                jpeg_quality=args.quality,
            )
            rendered += 1
            if resolved.texture_path is None:
                mesh_only += 1
            print(f"OK   {item_id} -> {output_path}")
        except Exception as exc:
            failed += 1
            print(f"FAIL {item_id}: {exc}")

    if args.dry_run:
        renderable = sum(1 for item_id in item_ids if can_render(resolve_item_preview(specs[item_id], mesh_resolver, texture_resolver)))
        print(
            f"Dry run complete: {len(item_ids)} items, "
            f"{renderable} renderable, {no_model} skipped (no mesh), {skipped} skipped existing"
        )
    else:
        print(
            f"Done: rendered={rendered}, skipped={skipped}, failed={failed}, "
            f"no_model={no_model}, mesh_only={mesh_only}"
        )

    if failed:
        raise SystemExit(1)


if __name__ == "__main__":
    main()
