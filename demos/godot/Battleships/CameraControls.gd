extends Camera2D

@onready var zoom_goal := zoom
@onready var position_goal := global_position

var last_mouse : Vector2

func _input(event: InputEvent) -> void:
	if Input.is_action_just_pressed("look>pan"):
		last_mouse = get_global_mouse_position()

	var motion := event as InputEventMouseMotion
	if motion:
		if Input.is_action_pressed("look>pan"):
			position_goal -= get_global_mouse_position() - last_mouse

			pass

	if Input.is_action_just_pressed("look>zoom+"):
		zoom_goal *= 1.1
		zoom_goal = Vector2(clampf(zoom_goal.x, 0.1, 5), clampf(zoom_goal.y, 0.1, 5))

	if Input.is_action_just_pressed("look>zoom-"):
		zoom_goal *= 0.9
		zoom_goal = Vector2(clampf(zoom_goal.x, 0.1, 5), clampf(zoom_goal.y, 0.1, 5))


func _process(delta: float) -> void:
	zoom = zoom * 0.9 + 0.1 * zoom_goal
	global_position = global_position * 0.9 + 0.1 * position_goal
