"""Parse Project Zomboid Mesh (.txt) files."""

from __future__ import annotations

import re
from dataclasses import dataclass
from pathlib import Path

import numpy as np

FLOAT3_RE = re.compile(
    r"^\s*([-+]?\d*\.?\d+(?:[eE][-+]?\d+)?)\s*,\s*"
    r"([-+]?\d*\.?\d+(?:[eE][-+]?\d+)?)\s*,\s*"
    r"([-+]?\d*\.?\d+(?:[eE][-+]?\d+)?)\s*$"
)
FLOAT2_RE = re.compile(
    r"^\s*([-+]?\d*\.?\d+(?:[eE][-+]?\d+)?)\s*,\s*"
    r"([-+]?\d*\.?\d+(?:[eE][-+]?\d+)?)\s*$"
)
FACE_RE = re.compile(r"^\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*$")


@dataclass
class PZMesh:
    name: str
    vertices: np.ndarray
    normals: np.ndarray
    uvs: np.ndarray
    faces: np.ndarray


def _parse_float3(line: str) -> tuple[float, float, float]:
    match = FLOAT3_RE.match(line.strip())
    if not match:
        raise ValueError(f"Expected float3 line, got: {line!r}")
    return float(match.group(1)), float(match.group(2)), float(match.group(3))


def _parse_float2(line: str) -> tuple[float, float]:
    match = FLOAT2_RE.match(line.strip())
    if not match:
        raise ValueError(f"Expected float2 line, got: {line!r}")
    return float(match.group(1)), float(match.group(2))


def parse_pz_mesh(path: Path) -> PZMesh:
    text = path.read_text(encoding="utf-8", errors="replace")
    lines = text.splitlines()

    name = path.stem
    vertex_count = 0
    face_count = 0
    buffer_start = 0
    faces_start = 0

    for index, line in enumerate(lines):
        stripped = line.strip()
        if stripped == "# Model Name:" and index + 1 < len(lines):
            name = lines[index + 1].strip()
        elif stripped == "# Vertex Count:" and index + 1 < len(lines):
            vertex_count = int(lines[index + 1].strip())
        elif stripped == "# Vertex Buffer:":
            buffer_start = index + 1
        elif stripped == "# Number of Faces:" and index + 1 < len(lines):
            face_count = int(lines[index + 1].strip())
        elif stripped == "# Face Data:":
            faces_start = index + 1

    if vertex_count <= 0:
        raise ValueError(f"No vertices found in mesh: {path}")

    vertices: list[list[float]] = []
    normals: list[list[float]] = []
    uvs: list[list[float]] = []

    line_index = buffer_start
    for _ in range(vertex_count):
        if line_index + 2 >= len(lines):
            raise ValueError(f"Unexpected end of vertex buffer in {path}")
        vertices.append(list(_parse_float3(lines[line_index])))
        normals.append(list(_parse_float3(lines[line_index + 1])))
        uvs.append(list(_parse_float2(lines[line_index + 2])))
        line_index += 3

    faces: list[list[int]] = []
    for offset in range(face_count):
        face_line = lines[faces_start + offset]
        match = FACE_RE.match(face_line.strip())
        if not match:
            raise ValueError(f"Invalid face line in {path}: {face_line!r}")
        faces.append([int(match.group(1)), int(match.group(2)), int(match.group(3))])

    return PZMesh(
        name=name,
        vertices=np.asarray(vertices, dtype=np.float64),
        normals=np.asarray(normals, dtype=np.float64),
        uvs=np.asarray(uvs, dtype=np.float64),
        faces=np.asarray(faces, dtype=np.int64),
    )
