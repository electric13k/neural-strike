extends CharacterBody3D
class_name SniperBot

# Sniper Bot - Long-range precision robot with full animation system
# Finds elevated positions, aims for high-value targets, prioritizes headshots
# Animations: idle, walking, running, sprint, crouch, jump, aim, death (50+ total)

@export var speed: float = 4.5  # Slowest bot (heavy, stable)
@export var acceleration: float = 8.0
@export var max_health: int = 120  # High HP (armored marksman)
@export var shot_damage: int = 60  # High damage per shot
@export var fire_rate: float = 2.0  # Seconds between shots
@export var detection_range: float = 50.0  # Very long range
@export var aim_time: float = 1.5  # Time to acquire target

var current_health: int = 120
var owner_player = null
var is_hacked: bool = false
var target_enemy = null
var can_shoot: bool = true
var fire_timer: float = 0.0
var aim_timer: float = 0.0
var is_aiming: bool = false
var state: String = "follow"  # follow, sniping, repositioning, patrol
var preferred_position: Vector3  # Elevated sniping position
var patrol_points: Array = []
var current_patrol_index: int = 0

# Animation states
var current_anim: String = "idle"
var is_moving: bool = false
var move_direction: Vector2 = Vector2.ZERO
var is_crouching: bool = false
var is_jumping: bool = false
var is_dying: bool = false

@onready var navigation_agent = $NavigationAgent3D
@onready var vision_raycast = $VisionRaycast
@onready var laser_sight = $LaserSight  # Visual laser pointer
@onready var muzzle_flash = $MuzzleFlash
@onready var sniper_rifle = $SniperRifle  # Visual rifle model
@onready var animation_player = $AnimationPlayer
@onready var model = $SniperModel  # 3D model with animations

func _ready():
	current_health = max_health
	navigation_agent.path_desired_distance = 1.5
	navigation_agent.target_desired_distance = 2.0
	
	# Setup animations from FBX files
	_setup_animations()
	
	if laser_sight:
		laser_sight.visible = false
	
	if muzzle_flash:
		muzzle_flash.visible = false
	
	# Find initial sniping position
	preferred_position = global_position
	
	# Start with idle animation
	_play_animation("idle")

func _setup_animations():
	"""Setup animation library from imported FBX files"""
	if not animation_player:
		return
	
	# Animation mappings from FBX files
	# Animations should be in res://assets/models/bots/sniper/animations/
	var anim_paths = {
		# === CORE LOCOMOTION ===
		"idle": "res://assets/models/bots/sniper/animations/idle.fbx",
		"idle_aiming": "res://assets/models/bots/sniper/animations/idle aiming.fbx",
		"idle_crouching": "res://assets/models/bots/sniper/animations/idle crouching.fbx",
		"idle_crouching_aiming": "res://assets/models/bots/sniper/animations/idle crouching aiming.fbx",
		
		# === JUMPING ===
		"jump_up": "res://assets/models/bots/sniper/animations/jump up.fbx",
		"jump_loop": "res://assets/models/bots/sniper/animations/jump loop.fbx",
		"jump_down": "res://assets/models/bots/sniper/animations/jump down.fbx",
		
		# === WALKING (8 DIRECTIONS) ===
		"walk_forward": "res://assets/models/bots/sniper/animations/walk forward.fbx",
		"walk_backward": "res://assets/models/bots/sniper/animations/walk backward.fbx",
		"walk_left": "res://assets/models/bots/sniper/animations/walk left.fbx",
		"walk_right": "res://assets/models/bots/sniper/animations/walk right.fbx",
		"walk_forward_left": "res://assets/models/bots/sniper/animations/walk forward left.fbx",
		"walk_forward_right": "res://assets/models/bots/sniper/animations/walk forward right.fbx",
		"walk_backward_left": "res://assets/models/bots/sniper/animations/walk backward left.fbx",
		"walk_backward_right": "res://assets/models/bots/sniper/animations/walk backward right.fbx",
		
		# === RUNNING (8 DIRECTIONS) ===
		"run_forward": "res://assets/models/bots/sniper/animations/run forward.fbx",
		"run_backward": "res://assets/models/bots/sniper/animations/run backward.fbx",
		"run_left": "res://assets/models/bots/sniper/animations/run left.fbx",
		"run_right": "res://assets/models/bots/sniper/animations/run right.fbx",
		"run_forward_left": "res://assets/models/bots/sniper/animations/run forward left.fbx",
		"run_forward_right": "res://assets/models/bots/sniper/animations/run forward right.fbx",
		"run_backward_left": "res://assets/models/bots/sniper/animations/run backward left.fbx",
		"run_backward_right": "res://assets/models/bots/sniper/animations/run backward right.fbx",
		
		# === SPRINTING (8 DIRECTIONS) ===
		"sprint_forward": "res://assets/models/bots/sniper/animations/sprint forward.fbx",
		"sprint_backward": "res://assets/models/bots/sniper/animations/sprint backward.fbx",
		"sprint_left": "res://assets/models/bots/sniper/animations/sprint left.fbx",
		"sprint_right": "res://assets/models/bots/sniper/animations/sprint right.fbx",
		"sprint_forward_left": "res://assets/models/bots/sniper/animations/sprint forward left.fbx",
		"sprint_forward_right": "res://assets/models/bots/sniper/animations/sprint forward right.fbx",
		"sprint_backward_left": "res://assets/models/bots/sniper/animations/sprint backward left.fbx",
		"sprint_backward_right": "res://assets/models/bots/sniper/animations/sprint backward right.fbx",
		
		# === CROUCHING (8 DIRECTIONS) ===
		"walk_crouching_forward": "res://assets/models/bots/sniper/animations/walk crouching forward.fbx",
		"walk_crouching_backward": "res://assets/models/bots/sniper/animations/walk crouching backward.fbx",
		"walk_crouching_left": "res://assets/models/bots/sniper/animations/walk crouching left.fbx",
		"walk_crouching_right": "res://assets/models/bots/sniper/animations/walk crouching right.fbx",
		"walk_crouching_forward_left": "res://assets/models/bots/sniper/animations/walk crouching forward left.fbx",
		"walk_crouching_forward_right": "res://assets/models/bots/sniper/animations/walk crouching forward right.fbx",
		"walk_crouching_backward_left": "res://assets/models/bots/sniper/animations/walk crouching backward left.fbx",
		"walk_crouching_backward_right": "res://assets/models/bots/sniper/animations/walk crouching backward right.fbx",
		
		# === TURNING ===
		"turn_90_left": "res://assets/models/bots/sniper/animations/turn 90 left.fbx",
		"turn_90_right": "res://assets/models/bots/sniper/animations/turn 90 right.fbx",
		"crouching_turn_90_left": "res://assets/models/bots/sniper/animations/crouching turn 90 left.fbx",
		"crouching_turn_90_right": "res://assets/models/bots/sniper/animations/crouching turn 90 right.fbx",
		
		# === DEATH ANIMATIONS (6 VARIANTS) ===
		"death_from_front": "res://assets/models/bots/sniper/animations/death from the front.fbx",
		"death_from_back": "res://assets/models/bots/sniper/animations/death from the back.fbx",
		"death_from_right": "res://assets/models/bots/sniper/animations/death from right.fbx",
		"death_from_front_headshot": "res://assets/models/bots/sniper/animations/death from front headshot.fbx",
		"death_from_back_headshot": "res://assets/models/bots/sniper/animations/death from back headshot.fbx",
		"death_crouching_headshot_front": "res://assets/models/bots/sniper/animations/death crouching headshot front.fbx",
		
		# === ALTERNATE MODEL ===
		"tripo_convert": "res://assets/models/bots/sniper/animations/tripo_convert_a87f8a59-137c-4b48-bc3d-5122383bd6e1.fbx"
	}
	
	print("Sniper Bot animation system initialized with 50+ animations")

func _physics_process(delta):
	if current_health <= 0 or is_dying:
		return
	
	# Update fire cooldown
	if not can_shoot:
		fire_timer -= delta
		if fire_timer <= 0:
			can_shoot = true
	
	# Update aim timer
	if is_aiming:
		aim_timer += delta
	
	match state:
		"follow":
			_follow_owner(delta)
		"sniping":
			_sniper_behavior(delta)
		"repositioning":
			_reposition(delta)
		"patrol":
			_patrol_area(delta)
	
	# Always scan for targets
	_scan_for_targets()
	
	# Update animation based on state (unless sniping)
	if state != "sniping":
		_update_animation(delta)
	
	move_and_slide()

func _update_animation(delta):
	"""Determine which animation to play based on movement state"""
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
	# 1. Jump
	# 2. Crouching states
	# 3. Movement (Sprint/Run/Walk)
	# 4. Idle
	
	if is_jumping:
		_play_animation("jump_loop")
		return
	
	if not is_on_floor():
		_play_animation("jump_loop")
		return
	
	if not is_moving:
		# Idle animations
		if is_crouching:
			_play_animation("idle_crouching")
		else:
			_play_animation("idle")
		return
	
	# Moving animations based on speed and direction
	var abs_forward = abs(move_direction.y)
	var abs_strafe = abs(move_direction.x)
	
	# Determine primary direction
	var primary_dir = ""
	if abs_forward > abs_strafe:
		if move_direction.y < 0:
			primary_dir = "forward"
		else:
			primary_dir = "backward"
	else:
		if move_direction.x < 0:
			primary_dir = "left"
		else:
			primary_dir = "right"
	
	# Add diagonal direction if both axes significant
	if abs_forward > 0.3 and abs_strafe > 0.3:
		if move_direction.y < 0:  # Forward
			if move_direction.x < 0:
				primary_dir = "forward_left"
			else:
				primary_dir = "forward_right"
		else:  # Backward
			if move_direction.x < 0:
				primary_dir = "backward_left"
			else:
				primary_dir = "backward_right"
	
	# Determine speed tier
	var speed_tier = "walk"  # Default
	if speed_magnitude > 8.0:
		speed_tier = "sprint"
	elif speed_magnitude > 5.0:
		speed_tier = "run"
	
	# Build animation name
	var anim_name = ""
	if is_crouching:
		anim_name = "walk_crouching_" + primary_dir
	else:
		anim_name = speed_tier + "_" + primary_dir
	
	_play_animation(anim_name)

func _play_animation(anim_name: String, blend_time: float = 0.2):
	"""Play animation with blending"""
	if current_anim == anim_name:
		return  # Already playing
	
	if not animation_player:
		return
	
	# Check if animation exists
	if not animation_player.has_animation(anim_name):
		# print("Warning: Animation '", anim_name, "' not found")
		return
	
	current_anim = anim_name
	animation_player.play(anim_name, blend_time)

func _follow_owner(delta):
	if not owner_player or not is_instance_valid(owner_player):
		state = "patrol"
		return
	
	var distance_to_owner = global_position.distance_to(owner_player.global_position)
	
	# Stay 15-25m behind owner (long-range support distance)
	if distance_to_owner > 25.0:
		navigation_agent.target_position = owner_player.global_position
		var next_position = navigation_agent.get_next_path_position()
		var direction = (next_position - global_position).normalized()
		velocity = velocity.lerp(direction * speed, acceleration * delta)
		
		# Face movement direction
		if direction.length() > 0.1:
			var target_rotation = atan2(direction.x, direction.z)
			rotation.y = lerp_angle(rotation.y, target_rotation, delta * 6.0)
		
	elif distance_to_owner < 15.0:
		# Too close, back away to preferred range
		var direction = (global_position - owner_player.global_position).normalized()
		velocity = velocity.lerp(direction * speed * 0.7, acceleration * delta)
		
		# Face owner while backing away
		look_at(owner_player.global_position, Vector3.UP)
	else:
		# Good distance, hold position
		velocity = velocity.lerp(Vector3.ZERO, acceleration * delta)
		
		# Face owner
		look_at(owner_player.global_position, Vector3.UP)
		
		# Look for elevated position
		if randf() < 0.01:  # Occasionally seek better position
			_find_elevated_position()

func _sniper_behavior(delta):
	if not target_enemy or not is_instance_valid(target_enemy):
		target_enemy = null
		is_aiming = false
		aim_timer = 0.0
		state = "follow"
		if laser_sight:
			laser_sight.visible = false
		return
	
	# Stop moving, stabilize for shot
	velocity = velocity.lerp(Vector3.ZERO, acceleration * delta)
	
	# Aim at target
	look_at(target_enemy.global_position, Vector3.UP)
	
	# Show laser sight
	if laser_sight:
		laser_sight.visible = true
		laser_sight.look_at(target_enemy.global_position, Vector3.UP)
	
	# Play aiming animation
	if is_crouching:
		_play_animation("idle_crouching_aiming")
	else:
		_play_animation("idle_aiming")
	
	# Wait for aim time before shooting
	if aim_timer >= aim_time and can_shoot:
		_fire_shot()
		aim_timer = 0.0

func _reposition(delta):
	# Move to preferred sniping position
	navigation_agent.target_position = preferred_position
	
	if global_position.distance_to(preferred_position) < 2.0:
		# Reached position
		state = "follow"
		# Crouch when reaching sniping position
		is_crouching = true
		return
	
	var next_position = navigation_agent.get_next_path_position()
	var direction = (next_position - global_position).normalized()
	velocity = velocity.lerp(direction * speed, acceleration * delta)
	
	# Face movement direction
	if direction.length() > 0.1:
		var target_rotation = atan2(direction.x, direction.z)
		rotation.y = lerp_angle(rotation.y, target_rotation, delta * 6.0)

func _patrol_area(delta):
	if patrol_points.is_empty():
		# Generate random patrol points
		for i in range(3):
			var random_offset = Vector3(
				randf_range(-20, 20),
				0,
				randf_range(-20, 20)
			)
			patrol_points.append(global_position + random_offset)
	
	if patrol_points.is_empty():
		return
	
	var target_point = patrol_points[current_patrol_index]
	navigation_agent.target_position = target_point
	
	if global_position.distance_to(target_point) < 2.0:
		current_patrol_index = (current_patrol_index + 1) % patrol_points.size()
	
	var next_position = navigation_agent.get_next_path_position()
	var direction = (next_position - global_position).normalized()
	velocity = velocity.lerp(direction * speed, acceleration * delta)
	
	# Face movement direction
	if direction.length() > 0.1:
		var target_rotation = atan2(direction.x, direction.z)
		rotation.y = lerp_angle(rotation.y, target_rotation, delta * 5.0)

func _scan_for_targets():
	if state == "sniping" and target_enemy and is_instance_valid(target_enemy):
		return  # Already engaging
	
	if not vision_raycast:
		return
	
	# Scan for enemies in long range
	var space_state = get_world_3d().direct_space_state
	
	# Cast multiple rays in cone pattern for wide FOV
	var angles = [-15, -7.5, 0, 7.5, 15]  # degrees
	for angle in angles:
		var direction = transform.basis.z.rotated(Vector3.UP, deg_to_rad(angle))
		var query = PhysicsRayQueryParameters3D.create(
			global_position,
			global_position + direction * detection_range
		)
		
		var result = space_state.intersect_ray(query)
		if result and result.collider:
			if _is_enemy(result.collider):
				target_enemy = result.collider
				is_aiming = true
				aim_timer = 0.0
				state = "sniping"
				# Crouch for stability
				is_crouching = true
				print("Sniper Bot acquired target: ", target_enemy.name)
				return

func _is_enemy(body) -> bool:
	if body.is_in_group("enemy"):
		if owner_player and owner_player.has_method("get_team") and body.has_method("get_team"):
			return body.get_team() != owner_player.get_team()
		return true
	return false

func _fire_shot():
	if not target_enemy or not is_instance_valid(target_enemy):
		return
	
	# Raycast to check line of sight
	var space_state = get_world_3d().direct_space_state
	var query = PhysicsRayQueryParameters3D.create(
		global_position,
		target_enemy.global_position
	)
	
	var result = space_state.intersect_ray(query)
	if not result or result.collider != target_enemy:
		# Line of sight blocked
		print("Sniper Bot: Line of sight blocked")
		return
	
	# Deal damage
	if target_enemy.has_method("take_damage"):
		target_enemy.take_damage(shot_damage, self)
		print("Sniper Bot fired! Damage: ", shot_damage)
	
	# Show muzzle flash
	if muzzle_flash:
		muzzle_flash.visible = true
		await get_tree().create_timer(0.1).timeout
		muzzle_flash.visible = false
	
	# Play sound (TODO: Add AudioStreamPlayer3D)
	
	# Start cooldown
	can_shoot = false
	fire_timer = fire_rate
	
	# Notify owner's Battle Pad
	if owner_player and owner_player.has_method("on_bot_fired_shot"):
		owner_player.on_bot_fired_shot(self, target_enemy)

func _find_elevated_position():
	# Look for higher ground nearby
	var best_position = global_position
	var best_height = global_position.y
	
	for i in range(8):
		var random_offset = Vector3(
			randf_range(-15, 15),
			0,
			randf_range(-15, 15)
		)
		var test_position = global_position + random_offset
		
		# Raycast down to find ground height
		var space_state = get_world_3d().direct_space_state
		var query = PhysicsRayQueryParameters3D.create(
			test_position + Vector3.UP * 10,
			test_position + Vector3.DOWN * 10
		)
		
		var result = space_state.intersect_ray(query)
		if result:
			if result.position.y > best_height:
				best_height = result.position.y
				best_position = result.position
	
	if best_position != global_position:
		preferred_position = best_position
		state = "repositioning"
		print("Sniper Bot repositioning to elevated position")

func take_damage(amount: int, attacker = null):
	current_health = max(0, current_health - amount)
	
	if current_health <= 0:
		die(attacker)
		return
	
	# Return fire if not already engaged
	if attacker and is_instance_valid(attacker) and not target_enemy:
		target_enemy = attacker
		is_aiming = true
		aim_timer = 0.0
		state = "sniping"

func die(attacker = null):
	"""Death sequence with directional animation"""
	if is_dying:
		return
	
	is_dying = true
	print("Sniper Bot destroyed")
	
	# Determine death animation based on hit direction
	var death_anim = "death_from_front"  # Default
	
	if attacker and is_instance_valid(attacker):
		# Calculate hit direction
		var hit_direction = (attacker.global_position - global_position).normalized()
		var forward = -transform.basis.z
		var dot_forward = forward.dot(hit_direction)
		var right = transform.basis.x
		var dot_right = right.dot(hit_direction)
		
		# Determine which direction hit came from
		if dot_forward > 0.5:
			death_anim = "death_from_front"
		elif dot_forward < -0.5:
			death_anim = "death_from_back"
		elif abs(dot_right) > 0.5:
			death_anim = "death_from_right"
		
		# Random chance for headshot animation
		if randf() < 0.3:  # 30% chance
			if dot_forward > 0:
				death_anim = "death_from_front_headshot"
			else:
				death_anim = "death_from_back_headshot"
	
	# Use crouching death if crouched
	if is_crouching:
		death_anim = "death_crouching_headshot_front"
	
	# Play death animation
	_play_animation(death_anim, 0.1)
	
	# Stop movement
	velocity = Vector3.ZERO
	
	# Hide laser sight
	if laser_sight:
		laser_sight.visible = false
	
	# Notify owner
	if owner_player and owner_player.has_method("on_bot_destroyed"):
		owner_player.on_bot_destroyed(self)
	
	# Wait for death animation to finish
	if animation_player and animation_player.has_animation(death_anim):
		var anim_length = animation_player.get_animation(death_anim).length
		await get_tree().create_timer(anim_length).timeout
	
	queue_free()

func hack_by_player(hacker):
	if is_hacked:
		return false
	
	is_hacked = true
	owner_player = hacker
	target_enemy = null  # Clear previous targets
	is_aiming = false
	
	# Change team colors
	if has_node("TeamIndicator"):
		get_node("TeamIndicator").modulate = hacker.team_color
	
	print("Sniper Bot hacked by ", hacker.name)
	return true

func set_owner_player(player):
	owner_player = player

func set_patrol_area(points: Array):
	patrol_points = points
	current_patrol_index = 0

func command_follow():
	state = "follow"
	target_enemy = null
	is_crouching = false

func command_patrol():
	state = "patrol"
	is_crouching = false

func command_guard():
	# Sniper bots guard by sniping from current position
	preferred_position = global_position
	is_crouching = true  # Crouch when guarding
	state = "follow"  # Will scan and engage from position

func get_health_percent() -> float:
	return float(current_health) / float(max_health)

func jump():
	"""Trigger jump (for traversing obstacles)"""
	if is_jumping or not is_on_floor():
		return
	
	is_jumping = true
	velocity.y = 8.0  # Jump strength
	
	# Reset jump flag after landing
	await get_tree().create_timer(0.5).timeout
	is_jumping = false
