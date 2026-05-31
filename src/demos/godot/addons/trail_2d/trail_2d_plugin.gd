@tool
extends EditorPlugin


func _enter_tree() -> void:
	add_custom_type('Trail2D', 'Line2D', preload('res://addons/trail_2d/trail_2d.gd'), preload('res://addons/trail_2d/icon.svg'))


func _exit_tree() -> void:
	# Clean-up of the plugin goes here.
	remove_custom_type('Trail2D')
