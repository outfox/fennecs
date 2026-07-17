# SPDX-License-Identifier: Unlicense or CC0
extends CanvasLayer
# Autoload singleton (registered as "DemoNavigator" in the Project Settings).
# Draws a slim menu bar along the top edge of the screen and provides global
# keyboard shortcuts to hop between scenes:
#   F1 or Esc ... Main Menu
#   F2, F3, ... the demos, as declared in the DemoRegistry

const DemoRegistry := preload("res://zzzShared/Navigation/demo_registry.gd")

const BAR_HEIGHT := 40.0

var _buttons := {} # scene path (String) -> Button
var _tab_group := ButtonGroup.new() # radio behavior: exactly one tab active


func _ready() -> void:
	layer = 100
	_build_bar()
	_highlight_scene.call_deferred("")


func _unhandled_input(event: InputEvent) -> void:
	var key := event as InputEventKey
	if key == null or not key.pressed or key.echo:
		return

	if key.keycode == DemoRegistry.MENU_KEY or key.keycode == KEY_ESCAPE:
		goto_scene(DemoRegistry.MAIN_MENU)
		get_viewport().set_input_as_handled()
		return

	var demo := DemoRegistry.demo_for_key(key.keycode)
	if not demo.is_empty():
		goto_scene(demo.scene)
		get_viewport().set_input_as_handled()


## Switches to the given scene (no-op when it is already running).
func goto_scene(path: String) -> void:
	var current := get_tree().current_scene
	if current != null and current.scene_file_path == path:
		return
	get_tree().change_scene_to_file(path)
	_highlight_scene(path)


func _build_bar() -> void:
	var panel := PanelContainer.new()
	panel.name = "Menu Bar"
	panel.theme = preload("res://zzzShared/fennecs_theme.tres")
	panel.set_anchors_and_offsets_preset(Control.PRESET_TOP_WIDE)
	panel.custom_minimum_size = Vector2(0, BAR_HEIGHT)

	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.02, 0.08, 0.11, 0.75)
	style.border_width_bottom = 1
	style.border_color = Color(1, 1, 1, 0.1)
	panel.add_theme_stylebox_override("panel", style)
	add_child(panel)

	var row := HBoxContainer.new()
	row.alignment = BoxContainer.ALIGNMENT_CENTER
	row.add_theme_constant_override("separation", 32)
	panel.add_child(row)

	_add_button(row, DemoRegistry.MENU_KEY_LABEL + "  Menu", DemoRegistry.MAIN_MENU)
	for demo in DemoRegistry.DEMOS:
		_add_button(row, demo.key_label + "  " + demo.title, demo.scene)


func _add_button(row: Control, label: String, scene: String) -> void:
	var button := Button.new()
	button.text = label
	button.flat = true
	button.focus_mode = Control.FOCUS_NONE
	button.toggle_mode = true
	button.button_group = _tab_group
	button.pressed.connect(goto_scene.bind(scene))
	row.add_child(button)
	_buttons[scene] = button


# Presses the tab of the scene we are in (or headed to) — the theme styles
# the pressed state in fennecs flame orange. Pass "" to detect the
# currently running scene.
func _highlight_scene(path: String) -> void:
	if path.is_empty():
		var current := get_tree().current_scene
		if current != null:
			path = current.scene_file_path
	for scene: String in _buttons:
		_buttons[scene].set_pressed_no_signal(scene == path)
