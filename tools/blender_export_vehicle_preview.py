"""Headless Blender script to render a textured vehicle FBX preview."""

from __future__ import annotations

import math
import sys
from pathlib import Path

import bpy
from mathutils import Vector


def _parse_args() -> dict[str, object]:
    argv = sys.argv
    if "--" not in argv:
        raise SystemExit("Expected arguments after --")
    args = argv[argv.index("--") + 1 :]
    if len(args) < 11:
        raise SystemExit(
            "Usage: blender --background --python blender_export_vehicle_preview.py -- "
            "fbx texture output width height scale ox oy oz azimuth elevation"
        )
    return {
        "fbx": Path(args[0]),
        "texture": Path(args[1]),
        "output": Path(args[2]),
        "width": int(args[3]),
        "height": int(args[4]),
        "scale": float(args[5]),
        "offset": (float(args[6]), float(args[7]), float(args[8])),
        "azimuth": float(args[9]),
        "elevation": float(args[10]),
    }


def _clear_scene() -> None:
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for block in bpy.data.meshes:
        bpy.data.meshes.remove(block)
    for block in bpy.data.materials:
        bpy.data.materials.remove(block)
    for block in bpy.data.images:
        bpy.data.images.remove(block)


def _import_fbx(path: Path) -> bpy.types.Object:
    bpy.ops.import_scene.fbx(filepath=str(path), use_anim=False)
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    if not meshes:
        raise RuntimeError(f"No mesh objects imported from {path}")
    if len(meshes) == 1:
        return meshes[0]

    bpy.ops.object.select_all(action="DESELECT")
    for obj in meshes:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = meshes[0]
    bpy.ops.object.join()
    return bpy.context.view_layer.objects.active


def _apply_transform(obj: bpy.types.Object, scale: float, offset: tuple[float, float, float]) -> None:
    obj.scale = (scale, scale, scale)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.location = Vector(offset)


def _assign_texture(obj: bpy.types.Object, texture_path: Path) -> None:
    material = bpy.data.materials.new(name="VehicleShell")
    material.use_nodes = True
    nodes = material.node_tree.nodes
    links = material.node_tree.links
    nodes.clear()

    output = nodes.new(type="ShaderNodeOutputMaterial")
    principled = nodes.new(type="ShaderNodeBsdfPrincipled")
    image_node = nodes.new(type="ShaderNodeTexImage")
    image_node.image = bpy.data.images.load(str(texture_path))
    links.new(image_node.outputs["Color"], principled.inputs["Base Color"])
    links.new(principled.outputs["BSDF"], output.inputs["Surface"])

    if obj.data.materials:
        obj.data.materials[0] = material
    else:
        obj.data.materials.append(material)


def _frame_camera(obj: bpy.types.Object, azimuth_deg: float, elevation_deg: float) -> None:
    bbox = [obj.matrix_world @ Vector(corner) for corner in obj.bound_box]
    centroid = sum(bbox, Vector()) / 8.0
    dimensions = obj.dimensions
    radius = max(dimensions.x, dimensions.y, dimensions.z) * 0.75
    distance = max(radius * 2.8, 1.0)

    az = math.radians(azimuth_deg)
    el = math.radians(elevation_deg)
    direction = Vector(
        (
            math.cos(el) * math.sin(az),
            math.sin(el),
            math.cos(el) * math.cos(az),
        )
    )
    camera_obj = bpy.data.objects.new("PreviewCamera", bpy.data.cameras.new("PreviewCamera"))
    bpy.context.collection.objects.link(camera_obj)
    camera_obj.location = centroid + direction * distance

    direction_to_target = centroid - camera_obj.location
    camera_obj.rotation_euler = direction_to_target.to_track_quat("-Z", "Y").to_euler()
    bpy.context.scene.camera = camera_obj


def _configure_render(output_path: Path, width: int, height: int) -> None:
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = width
    scene.render.resolution_y = height
    scene.render.film_transparent = True
    scene.render.image_settings.file_format = "PNG"
    scene.render.image_settings.color_mode = "RGBA"
    scene.render.filepath = str(output_path)


def main() -> None:
    params = _parse_args()
    _clear_scene()
    obj = _import_fbx(params["fbx"])
    _apply_transform(obj, float(params["scale"]), params["offset"])
    _assign_texture(obj, params["texture"])
    _frame_camera(obj, float(params["azimuth"]), float(params["elevation"]))
    _configure_render(params["output"], int(params["width"]), int(params["height"]))
    bpy.ops.render.render(write_still=True)


if __name__ == "__main__":
    main()
