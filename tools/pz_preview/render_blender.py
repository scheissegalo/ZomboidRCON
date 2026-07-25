"""Invoke Blender headless to render FBX vehicle previews."""

from __future__ import annotations

import shutil
import subprocess
from pathlib import Path


def find_blender(explicit: str | None = None) -> str | None:
    if explicit:
        path = Path(explicit)
        if path.exists():
            return str(path)
        resolved = shutil.which(explicit)
        if resolved:
            return resolved
        return None
    return shutil.which("blender")


def render_fbx_preview(
    blender_executable: str,
    blender_script: Path,
    fbx_path: Path,
    texture_path: Path,
    output_path: Path,
    *,
    scale: float = 1.0,
    offset: tuple[float, float, float] = (0.0, 0.0, 0.0),
    size: tuple[int, int] = (512, 320),
    azimuth_deg: float = 220.0,
    elevation_deg: float = 35.0,
) -> None:
    output_path.parent.mkdir(parents=True, exist_ok=True)
    command = [
        blender_executable,
        "--background",
        "--python",
        str(blender_script),
        "--",
        str(fbx_path),
        str(texture_path),
        str(output_path),
        str(size[0]),
        str(size[1]),
        str(scale),
        str(offset[0]),
        str(offset[1]),
        str(offset[2]),
        str(azimuth_deg),
        str(elevation_deg),
    ]
    result = subprocess.run(command, capture_output=True, text=True, check=False)
    combined = f"{result.stdout}\n{result.stderr}"
    if result.returncode != 0 or not output_path.exists():
        raise RuntimeError(
            "Blender preview render failed\n"
            f"command: {' '.join(command)}\n"
            f"stdout:\n{result.stdout}\n"
            f"stderr:\n{result.stderr}"
        )
    if "Error:" in combined or "Traceback" in combined:
        raise RuntimeError(
            "Blender preview render reported errors\n"
            f"command: {' '.join(command)}\n"
            f"stdout:\n{result.stdout}\n"
            f"stderr:\n{result.stderr}"
        )
