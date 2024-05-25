extends Node2D

var children : Array[StellarBody]

func _ready() -> void:
	for child in get_children():
		if child is StellarBody:
			children.append(child)


func _physics_process(delta : float) -> void:
	var average := Vector2.ZERO

	for child in children:
		average += child.position

	average /= len(children)

	for child in children:
		child.position -= average
