extends Node2D
class_name WorldManager

export var DEBUG := false

# Declare member variables here. Examples:
# var a = 2
# var b = "text"
#onready var map := $Map/Navigation2D/TileMap
onready var camera := $Camera
onready var nav := $Map/Navigation2D

onready var lights := get_tree().get_nodes_in_group("light")

func _input(event):
	if event is InputEventMouseButton:
		if event.button_index == BUTTON_LEFT and event.pressed:
			#print("Left button was clicked at ", map.world_to_map(event.position))
			var nav_to := get_global_mouse_position()
			$Bot.path = nav.get_simple_path($Bot.position, nav_to)
			#$Bot2.path = nav.get_simple_path($Bot2.position, nav_to + Vector2(16, 16))
			
			
			
			
