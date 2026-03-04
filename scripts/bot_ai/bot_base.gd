extends CharacterBody3D
class_name BotBase

# Base class for all bot types
# Handles basic AI states and movement

@export var max_health: float = 100.0
@export var speed: float = 5.0
@export var detection_range: float = 20.0

var current_health: float = 100.0
var target: Node3D = null
var owner_peer_id: int = -1  # -1 = neutral, otherwise player's peer ID

enum State { IDLE, PATROL, CHASE, COMBAT }
var current_state: State = State.IDLE

@onready var nav_agent: NavigationAgent3D = null

func _ready():
	current_health = max_health
	# Get NavigationAgent3D if it exists
	if has_node("NavigationAgent3D"):
		nav_agent = $NavigationAgent3D

func _physics_process(delta):
	match current_state:
		State.IDLE: _idle_logic()
		State.PATROL: _patrol_logic()
		State.CHASE: _chase_logic()
		State.COMBAT: _combat_logic()

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
			var dist = global_position.distance_to(player.global_position)
			if dist < nearest_dist:
				target = player
				nearest_dist = dist

func take_damage(amount: float, attacker: Node3D = null):
	current_health -= amount
	if current_health <= 0:
		die()
	elif attacker and not target:
		target = attacker
		current_state = State.CHASE

func die():
	queue_free()

# Methods for virus system
func is_bot() -> bool:
	return true

func get_owner_id() -> int:
	return owner_peer_id

func transfer_ownership(new_owner_id: int):
	owner_peer_id = new_owner_id
	# Change team color, etc.
	print("%s transferred to player %d" % [name, new_owner_id])

func get_role() -> String:
	return "Base"

func get_health_percent() -> float:
	return (current_health / max_health) * 100.0
