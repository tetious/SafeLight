extends MarginContainer

onready var world := find_parent("WorldManager")

onready var power_value := find_node("PowerValue")
onready var crystal_value := find_node("CrystalValue")

func _ready() -> void:
	WorldState.connect("StateChanged", self, "_state_changed")
	_state_changed()
	pass

func _state_changed():
	power_value.text = "%d/%d" % [WorldState.StoredPower, WorldState.MaxPower]
	crystal_value.text = str(10)
