"""Offscreen 3D preview rendering for PZ vehicle and item meshes."""

from __future__ import annotations

import io
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
ITEM_CAMERA_PAD = 1.30
DEFAULT_JPEG_BACKGROUND = (32, 32, 36)
DEFAULT_JPEG_QUALITY = 85


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


def _camera_distance(mesh: trimesh.Trimesh, *, distance_factor: float = 2.8) -> float:
    bounds = mesh.bounds
    size = bounds[1] - bounds[0]
    radius = float(np.linalg.norm(size)) * 0.5
    return max(radius * distance_factor, 1.0)


def _bounding_box_corners(bounds: np.ndarray) -> np.ndarray:
    minimum, maximum = bounds
    return np.array(
        [
            [minimum[0], minimum[1], minimum[2]],
            [maximum[0], minimum[1], minimum[2]],
            [maximum[0], maximum[1], minimum[2]],
            [minimum[0], maximum[1], minimum[2]],
            [minimum[0], minimum[1], maximum[2]],
            [maximum[0], minimum[1], maximum[2]],
            [maximum[0], maximum[1], maximum[2]],
            [minimum[0], maximum[1], maximum[2]],
        ],
        dtype=np.float64,
    )


def _configure_bounds_camera(
    scene: trimesh.Scene,
    mesh: trimesh.Trimesh,
    *,
    azimuth_deg: float,
    elevation_deg: float,
    pad: float = ITEM_CAMERA_PAD,
) -> None:
    scene.camera_transform = scene_cameras.look_at(
        _bounding_box_corners(mesh.bounds),
        fov=DEFAULT_FOV,
        rotation=_camera_rotation(azimuth_deg, elevation_deg),
        center=mesh.centroid,
        pad=pad,
    )


def _configure_scene_camera(
    scene: trimesh.Scene,
    mesh: trimesh.Trimesh,
    *,
    azimuth_deg: float,
    elevation_deg: float,
    distance_factor: float = 2.8,
) -> None:
    scene.camera_transform = scene_cameras.look_at(
        mesh.vertices,
        fov=DEFAULT_FOV,
        rotation=_camera_rotation(azimuth_deg, elevation_deg),
        distance=_camera_distance(mesh, distance_factor=distance_factor),
        center=mesh.centroid,
    )


def _save_image_bytes(
    png_bytes: bytes,
    output_path: Path,
    *,
    image_format: str = "png",
    jpeg_quality: int = DEFAULT_JPEG_QUALITY,
    jpeg_background: tuple[int, int, int] = DEFAULT_JPEG_BACKGROUND,
) -> None:
    output_path.parent.mkdir(parents=True, exist_ok=True)
    image = Image.open(io.BytesIO(png_bytes)).convert("RGBA")

    if image_format.lower() in {"jpg", "jpeg"}:
        background = Image.new("RGB", image.size, jpeg_background)
        background.paste(image, mask=image.split()[3])
        background.save(output_path, format="JPEG", quality=jpeg_quality, optimize=True)
        return

    image.save(output_path, format="PNG")


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
    _save_image_bytes(png_bytes, output_path)


def render_trimesh_preview(
    mesh: trimesh.Trimesh,
    texture_path: Path | None,
    output_path: Path,
    *,
    size: tuple[int, int] = (512, 320),
    azimuth_deg: float = DEFAULT_AZIMUTH_DEG,
    elevation_deg: float = DEFAULT_ELEVATION_DEG,
    distance_factor: float = 2.8,
    auto_frame: bool = False,
    frame_pad: float = ITEM_CAMERA_PAD,
    image_format: str = "png",
    jpeg_quality: int = DEFAULT_JPEG_QUALITY,
    jpeg_background: tuple[int, int, int] = DEFAULT_JPEG_BACKGROUND,
) -> None:
    textured = mesh.copy()
    uvs = getattr(textured.visual, "uv", None)
    if texture_path is not None:
        textured.visual = trimesh.visual.TextureVisuals(
            uv=uvs,
            image=_load_texture_image(texture_path),
        )
    elif uvs is None:
        textured.visual = trimesh.visual.ColorVisuals(
            vertex_colors=np.full((len(textured.vertices), 4), [160, 160, 160, 255], dtype=np.uint8)
        )
    scene = trimesh.Scene(textured)
    if auto_frame:
        _configure_bounds_camera(
            scene,
            textured,
            azimuth_deg=azimuth_deg,
            elevation_deg=elevation_deg,
            pad=frame_pad,
        )
    else:
        _configure_scene_camera(
            scene,
            textured,
            azimuth_deg=azimuth_deg,
            elevation_deg=elevation_deg,
            distance_factor=distance_factor,
        )
    png_bytes = scene.save_image(resolution=list(size), visible=True)
    _save_image_bytes(
        png_bytes,
        output_path,
        image_format=image_format,
        jpeg_quality=jpeg_quality,
        jpeg_background=jpeg_background,
    )


def render_item_preview(
    mesh_path: Path,
    texture_path: Path | None,
    output_path: Path,
    *,
    scale: float = 1.0,
    size: tuple[int, int] = (256, 256),
    azimuth_deg: float = DEFAULT_AZIMUTH_DEG,
    elevation_deg: float = DEFAULT_ELEVATION_DEG,
    image_format: str = "jpeg",
    jpeg_quality: int = DEFAULT_JPEG_QUALITY,
) -> None:
    from pz_preview.fbx_loader import apply_transform, load_mesh_asset

    mesh = apply_transform(load_mesh_asset(mesh_path), scale, (0.0, 0.0, 0.0))
    render_trimesh_preview(
        mesh,
        texture_path,
        output_path,
        size=size,
        azimuth_deg=azimuth_deg,
        elevation_deg=elevation_deg,
        auto_frame=True,
        image_format=image_format,
        jpeg_quality=jpeg_quality,
    )
