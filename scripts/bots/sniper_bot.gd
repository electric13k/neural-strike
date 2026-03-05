extends CharacterBody3D
class_name SniperBot

# Sniper Bot - Long-range precision robot
# Finds elevated positions, aims for high-value targets, prioritizes headshots

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

@onready var navigation_agent = $NavigationAgent3D
@onready var vision_raycast = $VisionRaycast
@onready var laser_sight = $LaserSight  # Visual laser pointer
@onready var muzzle_flash = $MuzzleFlash
@onready var sniper_rifle = $SniperRifle  # Visual rifle model

func _ready():
	current_health = max_health
	navigation_agent.path_desired_distance = 1.5
	navigation_agent.target_desired_distance = 2.0
	
	if laser_sight:
		laser_sight.visible = false
	
	if muzzle_flash:
		muzzle_flash.visible = false
	
	# Find initial sniping position
	preferred_position = global_position

func _physics_process(delta):
	if current_health <= 0:
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
	
	move_and_slide()

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
	elif distance_to_owner < 15.0:
		# Too close, back away to preferred range
		var direction = (global_position - owner_player.global_position).normalized()
		velocity = velocity.lerp(direction * speed * 0.7, acceleration * delta)
	else:
		# Good distance, hold position
		velocity = velocity.lerp(Vector3.ZERO, acceleration * delta)
		
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
		return
	
	var next_position = navigation_agent.get_next_path_position()
	var direction = (next_position - global_position).normalized()
	velocity = velocity.lerp(direction * speed, acceleration * delta)

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
		die()
		return
	
	# Return fire if not already engaged
	if attacker and is_instance_valid(attacker) and not target_enemy:
		target_enemy = attacker
		is_aiming = true
		aim_timer = 0.0
		state = "sniping"

func die():
	print("Sniper Bot destroyed")
	
	# Notify owner
	if owner_player and owner_player.has_method("on_bot_destroyed"):
		owner_player.on_bot_destroyed(self)
	
	queue_free()

func hack_by_player(hacker):
	if is_hacked:
		return false
	
	is_hacked = true
	owner_player = hacker
	target_enemy = null  # Clear previous targets
	
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

func command_patrol():
	state = "patrol"

func command_guard():
	# Sniper bots guard by sniping from current position
	preferred_position = global_position
	state = "follow"  # Will scan and engage from position

func get_health_percent() -> float:
	return float(current_health) / float(max_health)
