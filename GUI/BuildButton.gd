extends Button

export var wallIndex := -1
export var propName := ""

onready var world := find_parent("WorldManager")

func _toggled(button_pressed: bool) -> void:
	world.BuildMode = button_pressed
	world.BuildWallIndex = wallIndex
	world.BuildPropName = propName
