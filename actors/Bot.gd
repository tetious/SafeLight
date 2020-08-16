extends KinematicBody2D

onready var world := find_parent("WorldManager")

export var speed := 400.0
var path := PoolVector2Array() setget set_path


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	set_process(false)

func _process(delta: float) -> void:
	move_along_path(speed * delta)

func move_along_path(distance: float) -> void:
	var start := position
	for i in range(path.size()):
		var to_next := start.distance_to(path[0])
		if distance <= to_next and distance >= 0:
			position = start.linear_interpolate(path[0], distance / to_next)
			break
		elif distance < 0:
			position = path[0]
			set_process(false)
		distance -= to_next
		start = path[0]
		path.remove(0)
		

func set_path(value: PoolVector2Array) -> void:
	path = value
	if value.size() == 0: return
	set_process(true)
