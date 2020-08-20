extends Node2D

export var eu_tick = 10

func _ready() -> void:
	# Can't use the global name here for some reason :(
	get_node("/root/WorldState").connect("WorldTick", self, "_on_tick")

func _on_tick() -> void:
	WorldState.AddPower(eu_tick)
