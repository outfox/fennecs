extends Line2D

@export_category('Trail')
@export var length : = 50
@export var skip : = 1

@onready var parent : Node2D = get_parent()
var offset := Vector2.ZERO
var frame := 0

func _ready() -> void:
	offset = position
	top_level = true
	call_deferred("late_ready")

func late_ready() -> void:
	width = width * parent.scale.x
	modulate = parent.modulate
	material = parent.material

func _physics_process(_delta: float) -> void:
	global_position = Vector2.ZERO

	var point := parent.global_position + offset

	if frame % skip == 0:
		frame = 0
		add_point(point, 0)

		if get_point_count() > length:
			remove_point(get_point_count() - 1)
	else:
		set_point_position(0, point)
	frame += 1
