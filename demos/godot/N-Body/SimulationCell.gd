extends Node2D

var children : Array[StellarBody]

var cut : int

func _ready() -> void:
	scale = Vector2.ONE * (randf() + 0.5)
	modulate = Color.from_ok_hsl(randf(), 0.8, 0.5)

	cut = round(randf() * 3)

	for child in get_children():
		if child is StellarBody:
			if cut > 0:
				cut -= 1
				#child.Despawn()
