# SPDX-License-Identifier: Unlicense or CC0
extends Node2D

# Smooth panning and precise zooming for Camera2D
# Usage: This script may be placed on a child node
# of a Camera2D or on a Camera2D itself.
# Suggestion: Change and/or set up the three Input Actions,
# otherwise the mouse will fall back to hard-wired mouse
# buttons and you will miss out on alternative bindings,
# deadzones, and other nice things from the project InputMap.
class_name CameraZoomAndPan

@onready var camera : Camera2D = $".." if ($".." is Camera2D) else self

#region exported Parameters
@export_range(100, 10000, 100) var maxPanDistance : float = 10000
@export_range(1, 20, 0.01) var maxZoom : float = 5.0
@export_range(0.01, 1, 0.01) var minZoom : float = 0.1
@export_range(0.01, 0.2, 0.01) var zoomStepRatio : float = 0.1

@export_group("Actions")
@export var panAction : String = "camera>pan"
@export var zoomInAction : String = "camera>zoom+"
@export var zoomOutAction : String = "camera>zoom-"


@export_group("Mouse")
@export var zoomToCursor: bool = true
@export_enum("Auto", "Always", "Never") var useFallbackButtons: String = "Auto"
@export var panButton : MouseButton = MOUSE_BUTTON_MIDDLE
@export var zoomInButton : MouseButton = MOUSE_BUTTON_WHEEL_UP
@export var zoomOutButton : MouseButton = MOUSE_BUTTON_WHEEL_DOWN

@export_group("Smoothing")
@export_range(0, 0.4, 0.01) var panSmoothing : float = 0.2
@export_range(0, 0.4, 0.01) var zoomSmoothing : float = 0.2
#endregion


#region State Initialization
@onready var zoom_goal := camera.zoom
@onready var position_goal := camera.position

var fallback_mouse_pan : bool
var fallback_mouse_zoom_in : bool
var fallback_mouse_zoom_out : bool
var last_mouse : Vector2
var zoom_mouse : Vector2

@onready var damped_pan: Array[Vector2] = [camera.position, Vector2.ZERO]
@onready var damped_zoom: Array[Vector2] = [camera.zoom, Vector2.ZERO]


func _ready() -> void:
	# If the actions aren't defined and mouse fallback is enabled,
	# use the default mouse buttons
	var actions = InputMap.get_actions()
	var always = useFallbackButtons == "Always"
	var never = useFallbackButtons == "Never"
	fallback_mouse_pan = not never and (always or (panAction not in actions))
	fallback_mouse_zoom_in = not never and (always or (zoomInAction not in actions))
	fallback_mouse_zoom_out = not never and (always or (zoomOutAction not in actions))

	if not always and (fallback_mouse_pan or fallback_mouse_zoom_in or fallback_mouse_zoom_out):
		prints("CameraZoomAndPan: Mouse Fallbacks for Actions in effect!",
			panAction + "=" + str(fallback_mouse_pan),
			zoomInAction + "=" + str(fallback_mouse_zoom_in),
			zoomOutAction + "=" + str(fallback_mouse_zoom_out))
		printt("CameraZoomAndPan: TIP - set up all three of the following InputActions:",
			panAction,
			zoomInAction,
			zoomOutAction)
#endregion


func _process(delta: float) -> void:
	_SmoothDamp(damped_zoom, zoom_goal, zoomSmoothing, delta)

	# Zoom in and determine camera offset to keep
	# the view under the mouse cursor
	var mouse_pre_zoom := to_local(get_canvas_transform().affine_inverse().basis_xform(zoom_mouse))
	camera.zoom = damped_zoom[0]
	var mouse_post_zoom := to_local(get_canvas_transform().affine_inverse().basis_xform(zoom_mouse))

	var zoom_position_offset := (mouse_pre_zoom - mouse_post_zoom) if zoomToCursor else Vector2.ZERO

	position_goal += zoom_position_offset
	damped_pan[0] += zoom_position_offset


	_SmoothDamp(damped_pan, position_goal, panSmoothing, delta)
	camera.position = damped_pan[0]




func _unhandled_input(event: InputEvent) -> void:
	if not event is InputEventMouse and not event is InputEventAction:
		return

	var current_mouse := get_local_mouse_position()

	if Input.is_action_pressed(panAction) or (fallback_mouse_pan and Input.is_mouse_button_pressed(panButton)):
		position_goal += (last_mouse - current_mouse)
		position_goal = position_goal.clamp(-Vector2(maxPanDistance,maxPanDistance), Vector2(maxPanDistance,maxPanDistance))


	if Input.is_action_just_pressed(zoomInAction) or (fallback_mouse_zoom_in and Input.is_mouse_button_pressed(zoomInButton)):
		zoom_goal *= 1.0 / (1.0-zoomStepRatio)
		zoom_mouse = get_viewport().get_mouse_position()
		zoom_mouse -= get_viewport_rect().size * 0.5

	if Input.is_action_just_pressed(zoomOutAction) or (fallback_mouse_zoom_out and Input.is_mouse_button_pressed(zoomOutButton)):
		zoom_goal *= (1.0-zoomStepRatio)
		zoom_mouse = get_viewport().get_mouse_position()
		zoom_mouse -= get_viewport_rect().size * 0.5

	zoom_goal = zoom_goal.clamp(minZoom * Vector2.ONE, maxZoom * Vector2.ONE)
	last_mouse = current_mouse




func _SmoothDamp(state: Array[Vector2], target : Vector2, smoothTime : float, deltaTime : float):
		# We speed up the spring to allow for nicer input values
		# and a behaviour closer to the "actual" time to come to rest
		smoothTime /= 2.0

		var current := state[0]
		var linear_velocity := state[1]

		if smoothTime == 0:
			state[0] = target
			state[1] = Vector2.ZERO
			return

		var omega := 2.0 / smoothTime

		var x := omega * deltaTime;
		var expo := 1.0 / (1.0 + x + 0.48 * x * x + 0.235 * x * x * x);

		var change := current - target;
		var originalTo := target;

		# Optional: Clamp maxSpeed
		# var maxChange = maxSpeed * smoothTime;
		# change = clamp(change, -maxChange, maxChange);
		target = current - change;

		var temp := (linear_velocity + omega * change) * deltaTime
		linear_velocity = (linear_velocity - omega * temp) * expo
		var output := target + (change + temp) * expo

		# Prevent overshooting - FIXME
		# likely needs to treat all components separately
		if (originalTo.x > current.x) == (output.x > originalTo.x):
			output.x = originalTo.x
			linear_velocity.x = (output.x - originalTo.x) / deltaTime
		if (originalTo.y > current.y) == (output.y > originalTo.y):
			output.y = originalTo.y
			linear_velocity.y = (output.y - originalTo.y) / deltaTime

		state[0] = output
		state[1] = linear_velocity
