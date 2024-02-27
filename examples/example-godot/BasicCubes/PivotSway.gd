extends Node3D

@onready var tween : Tween

func coroutine():
	while true:
		tween = create_tween()
		tween.set_ease(Tween.EASE_IN_OUT)
		tween.set_trans(Tween.TRANS_QUART)
		tween.parallel().tween_property(self, "global_rotation_degrees",
		Vector3(
			randf_range(-180, 180),
			randf_range(-180, 180),
			randf_range(-180, 180))
		, 7)
		tween.set_ease(Tween.EASE_IN_OUT)
		tween.set_trans(Tween.TRANS_BACK)
		tween.parallel().tween_property($Camera3D, "position",
		Vector3(
			randf_range(-50, 50), randf_range(-30, 30), randf_range(250, 600))
		, 7)
		await tween.finished


# Called when the node enters the scene tree for the first time.
func _ready():
	coroutine()

