extends Node2D

var children : Array[StellarBody]


func _ready() -> void:
	scale = Vector2.ONE * (randf() + 0.5)

	var hue := fposmod((global_position.x/1300.0 + global_position.y/700.0), 1.0)
	var color := Color.from_ok_hsl(hue, 0.9, 0.6)

	modulate = color



	#for child in get_children():
		#if child is Sprite2D:
		#	child.modulate = color
