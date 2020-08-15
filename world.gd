extends Node2D


# Declare member variables here. Examples:
# var a = 2
# var b = "text"
onready var map := $Map/Navigation2D/TileMap
onready var camera := $Camera
onready var bot := $Bot
onready var nav := $Map/Navigation2D

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.

func _input(event):
	if event is InputEventMouseButton:
		if event.button_index == BUTTON_LEFT and event.pressed:
			#print("Left button was clicked at ", map.world_to_map(event.position))
			var nav_to := get_global_mouse_position()
			bot.path = nav.get_simple_path(bot.position, nav_to)
			
			
			
			


# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass
