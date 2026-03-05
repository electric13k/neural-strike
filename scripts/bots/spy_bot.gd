extends CharacterBody3D
class_name SpyBot

# Spy Bot - Pure reconnaissance robot with remote control via Battle Pad
# Player sees through spy bot's camera and issues waypoint commands
# NO COMBAT - stealth and intel only

@export var speed: float = 6.0  # Moderate speed for stealth
@export var sprint_speed: float = 9.0  # Can sprint when needed
@export var acceleration: float = 12.0
@export var max_health: int = 50  # Fragile (lowest HP)

var current_health: int = 50
var owner_player = null  # Reference to owning player
var is_hacked: bool = false
var waypoint_queue: Array = []  # Array of Vector3 positions
var current_waypoint_index: int = 0
var state: String = "idle"  # idle, moving_to_waypoint, cloaked, following
var is_cloaked: bool = false
var cloak_energy: float = 100.0  # 0-100, drains while cloaked
var cloak_drain_rate: float = 10.0  # Per second
var cloak_recharge_rate: float = 5.0  # Per second when uncloaked
var is_being_remote_controlled: bool = false

@onready var navigation_agent = $NavigationAgent3D
@onready var camera = $SpyCamera  # Camera for POV feed to Battle Pad
@onready var cloak_effect = $CloakEffect  # Visual cloaking shader
@onready var detection_area = $DetectionArea3D  # Area3D for detecting enemies
@onready var camera_feed_viewport = $CameraFeedViewport  # Viewport for rendering POV

func _ready():
	current_health = max_health
	navigation_agent.path_desired_distance = 0.5
	navigation_agent.target_desired_distance = 1.0
	
	if cloak_effect:
		cloak_effect.visible = false
	
	# Setup camera for remote viewing
	if camera:
		camera.current = false  # Not active by default
	
	# Connect detection signals
	if detection_area:
		detection_area.body_entered.connect(_on_enemy_detected)

func _physics_process(delta):
	if current_health <= 0:
		return
	
	# Update cloak energy
	_update_cloak(delta)
	
	# Handle remote control vs autonomous behavior
	if is_being_remote_controlled:
		# Player is actively controlling via Battle Pad
		# Bot follows waypoints issued by player
		_process_remote_control(delta)
	else:
		# Autonomous behavior
		match state:
			"idle":
				_idle_behavior(delta)
			"moving_to_waypoint":
				_move_to_waypoint(delta)
			"following":
				_follow_owner(delta)
	
	move_and_slide()
	
	# Send camera feed to owner's Battle Pad
	if owner_player and owner_player.has_method("receive_spy_camera_feed"):
		owner_player.receive_spy_camera_feed(self, camera)

func _update_cloak(delta):
	if is_cloaked:
		cloak_energy -= cloak_drain_rate * delta
		if cloak_energy <= 0:
			cloak_energy = 0
			_toggle_cloak(false)  # Auto-disable when energy depleted
	else:
		cloak_energy = min(100.0, cloak_energy + cloak_recharge_rate * delta)
	
	# Update cloak visual effect intensity
	if cloak_effect:
		cloak_effect.modulate.a = 0.3 if is_cloaked else 0.0

func _process_remote_control(delta):
	# Bot is being controlled remotely via Battle Pad
	if waypoint_queue.is_empty():
		# No waypoints, stay still
		velocity = velocity.lerp(Vector3.ZERO, acceleration * delta)
		return
	
	# Move to current waypoint
	var target_waypoint = waypoint_queue[current_waypoint_index]
	navigation_agent.target_position = target_waypoint
	
	var distance_to_waypoint = global_position.distance_to(target_waypoint)
	
	if distance_to_waypoint < 1.5:
		# Reached waypoint
		current_waypoint_index += 1
		if current_waypoint_index >= waypoint_queue.size():
			# All waypoints reached
			waypoint_queue.clear()
			current_waypoint_index = 0
			_notify_player("Waypoints complete")
		return
	
	var next_position = navigation_agent.get_next_path_position()
	var direction = (next_position - global_position).normalized()
	
	# Use sprint if cloaked (urgent movement)
	var current_speed = sprint_speed if is_cloaked else speed
	velocity = velocity.lerp(direction * current_speed, acceleration * delta)

func _idle_behavior(delta):
	# Stay still, rotate slowly to scan area
	rotate_y(0.5 * delta)
	velocity = velocity.lerp(Vector3.ZERO, acceleration * delta)

func _move_to_waypoint(delta):
	if waypoint_queue.is_empty():
		state = "idle"
		return
	
	var target_waypoint = waypoint_queue[current_waypoint_index]
	navigation_agent.target_position = target_waypoint
	
	var distance = global_position.distance_to(target_waypoint)
	
	if distance < 1.5:
		# Reached waypoint
		current_waypoint_index += 1
		if current_waypoint_index >= waypoint_queue.size():
			waypoint_queue.clear()
			current_waypoint_index = 0
			state = "idle"
		return
	
	var next_position = navigation_agent.get_next_path_position()
	var direction = (next_position - global_position).normalized()
	velocity = velocity.lerp(direction * speed, acceleration * delta)

func _follow_owner(delta):
	if not owner_player or not is_instance_valid(owner_player):
		state = "idle"
		return
	
	var distance_to_owner = global_position.distance_to(owner_player.global_position)
	
	# Stay 10-20m behind owner (stealthy distance)
	if distance_to_owner > 20.0:
		navigation_agent.target_position = owner_player.global_position
		var next_position = navigation_agent.get_next_path_position()
		var direction = (next_position - global_position).normalized()
		velocity = velocity.lerp(direction * speed, acceleration * delta)
	elif distance_to_owner < 10.0:
		# Too close, back away
		var direction = (global_position - owner_player.global_position).normalized()
		velocity = velocity.lerp(direction * speed * 0.5, acceleration * delta)
	else:
		# Good distance
		velocity = velocity.lerp(Vector3.ZERO, acceleration * delta)

func _on_enemy_detected(body):
	if not body.is_in_group("enemy"):
		return
	
	# Notify player about detected enemy
	if owner_player and owner_player.has_method("on_spy_detected_enemy"):
		owner_player.on_spy_detected_enemy(body, global_position)
	
	# Auto-cloak if energy available
	if not is_cloaked and cloak_energy > 20.0:
		_toggle_cloak(true)

func take_damage(amount: int, attacker = null):
	current_health = max(0, current_health - amount)
	
	# Disable cloak when hit
	if is_cloaked:
		_toggle_cloak(false)
	
	if current_health <= 0:
		die()
		return
	
	# Notify player of damage
	_notify_player("Spy Bot taking damage!")

func die():
	print("Spy Bot destroyed")
	
	# Notify player
	if owner_player and owner_player.has_method("on_bot_destroyed"):
		owner_player.on_bot_destroyed(self)
	
	# Disable remote control
	if owner_player and owner_player.has_method("disconnect_spy_camera"):
		owner_player.disconnect_spy_camera()
	
	queue_free()

func hack_by_player(hacker):
	if is_hacked:
		return false
	
	is_hacked = true
	owner_player = hacker
	
	# Change team colors/indicators
	if has_node("TeamIndicator"):
		get_node("TeamIndicator").modulate = hacker.team_color
	
	print("Spy Bot hacked by ", hacker.name)
	return true

# Spy Bot specific functions (Remote Control API)

func enable_remote_control(player):
	"""Called when player opens Battle Pad and selects this spy bot"""
	if owner_player != player:
		return false
	
	is_being_remote_controlled = true
	state = "moving_to_waypoint"
	
	# Activate spy camera
	if camera:
		camera.current = true
	
	print("Remote control enabled for Spy Bot")
	return true

func disable_remote_control():
	"""Called when player closes Battle Pad or switches view"""
	is_being_remote_controlled = false
	state = "following"  # Return to following owner
	
	# Deactivate spy camera
	if camera:
		camera.current = false
	
	print("Remote control disabled for Spy Bot")

func add_waypoint(position: Vector3):
	"""Player clicks on Battle Pad minimap to set waypoint"""
	waypoint_queue.append(position)
	if state != "moving_to_waypoint":
		current_waypoint_index = 0
		state = "moving_to_waypoint"
	
	print("Waypoint added: ", position)

func clear_waypoints():
	"""Clear all pending waypoints"""
	waypoint_queue.clear()
	current_waypoint_index = 0
	state = "idle"
	
	print("Waypoints cleared")

func go_to_location(position: Vector3):
	"""Direct command to go to specific location (clears queue)"""
	clear_waypoints()
	add_waypoint(position)
	
	print("Going to location: ", position)

func return_to_owner():
	"""Command spy bot to return to player"""
	if not owner_player:
		return
	
	clear_waypoints()
	state = "following"
	
	print("Returning to owner")

func _toggle_cloak(enabled: bool):
	"""Enable/disable cloaking"""
	if enabled and cloak_energy < 10.0:
		return  # Not enough energy
	
	is_cloaked = enabled
	
	if cloak_effect:
		cloak_effect.visible = enabled
	
	print("Cloak: ", "ENABLED" if enabled else "DISABLED")

func toggle_cloak():
	"""Player-triggered cloak toggle"""
	_toggle_cloak(not is_cloaked)

func get_camera_feed() -> Camera3D:
	"""Returns camera for Battle Pad display"""
	return camera

func get_cloak_energy() -> float:
	"""Returns current cloak energy percentage"""
	return cloak_energy

func _notify_player(message: String):
	"""Send notification to owning player"""
	if owner_player and owner_player.has_method("show_notification"):
		owner_player.show_notification(message)
	
	print("[Spy Bot] ", message)

func set_owner_player(player):
	owner_player = player

func command_follow():
	state = "following"
	clear_waypoints()

func command_patrol():
	# Spy bots don't patrol, they follow waypoints
	pass

func command_guard():
	state = "idle"
	clear_waypoints()
