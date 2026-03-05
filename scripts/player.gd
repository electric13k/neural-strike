extends CharacterBody3D
class_name Player

# Player controller with health system and 3D healthbar

@export var speed: float = 5.0
@export var sprint_speed: float = 8.0
@export var jump_velocity: float = 4.5
@export var mouse_sensitivity: float = 0.002
@export var max_health: float = 100.0
@export var team_id: int = 0  # 0 = team A, 1 = team B

var current_health: float = 100.0
var is_sprinting: bool = false

@onready var head: Node3D = $Head if has_node("Head") else null
@onready var camera: Camera3D = $Head/Camera3D if has_node("Head/Camera3D") else null
@onready var healthbar: HealthBar3D = $HealthBar3D if has_node("HealthBar3D") else null

func _ready():
	current_health = max_health
	add_to_group("players")
	add_to_group("local_player")  # For respawn system
	
	# Set up healthbar
	if not healthbar:
		# Create healthbar if not in scene
		healthbar = preload("res://scripts/ui/healthbar_3d.gd").new()
		add_child(healthbar)
	
	healthbar.max_health = max_health
	healthbar.current_health = current_health
	healthbar.is_enemy = false  # Player is always friendly to self
	healthbar.offset_y = 2.2  # Above head
	
	# Capture mouse
	if is_multiplayer_authority():
		Input.mouse_mode = Input.MOUSE_MODE_CAPTURED

func _input(event):
	if not is_multiplayer_authority():
		return
	
	if event is InputEventMouseMotion and Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
		if head:
			# Rotate body horizontally
			rotate_y(-event.relative.x * mouse_sensitivity)
			# Rotate head vertically
			head.rotate_x(-event.relative.y * mouse_sensitivity)
			head.rotation.x = clamp(head.rotation.x, -PI/2, PI/2)

func _physics_process(delta):
	if not is_multiplayer_authority():
		return
	
	# Gravity
	if not is_on_floor():
		velocity.y -= 9.8 * delta
	
	# Jump
	if Input.is_action_just_pressed("jump") and is_on_floor():
		velocity.y = jump_velocity
	
	# Sprint
	is_sprinting = Input.is_action_pressed("sprint")
	var current_speed = sprint_speed if is_sprinting else speed
	
	# Movement
	var input_dir = Input.get_vector("move_left", "move_right", "move_forward", "move_back")
	var direction = (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
	
	if direction:
		velocity.x = direction.x * current_speed
		velocity.z = direction.z * current_speed
	else:
		velocity.x = move_toward(velocity.x, 0, current_speed)
		velocity.z = move_toward(velocity.z, 0, current_speed)
	
	move_and_slide()

func take_damage(amount: float, attacker: Node3D = null):
	"""Take damage and update healthbar."""
	current_health -= amount
	current_health = clamp(current_health, 0, max_health)
	
	if healthbar:
		healthbar.update_health(current_health)
	
	if current_health <= 0:
		die(attacker)
	
	print("[Player] Took %d damage, health: %d/%d" % [amount, current_health, max_health])

func heal(amount: float):
	"""Heal player and update healthbar."""
	current_health += amount
	current_health = clamp(current_health, 0, max_health)
	
	if healthbar:
		healthbar.update_health(current_health)
	
	print("[Player] Healed %d, health: %d/%d" % [amount, current_health, max_health])

func reset_health():
	"""Reset to full health (used on respawn)."""
	current_health = max_health
	if healthbar:
		healthbar.update_health(current_health)

func die(killer: Node3D = null):
	"""Handle player death."""
	print("[Player] Died")
	
	# Notify game manager for scoring
	if killer and killer.has_method("get_peer_id"):
		var killer_id = killer.get_peer_id()
		var victim_id = multiplayer.get_unique_id()
		GameManager.on_player_kill(killer_id, victim_id)
	
	# Hide player temporarily (respawn handles visibility)
	visible = false
	# Respawn handled by GameManager

func get_peer_id() -> int:
	"""Get this player's network peer ID."""
	return multiplayer.get_unique_id()

func get_team_id() -> int:
	"""Get team ID for team-based modes."""
	return team_id

func set_team(new_team_id: int):
	"""Set team and update healthbar color for other players."""
	team_id = new_team_id
	# Healthbar color is relative - handled by other players' view
