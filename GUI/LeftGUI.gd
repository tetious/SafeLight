extends MarginContainer

onready var world := find_parent("WorldManager")

func _ready() -> void:
	WorldState.connect("state_changed", self, "_state_changed")

func _state_changed():
	$VBoxContainer/HBoxContainer/TotalVal.text = "%d/%d" % [WorldState.idle_bots(), WorldState.bot_count] 
