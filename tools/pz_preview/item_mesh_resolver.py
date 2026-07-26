"""Resolve item mesh references to on-disk models_X assets."""

from __future__ import annotations

from dataclasses import dataclass
from enum import Enum
from pathlib import Path

MESH_EXTENSIONS = (".FBX", ".fbx", ".X", ".x")


class ItemMeshKind(str, Enum):
    FBX = "fbx"
    X = "x"


@dataclass
class ResolvedItemMesh:
    kind: ItemMeshKind
    path: Path
    mesh_ref: str


class ItemMeshResolver:
    def __init__(self, pz_root: Path) -> None:
        self.models_x_dir = pz_root / "media" / "models_X"

    def resolve(self, mesh_ref: str | None) -> ResolvedItemMesh | None:
        if not mesh_ref:
            return None

        mesh_ref = mesh_ref.strip().replace("\\", "/")
        candidates: list[Path] = []

        direct = self.models_x_dir / mesh_ref
        if direct.suffix:
            candidates.append(direct)
        else:
            candidates.extend(direct.with_suffix(ext) for ext in MESH_EXTENSIONS)

        basename = Path(mesh_ref).name
        for ext in MESH_EXTENSIONS:
            candidates.append(self.models_x_dir / "WorldItems" / f"{basename}{ext}")
            candidates.append(self.models_x_dir / f"{basename}{ext}")

        seen: set[Path] = set()
        for candidate in candidates:
            if candidate in seen:
                continue
            seen.add(candidate)
            if candidate.exists():
                kind = ItemMeshKind.X if candidate.suffix.lower() == ".x" else ItemMeshKind.FBX
                return ResolvedItemMesh(kind=kind, path=candidate, mesh_ref=mesh_ref)

        return None
