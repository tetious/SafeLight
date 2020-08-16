extends KinematicBody2D

export var speed := 100
var destination := Vector2.ZERO
var path := PoolVector2Array()

onready var world : WorldManager = find_parent("WorldManager")
var targets := []

func _ready() -> void:
	pass

func _draw() -> void:
	if !world.DEBUG: return

	draw_circle(destination - position, 2, Color.blue)
	
	for target in targets:
		if target[2].empty():
			draw_line(target[0] - position, target[1] - position, Color.green)
		else:
			draw_line(target[0] - position, target[2].position - position, Color.red)
			
func move_along_path(distance: float) -> void:
	var start := position
	while !path.empty():
		var to_next := start.distance_to(path[0])
		move_and_slide(position.direction_to(path[0]) * distance)

		if (path.size() > 1 or distance <= to_next) and to_next > 16:
			break

		distance -= to_next
		start = path[0]
		path.remove(0)

func _physics_process(delta: float) -> void:
	
	targets.clear()
	for light in world.lights:
		var offset := Vector2(0, 8).rotated(get_angle_to(light.global_position))
		_check_it(position, light.global_position)
		_check_it(position + offset, light.global_position)
		_check_it(position - offset, light.global_position)
		
	var target := Vector2.ZERO
	for candidate in targets:
		if candidate[2].empty() and candidate[1].distance_to(position) < target.distance_to(position):
			target = candidate[1]
	
	if target != Vector2.ZERO:
		destination = target
	
	if path.empty():
		if destination != Vector2.ZERO and destination.distance_to(position) > 16:
			move_and_slide(position.direction_to(destination) * speed)
	else:
		move_along_path(speed)
	
	update()
	pass

func _check_it(source: Vector2, dest: Vector2) -> void:
	var space_state = get_world_2d().direct_space_state	
	var target = space_state.intersect_ray(source, dest, [self])
	targets.append([source, dest, target])
