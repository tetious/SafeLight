extends Node2D

onready var astar = AStar2D.new()
onready var map = $TileMap

# Declare member variables here. Examples:
# var a: int = 2
# var b: String = "text"


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


func add_tiles():
	for tile in map.get_used_cells():
		astar.add_point(tile_id(tile), tile)
		
func tile_id(tile):
	print(tile.x + tile.y)
	return tile.x + tile.y
