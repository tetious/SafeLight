extends KinematicBody2D

export var speed := 100
export var sight_distance := 400
var sight_size := Vector2(sight_distance, sight_distance)

var destination := Vector2.ZERO
var path := PoolVector2Array()


onready var world : WorldManager = find_parent("WorldManager")
var targets := []

func _ready() -> void:
	pass

func _draw() -> void:
	if !world.DEBUG: return

	draw_set_transform(Vector2(0,0) - global_position, 0, Vector2.ONE)
	draw_circle(destination, 2, Color.blue)
	draw_rect(get_sight_rect(), Color.yellowgreen, false)
		
	for target in targets:
		if target[2].empty():
			draw_line(target[0], target[1], Color.green)
		else:
			draw_line(target[0], target[2].position, Color.red)
			
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
		var can_see_light = get_sight_rect().intersects(light.get_light_rect())
		if !light.enabled or !can_see_light : continue
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
	
func get_sight_rect() -> Rect2:
	return Rect2(global_position - sight_size / 2, sight_size)

func _check_it(source: Vector2, dest: Vector2) -> void:
	var space_state = get_world_2d().direct_space_state	
	var target = space_state.intersect_ray(source, dest, [], 0b1)
	targets.append([source, dest, target])
