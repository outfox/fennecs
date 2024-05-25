# SPDX-License-Identifier: MIT

extends Label

var smoothed : float = 0.016
@onready var ECS : DemoCubes = %Demo
@onready var VisibleSlider : VSlider = %VisibleSlider


func _process(delta):
	if (delta > 0):
		smoothed = smoothed * 0.95 + 0.05 * delta
		var fps := roundi(1.0 / smoothed)
		var fps_text = "%d fps" % fps
		var world_text = EntityNode2D.DebugInfo()
		self.text = fps_text + "\n" + world_text
