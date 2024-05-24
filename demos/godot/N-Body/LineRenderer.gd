extends Line2D

var max_points : int = 250

func _physics_process(delta: float) -> void:
	while get_point_count() > max_points:
		remove_point(0)
	add_point(get_parent().global_position)
