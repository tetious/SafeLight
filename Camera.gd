extends Camera2D

export var speed = 500
const base_ramp = 0.001
var ramp = base_ramp
var drag_start

func _ready():
	set_process_input(true)
	set_process(true)
	pass

func _input(event):
	if(event.is_action_pressed("camera_pan")):
		drag_start = get_camera_position() + get_viewport().get_mouse_position()
			
func _process(delta):
	var velocity = Vector2(0,0)
	
	if(Input.is_action_pressed("camera_pan_down")): 
		velocity.y = 1
	elif(Input.is_action_pressed("camera_pan_up")):
		velocity.y = -1
	else:
		velocity.y = 0

	if(Input.is_action_pressed("camera_pan_left")): 
		velocity.x = -1
	elif(Input.is_action_pressed("camera_pan_right")):
		velocity.x = 1
	else:
		velocity.x = 0

	if(velocity.length_squared() > 0):
		if(ramp < 1): ramp *= 2
		position = position + velocity * delta * (speed * ramp)
	else:
		if(Input.is_action_pressed("camera_pan")):
			position = (get_viewport().get_mouse_position() - drag_start) * - 1
		ramp = base_ramp
