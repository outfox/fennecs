extends Camera2D

@onready var zoom_goal := zoom
@onready var position_goal := position

@onready var parent : Node2D = $".."

var last_mouse : Vector2

func zoom_and_pan() -> void:
	var current_mouse := get_local_mouse_position()
	if Input.is_action_pressed("look>pan"):
		position_goal += (last_mouse - current_mouse)

	if Input.is_action_just_pressed("look>zoom+"):
		zoom_goal *= 1.1111111
	zoom_goal = Vector2(clampf(zoom_goal.x, 0.1, 5), clampf(zoom_goal.y, 0.1, 5))


	if Input.is_action_just_pressed("look>zoom-"):
		zoom_goal *= 0.9
		zoom_goal = Vector2(clampf(zoom_goal.x, 0.1, 5), clampf(zoom_goal.y, 0.1, 5))

	last_mouse = current_mouse



func _process(delta: float) -> void:
	zoom_and_pan()

	var k := pow(0.9, 120.0*delta)

	var view_mouse = get_viewport().get_mouse_position()
	view_mouse -= get_viewport_rect().size * 0.5

	var mouse_pre_zoom := to_local(get_canvas_transform().affine_inverse().basis_xform(view_mouse))
	zoom = zoom * k + (1.0-k) * zoom_goal
	var mouse_post_zoom := to_local(get_canvas_transform().affine_inverse().basis_xform(view_mouse))

	var zoom_position_offset := (mouse_pre_zoom - mouse_post_zoom)

	position_goal += zoom_position_offset
	position = position * k + (1.0-k) * position_goal + zoom_position_offset
