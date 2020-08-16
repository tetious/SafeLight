extends Light2D
class_name SafeLight

onready var world := find_parent("WorldManager")

onready var light_distance : float = texture_scale * 48
onready var light_size := Vector2(light_distance, light_distance)


func get_light_rect() -> Rect2:
	return Rect2(global_position - light_size / 2, light_size)

func _draw() -> void:
	if !world.DEBUG: return
	draw_set_transform(Vector2(0,0) - global_position, 0, Vector2.ONE)
	draw_rect(get_light_rect(), Color.yellow, false)
