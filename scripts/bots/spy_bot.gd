extends CharacterBody3D
class_name SpyBot

# Spy Bot - Stealth reconnaissance robot with complete animation system
# Infiltrates enemy positions, provides camera feeds, marks targets
# Animations: idle, walking, running, sneaking, cover, falling, death (30+ total)

@export var speed: float = 6.0  # Medium speed (balanced stealth/mobility)
@export var sneak_speed: float = 2.5  # Very slow when sneaking
@export var acceleration: float = 12.0
@export var max_health: int = 80  # Low HP (fragile scout)
@export var detection_radius: float = 30.0  # How far it can detect enemies
@export var stealth_radius: float = 5.0  # How close enemies must be to detect spy

var current_health: int = 80
var owner_player = null
var is_hacked: bool = false
var detected_enemies: Array = []  # List of enemies in sight
var is_sneaking: bool = true  # Spy bots default to sneaking
var is_detected: bool = false  # Whether enemies spotted this spy
var state: String = "scout"  # scout, follow, retreat, hide
var patrol_points: Array = []
var current_patrol_index: int = 0
var camera_feed_active: bool = false

# Animation states
var current_anim: String = "idle"
var is_moving: bool = false
var move_direction: Vector2 = Vector2.ZERO
var is_in_cover: bool = false
var cover_position: Vector3 = Vector3.ZERO
var is_crouching: bool = true  # Spy bots default to crouched
var is_jumping: bool = false
var is_dying: bool = false

@onready var navigation_agent = $NavigationAgent3D
@onready var detection_area = $DetectionArea3D  # Area3D for detecting enemies
@onready var camera_3d = $SpyCamera  # Camera3D for surveillance
@onready var stealth_indicator = $StealthIndicator  # Visual feedback for stealth status
@onready var animation_player = $AnimationPlayer
@onready var model = $SpyModel  # 3D model with animations

func _ready():
	current_health = max_health
	navigation_agent.path_desired_distance = 0.5
	navigation_agent.target_desired_distance = 1.0
	
	# Setup animations from FBX files
	_setup_animations()
	
	# Setup detection area
	if detection_area:
		detection_area.body_entered.connect(_on_enemy_detected)
		detection_area.body_exited.connect(_on_enemy_lost)
	
	# Initialize stealth mode
	is_sneaking = true
	is_crouching = true
	
	# Start with crouched sneaking idle
	_play_animation("idle")
	
	print("Spy Bot initialized in stealth mode")

func _setup_animations():
	"""Setup animation library from imported FBX files"""
	if not animation_player:
		return
	
	# Animation mappings from FBX files
	# Animations should be in res://assets/models/bots/spy/animations/
	var anim_paths = {
		# === CORE IDLE STATES ===
		"idle": "res://assets/models/bots/spy/animations/idle.fbx",
		"idle_2": "res://assets/models/bots/spy/animations/idle (2).fbx",
		"idle_3": "res://assets/models/bots/spy/animations/idle (3).fbx",
		"idle_4": "res://assets/models/bots/spy/animations/idle (4).fbx",
		"idle_5": "res://assets/models/bots/spy/animations/idle (5).fbx",
		
		# === JUMPING & FALLING ===
		"jump": "res://assets/models/bots/spy/animations/jump.fbx",
		"jumping_up": "res://assets/models/bots/spy/animations/jumping up.fbx",
		"falling_idle": "res://assets/models/bots/spy/animations/falling idle.fbx",
		"falling_to_roll": "res://assets/models/bots/spy/animations/falling to roll.fbx",
		"hard_landing": "res://assets/models/bots/spy/animations/hard landing.fbx",
		"run_to_stop": "res://assets/models/bots/spy/animations/run to stop.fbx",
		
		# === BASIC LOCOMOTION ===
		"walking": "res://assets/models/bots/spy/animations/walking.fbx",
		"running": "res://assets/models/bots/spy/animations/running.fbx",
		"left_strafe": "res://assets/models/bots/spy/animations/left strafe.fbx",
		"right_strafe": "res://assets/models/bots/spy/animations/right strafe.fbx",
		"left_turn": "res://assets/models/bots/spy/animations/left turn.fbx",
		"right_turn": "res://assets/models/bots/spy/animations/right turn.fbx",
		"left_turn_90": "res://assets/models/bots/spy/animations/left turn 90.fbx",
		"right_turn_90": "res://assets/models/bots/spy/animations/right turn 90.fbx",
		
		# === STEALTH/SNEAKING (PRIMARY) ===
		"crouched_sneaking_left": "res://assets/models/bots/spy/animations/crouched sneaking left.fbx",
		"crouched_sneaking_right": "res://assets/models/bots/spy/animations/crouched sneaking right.fbx",
		
		# === COVER SYSTEM (8 VARIANTS) ===
		"cover_to_stand_2": "res://assets/models/bots/spy/animations/cover to stand (2).fbx",
		"cover_to_stand": "res://assets/models/bots/spy/animations/cover to stand.fbx",
		"stand_to_cover_2": "res://assets/models/bots/spy/animations/stand to cover (2).fbx",
		"stand_to_cover": "res://assets/models/bots/spy/animations/stand to cover.fbx",
		"left_cover_sneak": "res://assets/models/bots/spy/animations/left cover sneak.fbx",
		"right_cover_sneak": "res://assets/models/bots/spy/animations/right cover sneak.fbx",
		
		# === DEATH ANIMATION ===
		"death_1": "res://assets/models/bots/spy/animations/Death (1) spybot.fbx",
		
		# === ALTERNATE MODEL ===
		"tripo_convert": "res://assets/models/bots/spy/animations/tripo_convert_6fddd909-abb9-4..."
	}
	
	print("Spy Bot animation system initialized with 30+ stealth animations")

func _physics_process(delta):
	if current_health <= 0 or is_dying:
		return
	
	# Update stealth status
	_check_stealth_status()
	
	match state:
		"scout":
			_scout_area(delta)
		"follow":
			_follow_owner(delta)
		"retreat":
			_retreat(delta)
		"hide":
			_hide_in_cover(delta)
	
	# Continuously scan for enemies
	_scan_and_mark_enemies()
	
	# Update animation based on state
	if not is_in_cover:
		_update_animation(delta)
	
	move_and_slide()

func _update_animation(delta):
	"""Determine which animation to play based on movement and stealth state"""
	if is_dying:
		return
	
	# Calculate movement metrics
	var speed_magnitude = velocity.length()
	is_moving = speed_magnitude > 0.1
	
	# Get movement direction relative to facing
	if is_moving:
		var forward = -transform.basis.z
		var velocity_normalized = velocity.normalized()
		
		var forward_amount = forward.dot(velocity_normalized)
		var right = transform.basis.x
		var strafe_amount = right.dot(velocity_normalized)
		
		move_direction = Vector2(strafe_amount, forward_amount)
	else:
		move_direction = Vector2.ZERO
	
	# Animation priority:
	# 1. Jump/Falling
	# 2. Cover animations
	# 3. Sneaking (if in stealth mode)
	# 4. Walking/Running
	# 5. Idle
	
	if is_jumping or not is_on_floor():
		_play_animation("falling_idle")
		return
	
	if not is_moving:
		# Idle - cycle through idle variants for variety
		if current_anim.begins_with("idle"):
			return  # Already playing an idle variant
		else:
			var idle_variant = randi() % 5 + 1
			if idle_variant == 1:
				_play_animation("idle")
			else:
				_play_animation("idle_" + str(idle_variant))
		return
	
	# Moving animations
	var abs_strafe = abs(move_direction.x)
	var abs_forward = abs(move_direction.y)
	
	# Prioritize strafe for stealth
	if is_sneaking and abs_strafe > 0.3:
		if move_direction.x < 0:
			_play_animation("crouched_sneaking_left")
		else:
			_play_animation("crouched_sneaking_right")
		return
	
	# Strafe animations (non-stealth)
	if abs_strafe > 0.5 and not is_sneaking:
		if move_direction.x < 0:
			_play_animation("left_strafe")
		else:
			_play_animation("right_strafe")
		return
	
	# Forward/backward movement
	if abs_forward > 0.3:
		if is_sneaking:
			# Sneaking uses crouched animations (slower)
			if move_direction.y < 0:
				_play_animation("crouched_sneaking_right")  # Forward sneak
			else:
				_play_animation("crouched_sneaking_left")  # Backward sneak
		else:
			# Normal movement
			if speed_magnitude > 5.0:
				_play_animation("running")
			else:
				_play_animation("walking")
		return
	
	# Fallback to walking
	_play_animation("walking")

func _play_animation(anim_name: String, blend_time: float = 0.3):
	"""Play animation with smooth blending (longer blend for stealth)"""
	if current_anim == anim_name:
		return  # Already playing
	
	if not animation_player:
		return
	
	if not animation_player.has_animation(anim_name):
		# print("Warning: Animation '", anim_name, "' not found")
		return
	
	current_anim = anim_name
	animation_player.play(anim_name, blend_time)

func _scout_area(delta):
	# Autonomous scouting behavior
	if patrol_points.is_empty():
		# Generate strategic patrol points (enemy territory)
		for i in range(5):
			var random_offset = Vector3(
				randf_range(-25, 25),
				0,
				randf_range(-25, 25)
			)
			patrol_points.append(global_position + random_offset)
	
	if patrol_points.is_empty():
		return
	
	var target_point = patrol_points[current_patrol_index]
	navigation_agent.target_position = target_point
	
	if global_position.distance_to(target_point) < 2.0:
		# Reached waypoint, pause briefly
		current_patrol_index = (current_patrol_index + 1) % patrol_points.size()
		velocity = Vector3.ZERO
		return
	
	var next_position = navigation_agent.get_next_path_position()
	var direction = (next_position - global_position).normalized()
	
	# Use sneak speed when in stealth mode
	var move_speed = sneak_speed if is_sneaking else speed
	velocity = velocity.lerp(direction * move_speed, acceleration * delta)
	
	# Face movement direction
	if direction.length() > 0.1:
		var target_rotation = atan2(direction.x, direction.z)
		rotation.y = lerp_angle(rotation.y, target_rotation, delta * 5.0)

func _follow_owner(delta):
	if not owner_player or not is_instance_valid(owner_player):
		state = "scout"
		return
	
	var distance_to_owner = global_position.distance_to(owner_player.global_position)
	
	# Stay 8-15m behind owner (scout distance)
	if distance_to_owner > 15.0:
		navigation_agent.target_position = owner_player.global_position
		var next_position = navigation_agent.get_next_path_position()
		var direction = (next_position - global_position).normalized()
		
		var move_speed = sneak_speed if is_sneaking else speed
		velocity = velocity.lerp(direction * move_speed, acceleration * delta)
		
		# Face movement direction
		if direction.length() > 0.1:
			var target_rotation = atan2(direction.x, direction.z)
			rotation.y = lerp_angle(rotation.y, target_rotation, delta * 5.0)
		
	elif distance_to_owner < 8.0:
		# Too close, maintain distance
		var direction = (global_position - owner_player.global_position).normalized()
		velocity = velocity.lerp(direction * sneak_speed, acceleration * delta)
	else:
		# Good distance
		velocity = velocity.lerp(Vector3.ZERO, acceleration * delta)

func _retreat(delta):
	# Flee from danger when detected
	if not owner_player or not is_instance_valid(owner_player):
		# Flee to nearest cover
		_find_cover()
		return
	
	# Flee toward owner
	navigation_agent.target_position = owner_player.global_position
	var next_position = navigation_agent.get_next_path_position()
	var direction = (next_position - global_position).normalized()
	
	# Run at full speed when retreating
	is_sneaking = false
	velocity = velocity.lerp(direction * speed * 1.2, acceleration * delta)
	
	# Face movement direction
	if direction.length() > 0.1:
		var target_rotation = atan2(direction.x, direction.z)
		rotation.y = lerp_angle(rotation.y, target_rotation, delta * 8.0)
	
	# If reached owner, resume scouting
	if global_position.distance_to(owner_player.global_position) < 3.0:
		state = "scout"
		is_sneaking = true

func _hide_in_cover(delta):
	# Stay in cover position
	if global_position.distance_to(cover_position) > 1.0:
		# Move to cover
		navigation_agent.target_position = cover_position
		var next_position = navigation_agent.get_next_path_position()
		var direction = (next_position - global_position).normalized()
		velocity = velocity.lerp(direction * speed, acceleration * delta)
	else:
		# In cover, stay still
		if not is_in_cover:
			_enter_cover()
		velocity = velocity.lerp(Vector3.ZERO, acceleration * delta)

func _enter_cover():
	"""Enter cover animation"""
	is_in_cover = true
	_play_animation("stand_to_cover", 0.2)
	print("Spy Bot entered cover")

func _exit_cover():
	"""Exit cover animation"""
	is_in_cover = false
	_play_animation("cover_to_stand", 0.2)
	print("Spy Bot exited cover")

func _find_cover():
	# Look for nearby cover objects
	var best_cover = global_position
	var best_distance = 999.0
	
	# Simple cover finding (look for objects with "cover" in name or group)
	var nearby_objects = get_tree().get_nodes_in_group("cover")
	for obj in nearby_objects:
		if obj is Node3D:
			var distance = global_position.distance_to(obj.global_position)
			if distance < best_distance and distance < 20.0:
				best_distance = distance
				best_cover = obj.global_position
	
	if best_cover != global_position:
		cover_position = best_cover
		state = "hide"
		print("Spy Bot seeking cover")

func _check_stealth_status():
	# Check if any enemies are close enough to detect spy
	if detected_enemies.is_empty():
		is_detected = false
		return
	
	for enemy in detected_enemies:
		if not is_instance_valid(enemy):
			continue
		
		var distance = global_position.distance_to(enemy.global_position)
		if distance < stealth_radius:
			# Detected! Retreat
			if not is_detected:
				is_detected = true
				state = "retreat"
				print("Spy Bot detected by enemy!")
				
				# Notify owner
				if owner_player and owner_player.has_method("show_notification"):
					owner_player.show_notification("Spy Bot compromised!")
			return
	
	# Not detected, resume stealth
	if is_detected and state == "retreat":
		var all_far = true
		for enemy in detected_enemies:
			if is_instance_valid(enemy):
				if global_position.distance_to(enemy.global_position) < stealth_radius * 2:
					all_far = false
					break
		
		if all_far:
			is_detected = false
			state = "scout"
			print("Spy Bot resumed stealth")

func _scan_and_mark_enemies():
	# Update detected enemies list and notify owner
	for enemy in detected_enemies:
		if not is_instance_valid(enemy):
			continue
		
		# Send enemy position to owner's Battle Pad
		if owner_player and owner_player.has_method("on_spy_detected_enemy"):
			owner_player.on_spy_detected_enemy(enemy, enemy.global_position)

func _on_enemy_detected(body):
	if _is_enemy(body):
		if not detected_enemies.has(body):
			detected_enemies.append(body)
			print("Spy Bot detected enemy: ", body.name)
			
			# Notify owner
			if owner_player and owner_player.has_method("on_spy_detected_enemy"):
				owner_player.on_spy_detected_enemy(body, body.global_position)

func _on_enemy_lost(body):
	if detected_enemies.has(body):
		detected_enemies.erase(body)
		print("Spy Bot lost sight of enemy: ", body.name)

func _is_enemy(body) -> bool:
	if body.is_in_group("enemy"):
		if owner_player and owner_player.has_method("get_team") and body.has_method("get_team"):
			return body.get_team() != owner_player.get_team()
		return true
	return false

func start_camera_feed():
	"""Activate camera feed to owner's Battle Pad"""
	if not camera_3d:
		return
	
	camera_feed_active = true
	
	if owner_player and owner_player.has_method("receive_spy_camera_feed"):
		owner_player.receive_spy_camera_feed(self, camera_3d)
	
	print("Spy Bot camera feed activated")

func stop_camera_feed():
	"""Deactivate camera feed"""
	camera_feed_active = false
	
	if owner_player and owner_player.has_method("disconnect_spy_camera"):
		owner_player.disconnect_spy_camera()
	
	print("Spy Bot camera feed deactivated")

func take_damage(amount: int, attacker = null):
	current_health = max(0, current_health - amount)
	
	if current_health <= 0:
		die()
		return
	
	# Retreat when taking damage
	if not is_detected:
		is_detected = true
		state = "retreat"
		
		# Notify owner
		if owner_player and owner_player.has_method("show_notification"):
			owner_player.show_notification("Spy Bot under fire!")

func die():
	"""Death sequence with animation"""
	if is_dying:
		return
	
	is_dying = true
	print("Spy Bot destroyed")
	
	# Stop camera feed
	stop_camera_feed()
	
	# Exit cover if in cover
	if is_in_cover:
		is_in_cover = false
	
	# Play death animation
	_play_animation("death_1", 0.1)
	
	# Stop movement
	velocity = Vector3.ZERO
	
	# Notify owner
	if owner_player and owner_player.has_method("on_bot_destroyed"):
		owner_player.on_bot_destroyed(self)
	
	if owner_player and owner_player.has_method("show_notification"):
		owner_player.show_notification("Spy Bot destroyed - camera feed lost")
	
	# Wait for death animation to finish
	if animation_player and animation_player.has_animation("death_1"):
		var anim_length = animation_player.get_animation("death_1").length
		await get_tree().create_timer(anim_length).timeout
	
	queue_free()

func hack_by_player(hacker):
	if is_hacked:
		return false
	
	is_hacked = true
	owner_player = hacker
	detected_enemies.clear()
	is_detected = false
	
	# Change team colors
	if has_node("TeamIndicator"):
		get_node("TeamIndicator").modulate = hacker.team_color
	
	print("Spy Bot hacked by ", hacker.name)
	return true

func set_owner_player(player):
	owner_player = player

func set_patrol_area(points: Array):
	patrol_points = points
	current_patrol_index = 0

func command_follow():
	state = "follow"
	is_sneaking = true
	if is_in_cover:
		_exit_cover()

func command_scout():
	state = "scout"
	is_sneaking = true
	if is_in_cover:
		_exit_cover()

func command_hide():
	_find_cover()

func toggle_stealth():
	is_sneaking = not is_sneaking
	print("Spy Bot stealth mode: ", "ON" if is_sneaking else "OFF")

func get_health_percent() -> float:
	return float(current_health) / float(max_health)

func jump():
	"""Trigger jump (for traversing obstacles)"""
	if is_jumping or not is_on_floor():
		return
	
	is_jumping = true
	velocity.y = 7.0  # Jump strength
	
	# Reset jump flag after landing
	await get_tree().create_timer(0.5).timeout
	is_jumping = false
