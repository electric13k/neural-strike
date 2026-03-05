extends CharacterBody3D
class_name BotBase

# Base class for all bot types
# Handles basic AI states, movement, and healthbar

@export var max_health: float = 100.0
@export var speed: float = 5.0
@export var detection_range: float = 20.0

var current_health: float = 100.0
var target: Node3D = null
var owner_peer_id: int = -1  # -1 = neutral, otherwise player's peer ID

enum State { IDLE, PATROL, CHASE, COMBAT }
var current_state: State = State.IDLE

@onready var nav_agent: NavigationAgent3D = null
@onready var healthbar: HealthBar3D = null

func _ready():
	current_health = max_health
	add_to_group("bots")
	
	# Get NavigationAgent3D if it exists
	if has_node("NavigationAgent3D"):
		nav_agent = $NavigationAgent3D
	
	# Set up healthbar
	if not healthbar and has_node("HealthBar3D"):
		healthbar = $HealthBar3D
	else:
		# Create healthbar if not in scene
		healthbar = preload("res://scripts/ui/healthbar_3d.gd").new()
		add_child(healthbar)
	
	healthbar.max_health = max_health
	healthbar.current_health = current_health
	healthbar.offset_y = 1.8  # Above bot head
	update_healthbar_team_color()

func _physics_process(delta):
	# Update healthbar team color every frame based on viewer
	update_healthbar_team_color()
	
	match current_state:
		State.IDLE: _idle_logic()
		State.PATROL: _patrol_logic()
		State.CHASE: _chase_logic()
		State.COMBAT: _combat_logic()

func update_healthbar_team_color():
	"""Update healthbar color based on who owns this bot vs local player."""
	if not healthbar:
		return
	
	# Get local player
	var local_player = get_tree().get_first_node_in_group("local_player")
	if not local_player:
		# No local player, default to enemy (red)
		healthbar.set_team(true)
		return
	
	# Check if bot is owned by local player
	var local_peer_id = local_player.get_peer_id() if local_player.has_method("get_peer_id") else -1
	
	if owner_peer_id == local_peer_id:
		# Same owner = friendly (green)
		healthbar.set_team(false)
	else:
		# Different owner = enemy (red)
		healthbar.set_team(true)

func _idle_logic():
	# Look for enemies
	find_target()
	if target:
		current_state = State.CHASE

func _patrol_logic():
	# Random movement
	pass

func _chase_logic():
	if not target or not is_instance_valid(target):
		target = null
		current_state = State.IDLE
		return
	
	if nav_agent:
		nav_agent.target_position = target.global_position
		var next_pos = nav_agent.get_next_path_position()
		velocity = (next_pos - global_position).normalized() * speed
	else:
		# Direct movement if no navigation
		velocity = (target.global_position - global_position).normalized() * speed
	
	move_and_slide()
	
	# Switch to combat when close
	if global_position.distance_to(target.global_position) < 10.0:
		current_state = State.COMBAT

func _combat_logic():
	if not target or not is_instance_valid(target):
		current_state = State.IDLE
		return
	
	# Face target
	look_at(target.global_position, Vector3.UP)
	
	# Keep distance
	var distance = global_position.distance_to(target.global_position)
	if distance > 15.0:
		current_state = State.CHASE

func find_target():
	# Find nearest enemy player
	var players = get_tree().get_nodes_in_group("players")
	var nearest_dist = detection_range
	for player in players:
		if is_instance_valid(player):
			# Don't target own owner
			if player.has_method("get_peer_id") and player.get_peer_id() == owner_peer_id:
				continue
			
			var dist = global_position.distance_to(player.global_position)
			if dist < nearest_dist:
				target = player
				nearest_dist = dist

func take_damage(amount: float, attacker: Node3D = null):
	"""Take damage and update healthbar."""
	current_health -= amount
	current_health = clamp(current_health, 0, max_health)
	
	if healthbar:
		healthbar.update_health(current_health)
	
	if current_health <= 0:
		die()
	elif attacker and not target:
		target = attacker
		current_state = State.CHASE
	
	print("[Bot %s] Took %d damage, health: %d/%d" % [name, amount, current_health, max_health])

func heal(amount: float):
	"""Heal bot (used by medic bots)."""
	current_health += amount
	current_health = clamp(current_health, 0, max_health)
	
	if healthbar:
		healthbar.update_health(current_health)

func die():
	print("[Bot %s] Died" % name)
	queue_free()

# Methods for virus system
func is_bot() -> bool:
	return true

func get_owner_id() -> int:
	return owner_peer_id

func transfer_ownership(new_owner_id: int):
	"""Transfer bot ownership (used by data knife virus)."""
	owner_peer_id = new_owner_id
	update_healthbar_team_color()
	print("%s transferred to player %d" % [name, new_owner_id])

func get_role() -> String:
	return "Base"

func get_health_percent() -> float:
	return (current_health / max_health) * 100.0

func get_peer_id() -> int:
	"""For compatibility with player targeting."""
	return owner_peer_id
