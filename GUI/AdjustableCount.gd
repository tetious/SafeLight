extends HBoxContainer

export var value := 0 setget set_value

signal value_changed

func _ready() -> void:
	$Label.text = name

func set_value(val : int) -> void:
	value = val
	$Value.text = str(value)

func _on_PlusButton_pressed() -> void:
	self.value = WorldState.call("set_" + name.to_lower(), value + 1)

func _on_MinusButton_pressed() -> void:
	self.value = WorldState.call("set_" + name.to_lower(), value - 1)
