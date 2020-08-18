extends Node

var bot_count := 0 setget _set_bot_count
var gatherers := 0 setget set_gatherers

signal state_changed

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.

func _set_bot_count(val):
	bot_count = val
	emit_signal("state_changed")

func set_gatherers(val):
	if val < 0: return
	var non_gatherers = idle_bots() + gatherers
	if val <= non_gatherers: 
		gatherers = val
		emit_signal("state_changed")
		
	return gatherers
	
func idle_bots():
	return bot_count - gatherers
