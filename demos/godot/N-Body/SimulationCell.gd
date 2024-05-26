extends Node2D

var children : Array[StellarBody]

var cut : int

func _ready() -> void:
	scale = Vector2.ONE * (randf() + 0.5)
	var color := Color.from_ok_hsl(randf(), 0.8, 0.6)
	#color.a = 1.0

	cut = round(randf() * 3)

	for child in get_children():
		if child is Sprite2D:
			child.modulate = color
