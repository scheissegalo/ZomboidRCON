"""Load FBX vehicle meshes for preview rendering."""

from __future__ import annotations

import shutil
import subprocess
import tempfile
from pathlib import Path

import numpy as np
import trimesh

MESH_FALLBACKS: dict[str, str] = {
    "vehicle_racecar": "Vehicles_SportsCar",
    "Vehicles_ModernCarLights": "Vehicles_ModernCar",
    "SportsCarWithDoors": "Vehicles_SportsCar",
    "ModernCarWithDoors_Martin": "Vehicles_ModernCar",
}


def fallback_mesh_name(mesh_name: str) -> str | None:
    return MESH_FALLBACKS.get(mesh_name)


def _merge_trimesh_objects(objects: list[trimesh.Trimesh]) -> trimesh.Trimesh:
    if len(objects) == 1:
        return objects[0]
    return trimesh.util.concatenate(objects)


def load_obj_mesh(obj_path: Path) -> trimesh.Trimesh:
    loaded = trimesh.load(obj_path, force="mesh", process=False)
    if isinstance(loaded, trimesh.Scene):
        meshes = [geom for geom in loaded.geometry.values() if isinstance(geom, trimesh.Trimesh)]
        if not meshes:
            raise RuntimeError(f"No mesh geometry found in {obj_path}")
        return _merge_trimesh_objects(meshes)
    if not isinstance(loaded, trimesh.Trimesh):
        raise RuntimeError(f"Unsupported OBJ payload in {obj_path}: {type(loaded)!r}")
    return loaded


def convert_mesh_to_obj(mesh_path: Path, obj_path: Path) -> None:
    assimp = shutil.which("assimp")
    if not assimp:
        raise RuntimeError(
            "Mesh conversion requires the assimp CLI (assimp-utils package) or Blender. "
            "Install assimp-utils or use a chassis fallback mesh."
        )

    result = subprocess.run(
        [assimp, "export", str(mesh_path), str(obj_path)],
        capture_output=True,
        text=True,
        check=False,
    )
    if result.returncode != 0 or not obj_path.exists():
        raise RuntimeError(
            "assimp mesh export failed\n"
            f"command: {assimp} export {mesh_path} {obj_path}\n"
            f"stdout:\n{result.stdout}\n"
            f"stderr:\n{result.stderr}"
        )


def convert_fbx_to_obj(fbx_path: Path, obj_path: Path) -> None:
    convert_mesh_to_obj(fbx_path, obj_path)


def load_mesh_asset(mesh_path: Path) -> trimesh.Trimesh:
    with tempfile.TemporaryDirectory(prefix="pz_mesh_") as temp_dir:
        obj_path = Path(temp_dir) / f"{mesh_path.stem}.obj"
        convert_mesh_to_obj(mesh_path, obj_path)
        return load_obj_mesh(obj_path)


def load_fbx_mesh(fbx_path: Path) -> trimesh.Trimesh:
    return load_mesh_asset(fbx_path)


def apply_transform(
    mesh: trimesh.Trimesh,
    scale: float,
    offset: tuple[float, float, float],
) -> trimesh.Trimesh:
    transformed = mesh.copy()
    transformed.apply_scale(scale)
    transformed.apply_translation(np.asarray(offset, dtype=np.float64))
    return transformed
