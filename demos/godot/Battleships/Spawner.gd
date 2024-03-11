extends Timer

@export var prefab : PackedScene


func _ready() -> void:
	for j in range(500):
		await timeout
		var ship = prefab.instantiate() as Node2D
		ship.rotate(randf() * TAU)
		ship.name = str(j)
		add_sibling(ship)
