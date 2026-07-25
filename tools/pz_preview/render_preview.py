"""Offscreen 3D preview rendering for PZ vehicle meshes."""

from __future__ import annotations

import math
from pathlib import Path

import numpy as np
import trimesh
from PIL import Image
from trimesh import transformations
from trimesh.scene import cameras as scene_cameras

from pz_preview.pz_mesh import PZMesh, parse_pz_mesh

DEFAULT_AZIMUTH_DEG = 220.0
DEFAULT_ELEVATION_DEG = 35.0
DEFAULT_FOV = (60.0, 45.0)


def _apply_transform(vertices: np.ndarray, scale: float, offset: tuple[float, float, float]) -> np.ndarray:
    transformed = vertices * scale
    transformed += np.asarray(offset, dtype=np.float64)
    return transformed


def pz_mesh_to_trimesh(pz_mesh: PZMesh, scale: float, offset: tuple[float, float, float]) -> trimesh.Trimesh:
    vertices = _apply_transform(pz_mesh.vertices, scale, offset)
    return trimesh.Trimesh(
        vertices=vertices,
        faces=pz_mesh.faces,
        vertex_normals=pz_mesh.normals,
        process=False,
    )


def _load_texture_image(texture_path: Path) -> Image.Image:
    return Image.open(texture_path).convert("RGBA")


def _attach_texture(mesh: trimesh.Trimesh, uvs: np.ndarray, texture_path: Path) -> trimesh.Trimesh:
    mesh.visual = trimesh.visual.TextureVisuals(uv=uvs, image=_load_texture_image(texture_path))
    return mesh


def _camera_rotation(azimuth_deg: float, elevation_deg: float) -> np.ndarray:
    # trimesh orbit pitch is inverted: negative pitch places the camera above the model.
    pitch = math.radians(-elevation_deg)
    yaw = math.radians(azimuth_deg)
    return transformations.euler_matrix(pitch, yaw, 0.0)


def _camera_distance(mesh: trimesh.Trimesh) -> float:
    bounds = mesh.bounds
    size = bounds[1] - bounds[0]
    radius = float(np.linalg.norm(size)) * 0.5
    return max(radius * 2.8, 1.0)


def _configure_scene_camera(
    scene: trimesh.Scene,
    mesh: trimesh.Trimesh,
    *,
    azimuth_deg: float,
    elevation_deg: float,
) -> None:
    scene.camera_transform = scene_cameras.look_at(
        mesh.vertices,
        fov=DEFAULT_FOV,
        rotation=_camera_rotation(azimuth_deg, elevation_deg),
        distance=_camera_distance(mesh),
        center=mesh.centroid,
    )


def render_mesh_preview(
    mesh_path: Path,
    texture_path: Path,
    output_path: Path,
    *,
    scale: float = 1.0,
    offset: tuple[float, float, float] = (0.0, 0.0, 0.0),
    size: tuple[int, int] = (512, 320),
    azimuth_deg: float = DEFAULT_AZIMUTH_DEG,
    elevation_deg: float = DEFAULT_ELEVATION_DEG,
) -> None:
    pz_mesh = parse_pz_mesh(mesh_path)
    mesh = pz_mesh_to_trimesh(pz_mesh, scale, offset)
    mesh = _attach_texture(mesh, pz_mesh.uvs, texture_path)

    scene = trimesh.Scene(mesh)
    _configure_scene_camera(scene, mesh, azimuth_deg=azimuth_deg, elevation_deg=elevation_deg)

    png_bytes = scene.save_image(resolution=list(size), visible=True)
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_bytes(png_bytes)


def render_trimesh_preview(
    mesh: trimesh.Trimesh,
    texture_path: Path,
    output_path: Path,
    *,
    size: tuple[int, int] = (512, 320),
    azimuth_deg: float = DEFAULT_AZIMUTH_DEG,
    elevation_deg: float = DEFAULT_ELEVATION_DEG,
) -> None:
    textured = mesh.copy()
    uvs = getattr(textured.visual, "uv", None)
    textured.visual = trimesh.visual.TextureVisuals(
        uv=uvs,
        image=_load_texture_image(texture_path),
    )
    scene = trimesh.Scene(textured)
    _configure_scene_camera(scene, textured, azimuth_deg=azimuth_deg, elevation_deg=elevation_deg)
    png_bytes = scene.save_image(resolution=list(size), visible=True)
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_bytes(png_bytes)
