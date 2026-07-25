"""Resolve script mesh names to on-disk mesh assets."""

from __future__ import annotations

from dataclasses import dataclass
from enum import Enum
from pathlib import Path

MESH_ALIASES: dict[str, str] = {
    "Vehicles_CarNormalLights": "Vehicles_CarLights",
    "Vehicles_VanAmbulance": "Vehicles_Ambulance",
}

MESH_FALLBACKS: dict[str, str] = {
    "vehicle_racecar": "Vehicles_SportsCar",
    "Vehicles_ModernCarLights": "Vehicles_ModernCar",
    "SportsCarWithDoors": "Vehicles_SportsCar",
    "ModernCarWithDoors_Martin": "Vehicles_ModernCar",
}


class MeshSourceKind(str, Enum):
    TXT = "txt"
    FBX = "fbx"
    FBX_FALLBACK = "fbx_fallback"


@dataclass
class ResolvedMesh:
    kind: MeshSourceKind
    path: Path
    mesh_name: str
    requested_mesh_name: str | None = None


class MeshResolver:
    def __init__(self, pz_root: Path) -> None:
        self.pz_root = pz_root
        self.models_dir = pz_root / "media" / "models"
        self.fbx_dir = pz_root / "media" / "models_X" / "vehicles"
        self._txt_index = {path.stem.lower(): path for path in self.models_dir.glob("*.txt")}
        self._fbx_index = {
            path.stem.lower(): path for path in self.fbx_dir.glob("*") if path.suffix.lower() == ".fbx"
        }

    def resolve(self, mesh_name: str | None) -> ResolvedMesh | None:
        if not mesh_name:
            return None

        candidates = [mesh_name]
        alias = MESH_ALIASES.get(mesh_name)
        if alias:
            candidates.insert(0, alias)

        for candidate in candidates:
            txt_path = self.models_dir / f"{candidate}.txt"
            if txt_path.exists():
                return ResolvedMesh(MeshSourceKind.TXT, txt_path, candidate)

            indexed = self._txt_index.get(candidate.lower())
            if indexed:
                return ResolvedMesh(MeshSourceKind.TXT, indexed, indexed.stem)

        for candidate in candidates:
            fallback = MESH_FALLBACKS.get(candidate)
            if not fallback:
                continue
            txt_path = self.models_dir / f"{fallback}.txt"
            if txt_path.exists():
                return ResolvedMesh(
                    MeshSourceKind.FBX_FALLBACK,
                    txt_path,
                    fallback,
                    requested_mesh_name=mesh_name,
                )
            indexed = self._txt_index.get(fallback.lower())
            if indexed:
                return ResolvedMesh(
                    MeshSourceKind.FBX_FALLBACK,
                    indexed,
                    indexed.stem,
                    requested_mesh_name=mesh_name,
                )

        for candidate in candidates:
            fbx_path = self._find_fbx(candidate)
            if fbx_path:
                return ResolvedMesh(
                    MeshSourceKind.FBX,
                    fbx_path,
                    candidate,
                    requested_mesh_name=mesh_name,
                )

        return None

    def _find_fbx(self, mesh_name: str) -> Path | None:
        direct = self.fbx_dir / f"{mesh_name}.fbx"
        if direct.exists():
            return direct

        direct_upper = self.fbx_dir / f"{mesh_name}.FBX"
        if direct_upper.exists():
            return direct_upper

        indexed = self._fbx_index.get(mesh_name.lower())
        if indexed:
            return indexed

        prefixed = self._fbx_index.get(f"vehicles_{mesh_name.lower()}")
        if prefixed:
            return prefixed

        return None
