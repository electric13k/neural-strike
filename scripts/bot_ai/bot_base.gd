extends CharacterBody3D
class_name BotBase

enum BotState { IDLE, PATROL, CHASE, ATTACK, HACKED }

# Stats
@export var max_health: int = 100
var current_health: int = 100

@export var speed: float = 3.5
@export var attack_range: float = 15.0
@export var detection_range: float = 20.0

# Bot ownership
var owner_peer_id: int = -1  # -1 = neutral, 0 = server, >0 = player ID
var is_hacked: bool = false

# State
var current_state = BotState.IDLE
var target: Node3D = null

@onready var nav_agent = $NavigationAgent3D if has_node("NavigationAgent3D") else null
@onready var vision_ray = $RayCast3D if has_node("RayCast3D") else null

func _ready():
	current_health = max_health
	add_to_group("bots")

func _physics_process(delta):
	match current_state:
		BotState.IDLE:
			_idle_behavior(delta)
		BotState.PATROL:
			_patrol_behavior(delta)
		BotState.CHASE:
			_chase_behavior(delta)
		BotState.ATTACK:
			_attack_behavior(delta)
		BotState.HACKED:
			_hacked_behavior(delta)

func _idle_behavior(_delta):
	# Look for enemies
	var enemy = _find_nearest_enemy()
	if enemy:
		target = enemy
		current_state = BotState.CHASE

func _patrol_behavior(_delta):
	# Simple patrol - override in subclasses
	if nav_agent and not nav_agent.is_navigation_finished():
		var next_pos = nav_agent.get_next_path_position()
		var direction = (next_pos - global_position).normalized()
		velocity = direction * speed
		move_and_slide()
	else:
		current_state = BotState.IDLE

func _chase_behavior(_delta):
	if not target or not is_instance_valid(target):
		current_state = BotState.IDLE
		return
	
	var distance = global_position.distance_to(target.global_position)
	
	if distance > detection_range:
		current_state = BotState.IDLE
		target = null
	elif distance <= attack_range:
		current_state = BotState.ATTACK
	else:
		if nav_agent:
			nav_agent.target_position = target.global_position
			if not nav_agent.is_navigation_finished():
				var next_pos = nav_agent.get_next_path_position()
				var direction = (next_pos - global_position).normalized()
				velocity = direction * speed
				move_and_slide()

func _attack_behavior(_delta):
	if not target or not is_instance_valid(target):
		current_state = BotState.IDLE
		return
	
	var distance = global_position.distance_to(target.global_position)
	
	if distance > attack_range:
		current_state = BotState.CHASE
	else:
		# Face target
		look_at(target.global_position, Vector3.UP)
		rotation.x = 0
		rotation.z = 0
		
		# Attack logic - override in subclasses
		_perform_attack()

func _hacked_behavior(_delta):
	# Follow owner's commands - implement in subclasses
	pass

func _perform_attack():
	# Override in subclasses (shoot, melee, etc.)
	pass

func _find_nearest_enemy() -> Node3D:
	var players = get_tree().get_nodes_in_group("players")
	var nearest: Node3D = null
	var nearest_dist = detection_range
	
	for player in players:
		if not is_instance_valid(player):
			continue
		
		# Don't attack owner
		if player.get_multiplayer_authority() == owner_peer_id:
			continue
		
		var dist = global_position.distance_to(player.global_position)
		if dist < nearest_dist:
			nearest = player
			nearest_dist = dist
	
	return nearest

func take_damage(amount: int, attacker_id: int = -1):
	current_health = max(0, current_health - amount)
	
	# Interrupt hacking if taking damage
	if is_hacked and current_health > 0:
		is_hacked = false
		current_state = BotState.IDLE
	
	if current_health <= 0:
		die()

func heal(amount: int):
	current_health = min(max_health, current_health + amount)

func die():
	queue_free()

func is_friendly_to(peer_id: int) -> bool:
	"""Check if this bot is friendly to a given peer"""
	return owner_peer_id == peer_id

func set_owner(peer_id: int):
	"""Change bot ownership (used when hacked)"""
	owner_peer_id = peer_id
	is_hacked = true
	current_state = BotState.HACKED
