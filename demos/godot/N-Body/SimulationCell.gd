extends Control

var children : Array[StellarBody]

var aabb := AABB()
var center := Vector2.ZERO

func _ready() -> void:
	for child in get_children():
		if child is StellarBody:
			children.append(child)


func _process(delta : float) -> void:
	var average := Vector2.ZERO
	for child in children:
		average += child.position

	average /= len(children)

	center = average

	for child in children:
		child.position -= average

func _draw() -> void:
	draw_circle(center, 10, Color.RED)
