extends Timer

@export var prefab : PackedScene


func _ready() -> void:
	for j in range(500):
		await timeout
		var ship = prefab.instantiate() as Node2D
		add_sibling(ship)
		ship.rotate(randf() * TAU)
		ship.name = str(j)
