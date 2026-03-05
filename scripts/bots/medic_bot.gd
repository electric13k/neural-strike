extends CharacterBody3D
class_name MedicBot

# Medic Bot - Support robot that heals players and provides medical assistance
# Prioritizes healing low-health allies, stays at safe distance

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

@onready var navigation_agent = $NavigationAgent3D
@onready var heal_area = $HealDetectionArea3D  # Area3D for detecting allies
@onready var heal_beam = $HealBeam  # Visual healing beam
@onready var medical_kit = $MedicalKit  # Visual medical kit model

func _ready():
	current_health = max_health
	navigation_agent.path_desired_distance = 1.0
	navigation_agent.target_desired_distance = 2.0
	
	if heal_beam:
		heal_beam.visible = false
	
	# Connect heal area signals
	if heal_area:
		heal_area.body_entered.connect(_on_ally_entered_range)
		heal_area.body_exited.connect(_on_ally_exited_range)

func _physics_process(delta):
	if current_health <= 0:
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
	_scan_for_healing_targets()
	
	move_and_slide()

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
	elif distance_to_owner < 5.0:
		# Too close, back away slightly
		var direction = (global_position - owner_player.global_position).normalized()
		velocity = velocity.lerp(direction * speed * 0.5, acceleration * delta)
	else:
		# Good distance, slow down
		velocity = velocity.lerp(Vector3.ZERO, acceleration * delta)
		
		# Rotate to face owner
		look_at(owner_player.global_position, Vector3.UP)

func _perform_healing(delta):
	if not heal_target or not is_instance_valid(heal_target):
		state = "follow"
		heal_target = null
		if heal_beam:
			heal_beam.visible = false
		return
	
	var distance = global_position.distance_to(heal_target.global_position)
	
	# Move closer if out of range
	if distance > heal_range:
		navigation_agent.target_position = heal_target.global_position
		var next_position = navigation_agent.get_next_path_position()
		var direction = (next_position - global_position).normalized()
		velocity = velocity.lerp(direction * speed, acceleration * delta)
		return
	
	# In range, stop and heal
	velocity = velocity.lerp(Vector3.ZERO, acceleration * delta)
	look_at(heal_target.global_position, Vector3.UP)
	
	# Apply healing
	if can_heal and heal_target.has_method("receive_healing"):
		var heal_amount = heal_rate * delta
		heal_target.receive_healing(heal_amount, self)
		
		# Show heal beam
		if heal_beam:
			heal_beam.visible = true
			heal_beam.look_at(heal_target.global_position, Vector3.UP)
	
	# Check if target is fully healed
	if heal_target.has_method("get_health_percent"):
		if heal_target.get_health_percent() >= 1.0:
			# Target fully healed
			can_heal = false
			heal_timer = heal_cooldown
			heal_target = null
			state = "follow"
			if heal_beam:
				heal_beam.visible = false

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

func _guard_position(delta):
	# Stay at current position, rotate to scan
	rotate_y(0.3 * delta)
	velocity = velocity.lerp(Vector3.ZERO, acceleration * delta)

func _scan_for_healing_targets():
	if not can_heal or state == "healing":
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
		print("Medic Bot healing: ", heal_target.name)

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
		if heal_beam:
			heal_beam.visible = false

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

func die():
	print("Medic Bot destroyed")
	
	# Notify owner
	if owner_player and owner_player.has_method("on_bot_destroyed"):
		owner_player.on_bot_destroyed(self)
	
	queue_free()

func hack_by_player(hacker):
	if is_hacked:
		return false
	
	is_hacked = true
	owner_player = hacker
	
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
