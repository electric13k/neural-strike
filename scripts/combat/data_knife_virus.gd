extends Node3D
class_name DataKnifeVirus

# Data Knife Virus System
# Hack enemy bots by holding interaction for 7-10s (random)

@export var hack_duration_min: float = 7.0
@export var hack_duration_max: float = 10.0
@export var interrupt_on_damage: bool = true
@export var max_hack_distance: float = 2.0

var current_target: Node3D = null
var hack_progress: float = 0.0
var hack_time_required: float = 0.0
var is_hacking: bool = false
var hacker_player: Node3D = null

signal hack_started(target)
signal hack_progress_updated(progress_percent)
signal hack_completed(target)
signal hack_interrupted(reason)

func start_hack(target: Node3D, player: Node3D) -> bool:
	if is_hacking:
		return false
	
	# Check if target is a hackable bot
	if not target.has_method("is_bot") or not target.is_bot():
		print("Target is not a hackable bot")
		return false
	
	# Check if bot belongs to enemy team
	if target.has_method("get_owner_id") and target.get_owner_id() == player.get_peer_id():
		print("Cannot hack your own bot")
		return false
	
	current_target = target
	hacker_player = player
	hack_progress = 0.0
	hack_time_required = randf_range(hack_duration_min, hack_duration_max)
	is_hacking = true
	
	print("Hack started on %s (%.1fs required)" % [target.name, hack_time_required])
	hack_started.emit(target)
	return true

func _process(delta):
	if not is_hacking or not current_target or not hacker_player:
		return
	
	# Check if player is still close enough
	var distance = hacker_player.global_position.distance_to(current_target.global_position)
	if distance > max_hack_distance:
		interrupt_hack("Player moved too far (%.1fm)" % distance)
		return
	
	# Check if target is still alive
	if current_target.has_method("is_dead") and current_target.is_dead():
		interrupt_hack("Target was destroyed")
		return
	
	# Progress the hack
	hack_progress += delta
	var progress_percent = (hack_progress / hack_time_required) * 100.0
	hack_progress_updated.emit(progress_percent)
	
	# Check completion
	if hack_progress >= hack_time_required:
		complete_hack()

func complete_hack():
	if not current_target:
		return
	
	print("Hack completed on %s!" % current_target.name)
	
	# Transfer bot ownership to hacker
	if current_target.has_method("transfer_ownership"):
		current_target.transfer_ownership(hacker_player.get_peer_id())
	
	# Visual/audio feedback
	if current_target.has_method("play_hacked_effect"):
		current_target.play_hacked_effect()
	
	hack_completed.emit(current_target)
	reset_hack()

func interrupt_hack(reason: String):
	print("Hack interrupted: %s" % reason)
	hack_interrupted.emit(reason)
	reset_hack()

func reset_hack():
	is_hacking = false
	current_target = null
	hacker_player = null
	hack_progress = 0.0

func on_hacker_damaged():
	"""Call this when the hacker takes damage"""
	if interrupt_on_damage and is_hacking:
		interrupt_hack("Hacker took damage")

func get_progress_percent() -> float:
	if hack_time_required <= 0:
		return 0.0
	return (hack_progress / hack_time_required) * 100.0

func get_time_remaining() -> float:
	return max(0.0, hack_time_required - hack_progress)
