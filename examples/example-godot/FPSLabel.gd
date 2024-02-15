extends Label

var smoothed : float = 0.016
@onready var ECS : MultiMeshExample = %ECS

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if (delta > 0):
		smoothed = smoothed * 0.95 + 0.05 * delta
		var fps := roundi(1.0 / smoothed)
		self.text = "%d fps" % fps + "\n" + "%d entities" % ECS.InstanceCount
