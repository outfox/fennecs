extends Node2D

@export var spacing := 400.0

func _enter_tree() -> void:
	var children := get_children()
	var dimension : int = ceil(sqrt(len(children)))
	for i in range(len(children)):
		var x := i % dimension - dimension * 0.5
		var y := i / dimension - dimension * 0.5
		var child := children[i] as Node2D
		child.position = Vector2(x * spacing, y * spacing)
