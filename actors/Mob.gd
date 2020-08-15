extends KinematicBody2D

onready var world : WorldManager = find_parent("WorldManager")
var targets := []

func _ready() -> void:
	pass

func _draw() -> void:
	if !world.DEBUG: return
	for target in targets:
		if target[2].empty():
			draw_line(target[0] - position, target[1] - position, Color.green)
		else:
			draw_line(target[0] - position, target[2].position - position, Color.red)
			
func _process(delta: float) -> void:
	pass

func _physics_process(delta: float) -> void:
	targets.clear()
	for light in world.lights:
		var offset := Vector2(0, 8).rotated(get_angle_to(light.global_position))
		_check_it(position, light.global_position)
		_check_it(position + offset, light.global_position)
		_check_it(position - offset, light.global_position)
	update()
	pass

func _check_it(source: Vector2, dest: Vector2) -> void:
	var space_state = get_world_2d().direct_space_state	
	var target = space_state.intersect_ray(source, dest)
	targets.append([source, dest, target])
