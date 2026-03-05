extends CharacterBody3D
class_name CourierBot

# Courier Bot - Fast logistics robot for delivering items and providing speed support
# Optimized for mobility, delivery, and rapid repositioning

@export var speed: float = 8.0  # Faster than player (speed-optimized)
@export var sprint_speed: float = 12.0  # Can sprint faster than player
@export var acceleration: float = 15.0
@export var max_health: int = 75  # Less HP than assault, more than spy

var current_health: int = 75
var owner_player = null  # Reference to owning player
var is_hacked: bool = false
var target_enemy = null
var patrol_points: Array = []
var current_patrol_index: int = 0
var state: String = "follow"  # follow, patrol, deliver, guard
var delivery_target = null  # Player or location to deliver to
var cargo_item = null  # Item being carried

@onready var navigation_agent = $NavigationAgent3D
@onready var vision_raycast = $VisionRaycast
@onready var cargo_indicator = $CargoIndicator  # Visual indicator for cargo

func _ready():
	current_health = max_health
	navigation_agent.path_desired_distance = 1.0
	navigation_agent.target_desired_distance = 1.5
	
	if cargo_indicator:
		cargo_indicator.visible = false

func _physics_process(delta):
	if current_health <= 0:
		return
	
	match state:
		"follow":
			_follow_owner(delta)
		"patrol":
			_patrol_area(delta)
		"deliver":
			_deliver_cargo(delta)
		"guard":
			_guard_position(delta)
	
	# Always scan for threats
	_scan_for_threats()
	
	move_and_slide()

func _follow_owner(delta):
	if not owner_player or not is_instance_valid(owner_player):
		state = "patrol"
		return
	
	var distance_to_owner = global_position.distance_to(owner_player.global_position)
	
	# Stay within 8-15m of owner (further than medic, closer positioning)
	if distance_to_owner > 15.0:
		navigation_agent.target_position = owner_player.global_position
		var next_position = navigation_agent.get_next_path_position()
		var direction = (next_position - global_position).normalized()
		
		# Use sprint speed when catching up
		var current_speed = sprint_speed if distance_to_owner > 20.0 else speed
		velocity = velocity.lerp(direction * current_speed, acceleration * delta)
	elif distance_to_owner < 8.0:
		# Too close, back away slightly
		var direction = (global_position - owner_player.global_position).normalized()
		velocity = velocity.lerp(direction * speed * 0.5, acceleration * delta)
	else:
		# Good distance, slow down
		velocity = velocity.lerp(Vector3.ZERO, acceleration * delta)

func _patrol_area(delta):
	if patrol_points.is_empty():
		# Generate random patrol points around spawn
		for i in range(4):
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

func _deliver_cargo(delta):
	if not delivery_target or not is_instance_valid(delivery_target):
		state = "follow"
		cargo_item = null
		if cargo_indicator:
			cargo_indicator.visible = false
		return
	
	var target_pos = delivery_target.global_position if delivery_target is Node3D else delivery_target
	navigation_agent.target_position = target_pos
	
	var distance = global_position.distance_to(target_pos)
	
	if distance < 2.0:
		# Delivery complete
		_complete_delivery()
		return
	
	var next_position = navigation_agent.get_next_path_position()
	var direction = (next_position - global_position).normalized()
	
	# Use sprint speed for urgent deliveries
	velocity = velocity.lerp(direction * sprint_speed, acceleration * delta)

func _guard_position(delta):
	# Stay at current position, rotate to face threats
	if target_enemy and is_instance_valid(target_enemy):
		look_at(target_enemy.global_position, Vector3.UP)
	
	velocity = velocity.lerp(Vector3.ZERO, acceleration * delta)

func _scan_for_threats():
	if not vision_raycast:
		return
	
	# Courier bots prioritize survival over combat
	# Only engage if directly threatened
	var space_state = get_world_3d().direct_space_state
	var query = PhysicsRayQueryParameters3D.create(global_position, global_position + transform.basis.z * 15.0)
	
	var result = space_state.intersect_ray(query)
	if result and result.collider:
		if result.collider.is_in_group("enemy") and not result.collider.is_in_group("hacked_by_" + str(owner_player.get_instance_id())):
			target_enemy = result.collider
			# Courier bots flee rather than fight
			if state == "follow":
				_flee_from_threat()

func _flee_from_threat():
	if not target_enemy:
		return
	
	# Run away from threat toward owner
	var flee_direction = (global_position - target_enemy.global_position).normalized()
	var flee_target = global_position + flee_direction * 10.0
	
	if owner_player and is_instance_valid(owner_player):
		# Flee toward owner for protection
		flee_target = owner_player.global_position
	
	navigation_agent.target_position = flee_target

func take_damage(amount: int, attacker = null):
	current_health = max(0, current_health - amount)
	
	if current_health <= 0:
		die()
		return
	
	# Flee when taking damage
	if attacker and is_instance_valid(attacker):
		target_enemy = attacker
		_flee_from_threat()

func die():
	print("Courier Bot destroyed")
	
	# Drop cargo if carrying
	if cargo_item:
		_drop_cargo()
	
	# Notify owner
	if owner_player and owner_player.has_method("on_bot_destroyed"):
		owner_player.on_bot_destroyed(self)
	
	queue_free()

func hack_by_player(hacker):
	if is_hacked:
		return false
	
	is_hacked = true
	owner_player = hacker
	
	# Change team colors/indicators
	if has_node("TeamIndicator"):
		get_node("TeamIndicator").modulate = hacker.team_color
	
	print("Courier Bot hacked by ", hacker.name)
	return true

# Courier-specific functions

func pick_up_cargo(item):
	cargo_item = item
	if cargo_indicator:
		cargo_indicator.visible = true
	print("Courier Bot picked up cargo: ", item)

func deliver_to(target):
	delivery_target = target
	state = "deliver"
	print("Courier Bot delivering to: ", target)

func _complete_delivery():
	if delivery_target and delivery_target.has_method("receive_delivery"):
		delivery_target.receive_delivery(cargo_item)
	
	cargo_item = null
	delivery_target = null
	state = "follow"
	
	if cargo_indicator:
		cargo_indicator.visible = false
	
	print("Delivery complete!")

func _drop_cargo():
	# TODO: Spawn cargo as pickup in world
	print("Cargo dropped at: ", global_position)
	cargo_item = null
	
	if cargo_indicator:
		cargo_indicator.visible = false

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
