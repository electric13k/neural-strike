extends CharacterBody3D
class_name CourierBot

# Courier Bot - Fast delivery robot
# Fastest bot, carries items to designated targets
# Animations: idle, walking, running, left/right strafe, left/right turn, jump, dying

@export var speed: float = 8.0  # Fast base speed
@export var sprint_speed: float = 12.0  # Very fast sprint
@export var acceleration: float = 15.0
@export var max_health: int = 75  # Lower HP (speed over durability)

var current_health: int = 75
var owner_player = null
var is_hacked: bool = false
var delivery_target = null  # Where to deliver items
var cargo = null  # What item is being carried
var state: String = "follow"  # follow, delivering, returning, patrol
var patrol_points: Array = []
var current_patrol_index: int = 0
var is_sprinting: bool = false

# Animation states
var current_anim: String = "idle"
var is_moving: bool = false
var move_direction: Vector2 = Vector2.ZERO
var turn_direction: float = 0.0  # -1 = left, 1 = right
var is_jumping: bool = false
var is_dying: bool = false

@onready var navigation_agent = $NavigationAgent3D
@onready var animation_player = $AnimationPlayer
@onready var model = $CourierModel  # 3D model with animations
@onready var cargo_attachment = $CargoAttachment  # Node where cargo attaches

func _ready():
	current_health = max_health
	navigation_agent.path_desired_distance = 0.8
	navigation_agent.target_desired_distance = 1.5
	
	# Setup animations from FBX files
	_setup_animations()
	
	# Start with idle animation
	_play_animation("idle")

func _setup_animations():
	"""Setup animation library from imported FBX files"""
	if not animation_player:
		return
	
	# Animation mappings from FBX files
	var animation_library = AnimationLibrary.new()
	
	# Load animations (these should be in res://assets/models/bots/courier/animations/)
	# Assuming animations are imported from FBX files
	var anim_paths = {
		"idle": "res://assets/models/bots/courier/animations/idle.fbx",
		"walking": "res://assets/models/bots/courier/animations/walking.fbx",
		"running": "res://assets/models/bots/courier/animations/running.fbx",
		"left_strafe": "res://assets/models/bots/courier/animations/left strafe.fbx",
		"right_strafe": "res://assets/models/bots/courier/animations/right strafe.fbx",
		"left_strafe_walking": "res://assets/models/bots/courier/animations/left strafe walking.fbx",
		"right_strafe_walking": "res://assets/models/bots/courier/animations/right strafe walking.fbx",
		"left_turn_90": "res://assets/models/bots/courier/animations/left turn 90.fbx",
		"right_turn_90": "res://assets/models/bots/courier/animations/right turn 90.fbx",
		"left_turn": "res://assets/models/bots/courier/animations/left turn.fbx",
		"right_turn": "res://assets/models/bots/courier/animations/right turn.fbx",
		"jump": "res://assets/models/bots/courier/animations/jump.fbx",
		"dying": "res://assets/models/bots/courier/animations/Dying courier bot.fbx",
		"tripo_convert": "res://assets/models/bots/courier/animations/tripo_convert_4a5e0993-0920-4f9f-a06c-e18c7bbac25d.fbx"
	}
	
	print("Courier Bot animation system initialized")

func _physics_process(delta):
	if current_health <= 0 or is_dying:
		return
	
	match state:
		"follow":
			_follow_owner(delta)
		"delivering":
			_deliver_cargo(delta)
		"returning":
			_return_to_owner(delta)
		"patrol":
			_patrol_area(delta)
	
	# Update animation based on movement
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
		
		# Calculate forward/back and strafe
		var forward_amount = forward.dot(velocity_normalized)
		var right = transform.basis.x
		var strafe_amount = right.dot(velocity_normalized)
		
		move_direction = Vector2(strafe_amount, forward_amount)
	else:
		move_direction = Vector2.ZERO
	
	# Determine animation priority:
	# 1. Jump (if jumping)
	# 2. Strafe (if moving sideways)
	# 3. Running (if sprinting forward)
	# 4. Walking (if moving forward)
	# 5. Turn (if rotating in place)
	# 6. Idle (default)
	
	if is_jumping:
		_play_animation("jump")
		return
	
	if not is_moving:
		# Check for turning in place
		if abs(turn_direction) > 0.1:
			if turn_direction < 0:
				_play_animation("left_turn")
			else:
				_play_animation("right_turn")
		else:
			_play_animation("idle")
		return
	
	# Moving animations
	var abs_strafe = abs(move_direction.x)
	var abs_forward = abs(move_direction.y)
	
	# Prioritize strafe if significant
	if abs_strafe > 0.5:
		if is_sprinting:
			# Fast strafe
			if move_direction.x < 0:
				_play_animation("left_strafe")
			else:
				_play_animation("right_strafe")
		else:
			# Walking strafe
			if move_direction.x < 0:
				_play_animation("left_strafe_walking")
			else:
				_play_animation("right_strafe_walking")
	elif abs_forward > 0.5:
		# Forward/backward movement
		if is_sprinting:
			_play_animation("running")
		else:
			_play_animation("walking")
	else:
		_play_animation("idle")

func _play_animation(anim_name: String, blend_time: float = 0.2):
	"""Play animation with blending"""
	if current_anim == anim_name:
		return  # Already playing
	
	if not animation_player:
		return
	
	# Check if animation exists
	if not animation_player.has_animation(anim_name):
		print("Warning: Animation '", anim_name, "' not found")
		return
	
	current_anim = anim_name
	animation_player.play(anim_name, blend_time)
	
func _follow_owner(delta):
	if not owner_player or not is_instance_valid(owner_player):
		state = "patrol"
		return
	
	var distance_to_owner = global_position.distance_to(owner_player.global_position)
	
	# Stay within 8-15m of owner
	if distance_to_owner > 15.0:
		is_sprinting = true
		navigation_agent.target_position = owner_player.global_position
		var next_position = navigation_agent.get_next_path_position()
		var direction = (next_position - global_position).normalized()
		velocity = velocity.lerp(direction * sprint_speed, acceleration * delta)
		
		# Face movement direction
		if direction.length() > 0.1:
			var target_rotation = atan2(direction.x, direction.z)
			rotation.y = lerp_angle(rotation.y, target_rotation, delta * 8.0)
		
	elif distance_to_owner > 8.0:
		is_sprinting = false
		navigation_agent.target_position = owner_player.global_position
		var next_position = navigation_agent.get_next_path_position()
		var direction = (next_position - global_position).normalized()
		velocity = velocity.lerp(direction * speed, acceleration * delta)
		
		# Face movement direction
		if direction.length() > 0.1:
			var target_rotation = atan2(direction.x, direction.z)
			rotation.y = lerp_angle(rotation.y, target_rotation, delta * 8.0)
	else:
		# Good distance, slow down
		is_sprinting = false
		velocity = velocity.lerp(Vector3.ZERO, acceleration * delta)

func _deliver_cargo(delta):
	if not delivery_target or not is_instance_valid(delivery_target):
		# No target, return cargo to owner
		state = "returning"
		return
	
	is_sprinting = true  # Always sprint when delivering
	navigation_agent.target_position = delivery_target.global_position
	
	var distance = global_position.distance_to(delivery_target.global_position)
	
	if distance < 2.0:
		# Reached delivery target
		_complete_delivery()
		return
	
	var next_position = navigation_agent.get_next_path_position()
	var direction = (next_position - global_position).normalized()
	velocity = velocity.lerp(direction * sprint_speed, acceleration * delta)
	
	# Face movement direction
	if direction.length() > 0.1:
		var target_rotation = atan2(direction.x, direction.z)
		rotation.y = lerp_angle(rotation.y, target_rotation, delta * 10.0)

func _return_to_owner(delta):
	if not owner_player or not is_instance_valid(owner_player):
		state = "patrol"
		return
	
	is_sprinting = true
	navigation_agent.target_position = owner_player.global_position
	
	var distance = global_position.distance_to(owner_player.global_position)
	
	if distance < 3.0:
		# Returned to owner
		if cargo:
			_return_cargo_to_owner()
		state = "follow"
		return
	
	var next_position = navigation_agent.get_next_path_position()
	var direction = (next_position - global_position).normalized()
	velocity = velocity.lerp(direction * sprint_speed, acceleration * delta)
	
	# Face movement direction
	if direction.length() > 0.1:
		var target_rotation = atan2(direction.x, direction.z)
		rotation.y = lerp_angle(rotation.y, target_rotation, delta * 10.0)

func _patrol_area(delta):
	if patrol_points.is_empty():
		# Generate random patrol points
		for i in range(4):
			var random_offset = Vector3(
				randf_range(-20, 20),
				0,
				randf_range(-20, 20)
			)
			patrol_points.append(global_position + random_offset)
	
	if patrol_points.is_empty():
		return
	
	is_sprinting = false
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
		rotation.y = lerp_angle(rotation.y, target_rotation, delta * 6.0)

func _complete_delivery():
	"""Deliver cargo to target"""
	if not cargo:
		return
	
	if delivery_target.has_method("receive_delivery"):
		delivery_target.receive_delivery(cargo, self)
	
	# Remove cargo
	if cargo_attachment and cargo.get_parent() == cargo_attachment:
		cargo_attachment.remove_child(cargo)
	
	cargo = null
	delivery_target = null
	state = "returning"
	
	print("Courier Bot delivered cargo")

func _return_cargo_to_owner():
	"""Return undelivered cargo to owner"""
	if not cargo:
		return
	
	if owner_player.has_method("receive_returned_cargo"):
		owner_player.receive_returned_cargo(cargo, self)
	
	# Remove cargo
	if cargo_attachment and cargo.get_parent() == cargo_attachment:
		cargo_attachment.remove_child(cargo)
	
	cargo = null
	delivery_target = null
	
	print("Courier Bot returned cargo to owner")

func take_damage(amount: int, attacker = null):
	current_health = max(0, current_health - amount)
	
	if current_health <= 0:
		die()
		return
	
	# Flee when taking damage (courier bots avoid combat)
	if attacker and is_instance_valid(attacker):
		_flee_from_threat(attacker)

func _flee_from_threat(threat):
	"""Flee away from threat at high speed"""
	var flee_direction = (global_position - threat.global_position).normalized()
	var flee_target = global_position + flee_direction * 20.0
	
	if owner_player and is_instance_valid(owner_player):
		# Flee toward owner for protection
		flee_target = owner_player.global_position
	
	is_sprinting = true
	navigation_agent.target_position = flee_target
	
	# Override state to fleeing
	if state != "delivering":
		state = "returning"

func die():
	"""Death sequence with animation"""
	if is_dying:
		return
	
	is_dying = true
	print("Courier Bot destroyed")
	
	# Play dying animation
	_play_animation("dying", 0.1)
	
	# Stop movement
	velocity = Vector3.ZERO
	
	# Drop cargo if carrying
	if cargo and cargo_attachment:
		if cargo.get_parent() == cargo_attachment:
			cargo_attachment.remove_child(cargo)
			# Add cargo to world
			get_parent().add_child(cargo)
			cargo.global_position = global_position
	
	# Notify owner
	if owner_player and owner_player.has_method("on_bot_destroyed"):
		owner_player.on_bot_destroyed(self)
	
	# Wait for death animation to finish
	if animation_player and animation_player.has_animation("dying"):
		var anim_length = animation_player.get_animation("dying").length
		await get_tree().create_timer(anim_length).timeout
	
	queue_free()

func hack_by_player(hacker):
	if is_hacked:
		return false
	
	is_hacked = true
	owner_player = hacker
	
	# Drop cargo if carrying enemy cargo
	if cargo:
		if cargo_attachment and cargo.get_parent() == cargo_attachment:
			cargo_attachment.remove_child(cargo)
			get_parent().add_child(cargo)
			cargo.global_position = global_position
		cargo = null
		delivery_target = null
	
	# Change team colors
	if has_node("TeamIndicator"):
		get_node("TeamIndicator").modulate = hacker.team_color
	
	print("Courier Bot hacked by ", hacker.name)
	return true

func set_owner_player(player):
	owner_player = player

func assign_delivery(item, target):
	"""Assign cargo and delivery target"""
	cargo = item
	delivery_target = target
	state = "delivering"
	
	# Attach cargo visually
	if cargo_attachment:
		if cargo.get_parent():
			cargo.get_parent().remove_child(cargo)
		cargo_attachment.add_child(cargo)
		cargo.position = Vector3.ZERO
	
	print("Courier Bot assigned delivery to: ", target.name if target else "unknown")

func command_follow():
	state = "follow"

func command_patrol():
	state = "patrol"

func command_guard():
	# Courier bots don't guard, they patrol instead
	command_patrol()

func jump():
	"""Trigger jump (for traversing obstacles)"""
	if is_jumping or not is_on_floor():
		return
	
	is_jumping = true
	velocity.y = 8.0  # Jump strength
	
	# Reset jump flag after landing
	await get_tree().create_timer(0.5).timeout
	is_jumping = false
