extends Node2D


# Declare member variables here. Examples:
# var a: int = 2
# var b: String = "text"

onready var world := find_parent("WorldManager")
# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


func _draw() -> void:
	return
	if world.BuildMode:
		draw_line(world.rayStart + position, world.rayEnd + position, Color.red)

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	return
	update()
