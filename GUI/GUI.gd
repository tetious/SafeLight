extends Node2D


# Declare member variables here. Examples:
# var a: int = 2
# var b: String = "text"
onready var world := find_parent("WorldManager")

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	
	pass # Replace with function body.

func _draw() -> void:
	if world.BuildMode: 
		draw_rect(world.Map.GetTileRect(get_global_mouse_position() - position), Color.purple, false)
	
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	update()
	pass

func _unhandled_input(event: InputEvent) -> void:
	
	pass
