extends Node2D
class_name WorldManager

export var DEBUG := false

export var starting_bot_count := 5

#onready var map := $Map/Navigation2D/TileMap
onready var camera := $Camera
onready var nav := $Map/Navigation2D

var lights 

func _ready() -> void:
	WorldState.bot_count = starting_bot_count
	lights = get_tree().get_nodes_in_group("light")

func _process(delta: float) -> void:
	pass

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		if event.button_index == BUTTON_LEFT and event.pressed:
			var nav_to := get_global_mouse_position()
			print("Left button was clicked at ", nav_to)
			#$Bot.path = nav.get_simple_path($Bot.position, nav_to)
			#$Bot2.path = nav.get_simple_path($Bot2.position, nav_to + Vector2(16, 16))
			
			
			
