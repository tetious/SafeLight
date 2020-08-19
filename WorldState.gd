extends Node

var bot_count := 0 setget _set_bot_count
var gatherers := 0 setget set_gatherers
var stored_power := 0
var max_stored_power := 100


var paused := false setget _set_paused

onready var tick_timer := Timer.new()

signal world_tick
signal state_changed

func _ready() -> void:
	_start_ticktimer()
	pass

func _on_tick_timer() -> void:
	emit_signal("world_tick")

func _start_ticktimer() -> void:
	tick_timer.process_mode = Timer.TIMER_PROCESS_PHYSICS
	tick_timer.connect("timeout", self, "_on_tick_timer")
	add_child(tick_timer)
	tick_timer.start(1)

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

func add_power(val):
	stored_power += val
	if stored_power > max_stored_power: stored_power = max_stored_power
	emit_signal("state_changed")

func add_max_power(val):
	max_stored_power += val
	emit_signal("state_changed")

func idle_bots():
	return bot_count - gatherers
	
func _set_paused(val : bool):
	paused = val
	if paused:
		tick_timer.stop()
	else:
		tick_timer.start()
	
