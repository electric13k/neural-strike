extends CharacterBody3D

# Movement
@export var speed: float = 5.0
@export var jump_velocity: float = 4.5
@export var mouse_sensitivity: float = 0.002

# Health system
@export var max_health: int = 100
var current_health: int = 100

@onready var head = $Head
@onready var camera = $Head/Camera3D
@onready var weapon_mount = $WeaponMount

var current_weapon = null

func _ready():
	Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
	
	# Initialize health
	current_health = max_health
	
	# Load default weapon (if scene exists)
	if ResourceLoader.exists("res://scenes/weapons/assault_rifle.tscn"):
		var rifle_scene = load("res://scenes/weapons/assault_rifle.tscn")
		if rifle_scene:
			current_weapon = rifle_scene.instantiate()
			weapon_mount.add_child(current_weapon)

func _input(event):
	if event is InputEventMouseMotion:
		rotate_y(-event.relative.x * mouse_sensitivity)
		head.rotate_x(-event.relative.y * mouse_sensitivity)
		head.rotation.x = clamp(head.rotation.x, -PI/2, PI/2)

func _physics_process(delta):
	# Movement
	var input_dir = Input.get_vector("move_left", "move_right", "move_forward", "move_back")
	var direction = (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
	
	if direction:
		velocity.x = direction.x * speed
		velocity.z = direction.z * speed
	else:
		velocity.x = 0
		velocity.z = 0
	
	# Jump
	if Input.is_action_just_pressed("jump") and is_on_floor():
		velocity.y = jump_velocity
	
	# Gravity
	if not is_on_floor():
		velocity.y -= 9.8 * delta
	
	move_and_slide()
	
	# Shooting
	if Input.is_action_pressed("shoot") and current_weapon and current_weapon.has_method("fire"):
		current_weapon.fire()

func take_damage(amount: int):
	current_health = max(0, current_health - amount)
	if current_health <= 0:
		die()

func heal(amount: int):
	current_health = min(max_health, current_health + amount)

func die():
	print("Player died!")
	# Add death logic here (respawn, etc.)
