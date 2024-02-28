extends Label

var smoothed : float = 0.016
@onready var ECS : CubeDemo = %CubeDemo
@onready var VisibleSlider : VSlider = %VisibleSlider


func _process(delta):
	if (delta > 0):
		smoothed = smoothed * 0.95 + 0.05 * delta
		var fps := roundi(1.0 / smoothed)
		var fps_text = "%d fps" % fps
		var entities_text = "%d entities" % ECS.QueryCount
		var visible_text = "%3.0f" % (VisibleSlider.value*100) + "% visible"
		self.text = fps_text + '\n' + entities_text + '\n' +  visible_text
