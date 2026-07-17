# SPDX-License-Identifier: Unlicense or CC0
extends RefCounted
# DemoRegistry: single source of truth for the scenes of this demo project.
# Both the Main Menu and the DemoNavigator autoload build their
# buttons and shortcuts from this list - to add a demo, add an
# entry here and it shows up everywhere.

const MAIN_MENU := "res://zzzShared/Menu/main_menu.tscn"
const MENU_KEY := KEY_F1
const MENU_KEY_LABEL := "F1"

const DEMOS := [
	{
		"title": "Lots of Cubes",
		"scene": "res://Cubes/Demo.tscn",
		"key": KEY_F2,
		"key_label": "F2",
		"blurb": "Hundreds of thousands of cubes, all simulated on the CPU - no GPU compute involved.

Each cube is an Entity with a Position (Vector3), a custom Matrix4x3 Transform, and an integer identifier. A Query updates them every frame, and Query.Raw submits the Matrix4x3 memory directly to a MultiMeshInstance3D as a PackedFloat32Array - bulk data transfer from fennecs straight into Godot's Forward+ renderer.

Use the sliders to choose how many Entities are simulated and how many are rendered.",
	},
	{
		"title": "N-Body Problem",
		"scene": "res://N-Body/Demo.tscn",
		"key": KEY_F3,
		"key_label": "F3",
		"blurb": "The chaotic N-Body problem, powered by fennecs Entity-Entity relations.

Each colored cluster is a self-contained clique of 2 to 5 stars that all attract each other; every clique forms its own Archetype. A single Query computes the attraction for each pair of related stars - every Entity is processed exactly once per partner - and two more Queries integrate the forces into velocity and position.

Pan with the right or middle mouse button, zoom with the mouse wheel.",
	},
	{
		"title": "Battleships",
		"scene": "res://Battleships/Demo.tscn",
		"key": KEY_F4,
		"key_label": "F4",
		"blurb": "A naval free-for-all: factions of ships steer toward their fleet objectives, aim their guns, and open fire.

Ships, guns, and bullets are Entities that mix plain C# structs with Godot nodes stored directly as Components - ECS simulation and the classic scene tree working side by side. Targeting is a brute-force pass over all live ships each frame, kept fast by fennecs' tight iteration.

Pan with the right or middle mouse button, zoom with the mouse wheel.",
	},
]


static func demo_for_key(keycode: Key) -> Dictionary:
	for demo in DEMOS:
		if demo.key == keycode:
			return demo
	return {}
