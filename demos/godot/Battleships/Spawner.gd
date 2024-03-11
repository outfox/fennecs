extends Timer

@export var prefab : PackedScene


func _ready() -> void:
	for i in range(50):
		await timeout
		for j in range(10):
			var ship = prefab.instantiate() as Node2D
			ship.rotate(randf() * TAU)
			ship.name = str(j)
			add_sibling(ship)
