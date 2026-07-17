# SPDX-License-Identifier: Unlicense or CC0
extends Control
# Scene picker for the fennecs Godot demos. The demo buttons are generated
# from DemoRegistry, so adding a demo there automatically lists it here.
# Hovering or focusing a button shows the demo's explanation on the right.

const DemoRegistry := preload("res://zzzShared/Navigation/demo_registry.gd")

@onready var _buttons: VBoxContainer = %DemoButtons
@onready var _description: RichTextLabel = %Description


func _ready() -> void:
	var first: Button = null
	for demo in DemoRegistry.DEMOS:
		var button := _add_button("%s   %s" % [demo.key_label, demo.title])
		button.pressed.connect(_launch_demo.bind(demo))
		button.mouse_entered.connect(_show_blurb.bind(demo))
		button.focus_entered.connect(_show_blurb.bind(demo))
		if first == null:
			first = button

	if not OS.has_feature("web"):
		var quit := _add_button("Esc  Quit to Desktop")
		quit.pressed.connect(get_tree().quit)
		quit.modulate = Color(1, 1, 1, 0.6)

	if first != null:
		first.grab_focus()
		_show_blurb(DemoRegistry.DEMOS[0])


func _unhandled_input(event: InputEvent) -> void:
	# The DemoNavigator autoload handles F1..Fn; Esc on the menu itself quits.
	var key := event as InputEventKey
	if key != null and key.pressed and key.keycode == KEY_ESCAPE and not OS.has_feature("web"):
		get_viewport().set_input_as_handled()
		get_tree().quit()


func _add_button(label: String) -> Button:
	var button := Button.new()
	button.text = label
	button.custom_minimum_size = Vector2(380, 56)
	button.alignment = HORIZONTAL_ALIGNMENT_LEFT
	button.add_theme_font_size_override("font_size", 26)
	_buttons.add_child(button)
	return button


func _launch_demo(demo: Dictionary) -> void:
	get_node("/root/DemoNavigator").goto_scene(demo.scene)


func _show_blurb(demo: Dictionary) -> void:
	_description.text = "[font_size=28]%s[/font_size]\n\n%s" % [demo.title, demo.blurb]
