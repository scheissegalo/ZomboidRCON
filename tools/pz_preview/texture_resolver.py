"""Resolve item texture references to on-disk PNG assets."""

from __future__ import annotations

from pathlib import Path


class ItemTextureResolver:
    def __init__(self, pz_root: Path) -> None:
        self.textures_dir = pz_root / "media" / "textures"

    def resolve(
        self,
        texture_ref: str | None,
        *,
        mesh_ref: str | None = None,
        model_key: str | None = None,
    ) -> Path | None:
        candidates: list[str] = []
        if texture_ref:
            candidates.append(texture_ref.strip().replace("\\", "/"))

        if mesh_ref:
            mesh_ref = mesh_ref.strip().replace("\\", "/")
            candidates.append(mesh_ref)
            candidates.append(Path(mesh_ref).name)
            if "weapons/" in mesh_ref:
                weapon_subpath = mesh_ref.split("weapons/", 1)[1]
                candidates.append(f"weapons/{weapon_subpath}")

        if model_key:
            candidates.append(model_key)
            if mesh_ref and "weapons/" in mesh_ref:
                weapon_dir = Path(mesh_ref).parent.name
                candidates.append(f"weapons/{weapon_dir}/{model_key}")

        seen: set[str] = set()
        for ref in candidates:
            if not ref or ref in seen:
                continue
            seen.add(ref)

            relative = ref[:-4] if ref.lower().endswith(".png") else ref
            for path in (
                self.textures_dir / f"{relative}.png",
                self.textures_dir / "WorldItems" / f"{Path(relative).name}.png",
                self.textures_dir / "weapons" / f"{Path(relative).name}.png",
                self.textures_dir / "weapons" / "firearm" / f"{Path(relative).name}.png",
                self.textures_dir / "weapons" / "1handed" / f"{Path(relative).name}.png",
                self.textures_dir / "weapons" / "2handed" / f"{Path(relative).name}.png",
            ):
                if path.exists():
                    return path

        return None
