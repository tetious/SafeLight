extends MarginContainer

onready var world := find_parent("WorldManager")

func _ready() -> void:
	WorldState.connect("StateChanged", self, "_state_changed")
	_state_changed()
	pass

func _state_changed():
	$VBoxContainer/HBoxContainer/TotalVal.text = "%d/%d" % [WorldState.IdleBots, WorldState.BotCount] 
