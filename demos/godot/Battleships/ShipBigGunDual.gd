extends Sprite2D


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	look_at(get_global_mouse_position())
