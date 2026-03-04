extends Node
class_name HackingSystem

signal hacking_started(target)
signal hacking_completed(target, success)

@export var hack_time: float = 3.0
var is_hacking: bool = false
var current_target = null

func start_hack(target):
	if is_hacking: return
	
	current_target = target
	is_hacking = true
	hacking_started.emit(target)
	
	await get_tree().create_timer(hack_time).timeout
	
	complete_hack(true)

func complete_hack(success: bool):
	if not is_hacking: return
	
	is_hacking = false
	hacking_completed.emit(current_target, success)
	
	if success and current_target.has_method("on_hacked"):
		current_target.on_hacked()
	
	current_target = null

func cancel_hack():
	is_hacking = false
	current_target = null
