extends CharacterBody3D
class_name MedicBot

# Medic Bot - Support robot that heals players and provides medical assistance
# Prioritizes healing low-health allies, stays at safe distance
# Animations: idle, walking, running, strafe, turn, jump, heal (magic heal), death

@export var speed: float = 5.5  # Slower than player (support role)
@export var acceleration: float = 12.0
@export var max_health: int = 100  # Moderate HP
@export var heal_range: float = 10.0  # Range to heal allies
@export var heal_rate: float = 15.0  # HP per second
@export var heal_cooldown: float = 2.0  # Cooldown between heals

var current_health: int = 100
var owner_player = null  # Reference to owning player
var is_hacked: bool = false
var heal_target = null  # Current healing target
var can_heal: bool = true
var heal_timer: float = 0.0
var state: String = "follow"  # follow, healing, patrol, guard
var patrol_points: Array = []
var current_patrol_index: int = 0

# Animation states
var current_anim: String = "idle"
var is_moving: bool = false
var move_direction: Vector2 = Vector2.ZERO
var turn_direction: float = 0.0
var is_jumping: bool = false
var is_dying: bool = false
var is_healing_active: bool = false

@onready var navigation_agent = $NavigationAgent3D
@onready var animation_player = $AnimationPlayer
@onready var model = $MedicModel  # 3D model with animations
@onready var heal_area = $HealDetectionArea3D  # Area3D for detecting allies
@onready var heal_beam = $HealBeam  # Visual healing beam
@onready var heal_effect = $HealEffect  # Particle effect for healing
@onready var medical_kit = $MedicalKit  # Visual medical kit model

func _ready():
	current_health = max_health
	navigation_agent.path_desired_distance = 1.0
	navigation_agent.target_desired_distance = 2.0
	
	# Setup animations from FBX files
	_setup_animations()
	
	if heal_beam:
		heal_beam.visible = false
	
	if heal_effect:
		heal_effect.emitting = false
	
	# Connect heal area signals
	if heal_area:
		heal_area.body_entered.connect(_on_ally_entered_range)
		heal_area.body_exited.connect(_on_ally_exited_range)
	
	# Start with idle animation
	_play_animation("idle")

func _setup_animations():
	"""Setup animation library from imported FBX files"""
	if not animation_player:
		return
	
	# Animation mappings from FBX files
	# Animations should be in res://assets/models/bots/medic/animations/
	var anim_paths = {
		"idle": "res://assets/models/bots/medic/animations/idle.fbx",
		"walking": "res://assets/models/bots/medic/animations/walking.fbx",
		"running": "res://assets/models/bots/medic/animations/running.fbx",
		"left_strafe": "res://assets/models/bots/medic/animations/left strafe.fbx",
		"right_strafe": "res://assets/models/bots/medic/animations/right strafe.fbx",
		"left_strafe_walking": "res://assets/models/bots/medic/animations/left strafe walking.fbx",
		"right_strafe_walking": "res://assets/models/bots/medic/animations/right strafe walking.fbx",
		"left_turn_90": "res://assets/models/bots/medic/animations/left turn 90.fbx",
		"right_turn_90": "res://assets/models/bots/medic/animations/right turn 90.fbx",
		"left_turn": "res://assets/models/bots/medic/animations/left turn.fbx",
		"right_turn": "res://assets/models/bots/medic/animations/right turn.fbx",
		"jump": "res://assets/models/bots/medic/animations/jump.fbx",
		"magic_heal": "res://assets/models/bots/medic/animations/Magic Heal.fbx",
		"death": "res://assets/models/bots/medic/animations/Death.fbx",
		"tripo_convert": "res://assets/models/bots/medic/animations/tripo_convert_47ec81b6-084f-4f1b-8d3c-0f9e6c42c2d9.fbx"
	}
	
	print("Medic Bot animation system initialized")

func _physics_process(delta):
	if current_health <= 0 or is_dying:
		return
	
	# Update heal cooldown
	if not can_heal:
		heal_timer -= delta
		if heal_timer <= 0:
			can_heal = true
	
	match state:
		"follow":
			_follow_owner(delta)
		"healing":
			_perform_healing(delta)
		"patrol":
			_patrol_area(delta)
		"guard":
			_guard_position(delta)
	
	# Always scan for allies needing healing
	if state != "healing":
		_scan_for_healing_targets()
	
	# Update animation based on movement (unless healing)
	if not is_healing_active:
		_update_animation(delta)
	
	move_and_slide()

func _update_animation(delta):
	"""Determine which animation to play based on movement state"""
	if is_dying or is_healing_active:
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
	# 2. Strafe
	# 3. Walking/Running
	# 4. Turn
	# 5. Idle
	
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
		if speed_magnitude > 6.0:  # Fast strafe
			if move_direction.x < 0:
				_play_animation("left_strafe")
			else:
				_play_animation("right_strafe")
		else:  # Slow strafe
			if move_direction.x < 0:
				_play_animation("left_strafe_walking")
			else:
				_play_animation("right_strafe_walking")
	elif abs_forward > 0.5:
		# Forward/backward movement
		if speed_magnitude > 6.0:
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
	
	# Stay within 5-12m of owner (close support distance)
	if distance_to_owner > 12.0:
		navigation_agent.target_position = owner_player.global_position
		var next_position = navigation_agent.get_next_path_position()
		var direction = (next_position - global_position).normalized()
		velocity = velocity.lerp(direction * speed, acceleration * delta)
		
		# Face movement direction
		if direction.length() > 0.1:
			var target_rotation = atan2(direction.x, direction.z)
			rotation.y = lerp_angle(rotation.y, target_rotation, delta * 8.0)
		
	elif distance_to_owner < 5.0:
		# Too close, back away slightly
		var direction = (global_position - owner_player.global_position).normalized()
		velocity = velocity.lerp(direction * speed * 0.5, acceleration * delta)
		
		# Face owner while backing away
		look_at(owner_player.global_position, Vector3.UP)
	else:
		# Good distance, slow down
		velocity = velocity.lerp(Vector3.ZERO, acceleration * delta)
		
		# Face owner
		look_at(owner_player.global_position, Vector3.UP)

func _perform_healing(delta):
	if not heal_target or not is_instance_valid(heal_target):
		state = "follow"
		heal_target = null
		_stop_healing_animation()
		return
	
	var distance = global_position.distance_to(heal_target.global_position)
	
	# Move closer if out of range
	if distance > heal_range:
		navigation_agent.target_position = heal_target.global_position
		var next_position = navigation_agent.get_next_path_position()
		var direction = (next_position - global_position).normalized()
		velocity = velocity.lerp(direction * speed, acceleration * delta)
		
		# Face movement direction
		if direction.length() > 0.1:
			var target_rotation = atan2(direction.x, direction.z)
			rotation.y = lerp_angle(rotation.y, target_rotation, delta * 8.0)
		
		_stop_healing_animation()
		return
	
	# In range, stop and heal
	velocity = velocity.lerp(Vector3.ZERO, acceleration * delta)
	look_at(heal_target.global_position, Vector3.UP)
	
	# Start healing animation
	if not is_healing_active:
		_start_healing_animation()
	
	# Apply healing
	if can_heal and heal_target.has_method("receive_healing"):
		var heal_amount = heal_rate * delta
		heal_target.receive_healing(heal_amount, self)
	
	# Check if target is fully healed
	if heal_target.has_method("get_health_percent"):
		if heal_target.get_health_percent() >= 1.0:
			# Target fully healed
			can_heal = false
			heal_timer = heal_cooldown
			heal_target = null
			state = "follow"
			_stop_healing_animation()

func _start_healing_animation():
	"""Start healing visual effects and animation"""
	is_healing_active = true
	
	# Play magic heal animation
	_play_animation("magic_heal", 0.3)
	
	# Show heal beam
	if heal_beam:
		heal_beam.visible = true
		heal_beam.look_at(heal_target.global_position, Vector3.UP)
	
	# Start heal particle effect
	if heal_effect:
		heal_effect.emitting = true
	
	print("Medic Bot started healing")

func _stop_healing_animation():
	"""Stop healing visual effects"""
	is_healing_active = false
	
	# Hide heal beam
	if heal_beam:
		heal_beam.visible = false
	
	# Stop heal particle effect
	if heal_effect:
		heal_effect.emitting = false

func _patrol_area(delta):
	if patrol_points.is_empty():
		# Generate random patrol points
		for i in range(4):
			var random_offset = Vector3(
				randf_range(-15, 15),
				0,
				randf_range(-15, 15)
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
		rotation.y = lerp_angle(rotation.y, target_rotation, delta * 6.0)

func _guard_position(delta):
	# Stay at current position, rotate to scan
	rotate_y(0.3 * delta)
	velocity = velocity.lerp(Vector3.ZERO, acceleration * delta)

func _scan_for_healing_targets():
	if not can_heal:
		return
	
	if not heal_area:
		return
	
	# Find allies in range that need healing
	var bodies = heal_area.get_overlapping_bodies()
	var lowest_health_target = null
	var lowest_health_percent = 1.0
	
	for body in bodies:
		if body == self:
			continue
		
		# Check if ally (same team)
		if not _is_ally(body):
			continue
		
		# Check health
		if body.has_method("get_health_percent"):
			var health_percent = body.get_health_percent()
			if health_percent < lowest_health_percent and health_percent < 0.8:  # Only heal if <80% HP
				lowest_health_percent = health_percent
				lowest_health_target = body
	
	if lowest_health_target:
		heal_target = lowest_health_target
		state = "healing"
		print("Medic Bot targeting heal: ", heal_target.name)

func _is_ally(body) -> bool:
	# Check if body is on same team
	if body == owner_player:
		return true
	
	if body.has_method("get_team") and owner_player and owner_player.has_method("get_team"):
		return body.get_team() == owner_player.get_team()
	
	# Check if it's a bot owned by same player
	if body.has_method("get_owner_player"):
		return body.get_owner_player() == owner_player
	
	return false

func _on_ally_entered_range(body):
	if _is_ally(body) and body.has_method("get_health_percent"):
		if body.get_health_percent() < 0.8:
			print("Ally entered heal range: ", body.name)

func _on_ally_exited_range(body):
	if body == heal_target:
		heal_target = null
		state = "follow"
		_stop_healing_animation()

func take_damage(amount: int, attacker = null):
	current_health = max(0, current_health - amount)
	
	if current_health <= 0:
		die()
		return
	
	# Run away when taking damage
	if attacker and is_instance_valid(attacker):
		_flee_from_threat(attacker)

func _flee_from_threat(threat):
	# Medic bots flee to safety
	var flee_direction = (global_position - threat.global_position).normalized()
	var flee_target = global_position + flee_direction * 15.0
	
	if owner_player and is_instance_valid(owner_player):
		# Flee toward owner for protection
		flee_target = owner_player.global_position
	
	navigation_agent.target_position = flee_target
	
	# Stop healing if fleeing
	if state == "healing":
		heal_target = null
		_stop_healing_animation()
		state = "follow"

func die():
	"""Death sequence with animation"""
	if is_dying:
		return
	
	is_dying = true
	print("Medic Bot destroyed")
	
	# Stop healing
	_stop_healing_animation()
	
	# Play death animation
	_play_animation("death", 0.1)
	
	# Stop movement
	velocity = Vector3.ZERO
	
	# Notify owner
	if owner_player and owner_player.has_method("on_bot_destroyed"):
		owner_player.on_bot_destroyed(self)
	
	# Wait for death animation to finish
	if animation_player and animation_player.has_animation("death"):
		var anim_length = animation_player.get_animation("death").length
		await get_tree().create_timer(anim_length).timeout
	
	queue_free()

func hack_by_player(hacker):
	if is_hacked:
		return false
	
	is_hacked = true
	owner_player = hacker
	
	# Stop healing enemy targets
	if heal_target:
		heal_target = null
		_stop_healing_animation()
		state = "follow"
	
	# Change team colors
	if has_node("TeamIndicator"):
		get_node("TeamIndicator").modulate = hacker.team_color
	
	print("Medic Bot hacked by ", hacker.name)
	return true

func set_owner_player(player):
	owner_player = player

func set_patrol_area(points: Array):
	patrol_points = points
	current_patrol_index = 0

func command_follow():
	state = "follow"

func command_patrol():
	state = "patrol"

func command_guard():
	state = "guard"

func get_health_percent() -> float:
	return float(current_health) / float(max_health)

func receive_healing(amount: float, healer):
	current_health = min(max_health, current_health + int(amount))
	print("Medic Bot healed for ", amount, " HP")

func jump():
	"""Trigger jump (for traversing obstacles)"""
	if is_jumping or not is_on_floor():
		return
	
	is_jumping = true
	velocity.y = 8.0  # Jump strength
	
	# Reset jump flag after landing
	await get_tree().create_timer(0.5).timeout
	is_jumping = false
