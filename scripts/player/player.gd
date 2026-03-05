extends CharacterBody3D

# Movement
@export var speed: float = 5.0
@export var sprint_speed: float = 8.0
@export var crouch_speed: float = 2.5
@export var jump_velocity: float = 4.5
@export var mouse_sensitivity: float = 0.002

# Health system
@export var max_health: int = 100
var current_health: int = 100

@onready var head = $Head
@onready var camera = $Head/Camera3D
@onready var weapon_mount = $WeaponMount
@onready var animation_controller = $AnimationController

var current_weapon = null
var current_weapon_type: String = "greatsword"  # greatsword, pistol, dataknife, hammer

func _ready():
	Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
	
	# Initialize health
	current_health = max_health
	
	# Setup animation controller
	if animation_controller:
		animation_controller.character_body = self
		animation_controller.set_weapon_type(current_weapon_type)
	
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
	
	# Weapon switching
	if event.is_action_pressed("weapon_1"):
		switch_weapon("greatsword")
	elif event.is_action_pressed("weapon_2"):
		switch_weapon("pistol")
	elif event.is_action_pressed("weapon_3"):
		switch_weapon("dataknife")
	elif event.is_action_pressed("weapon_4"):
		switch_weapon("hammer")

func _physics_process(delta):
	# Get input direction
	var input_dir = Input.get_vector("move_left", "move_right", "move_forward", "move_back")
	
	# Update animation controller with input
	if animation_controller:
		animation_controller.set_movement_input(input_dir)
	
	# Calculate movement speed
	var current_speed = speed
	if Input.is_action_pressed("sprint") and input_dir.y < 0:  # Only sprint forward
		current_speed = sprint_speed
	elif Input.is_action_pressed("crouch"):
		current_speed = crouch_speed
	
	# Movement
	var direction = (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
	
	if direction:
		velocity.x = direction.x * current_speed
		velocity.z = direction.z * current_speed
	else:
		velocity.x = 0
		velocity.z = 0
	
	# Jump
	if Input.is_action_just_pressed("jump") and is_on_floor():
		velocity.y = jump_velocity
		if animation_controller:
			animation_controller.animation_player.play("jump up.fbx")
	
	# Gravity
	if not is_on_floor():
		velocity.y -= 9.8 * delta
	
	move_and_slide()
	
	# Combat actions
	if Input.is_action_just_pressed("shoot"):
		if animation_controller:
			animation_controller.play_attack()
		if current_weapon and current_weapon.has_method("fire"):
			current_weapon.fire()
	
	if Input.is_action_pressed("block") and current_weapon_type == "greatsword":
		if animation_controller:
			animation_controller.play_block()
	
	if Input.is_action_just_pressed("reload") and current_weapon_type == "pistol":
		if animation_controller:
			animation_controller.play_reload()
	
	if Input.is_action_just_pressed("dodge"):
		if animation_controller:
			animation_controller.play_dodge_roll()

func switch_weapon(weapon_type: String):
	current_weapon_type = weapon_type
	if animation_controller:
		animation_controller.set_weapon_type(weapon_type)
	
	# TODO: Swap weapon model in weapon_mount
	print("Switched to: ", weapon_type)

func take_damage(amount: int, from_direction: Vector3 = Vector3.ZERO):
	current_health = max(0, current_health - amount)
	
	if current_health <= 0:
		die(from_direction)

func heal(amount: int):
	current_health = min(max_health, current_health + amount)

func die(from_direction: Vector3 = Vector3.ZERO):
	# Determine death direction
	var dir = "front"
	if from_direction != Vector3.ZERO:
		var local_dir = global_transform.basis.inverse() * from_direction.normalized()
		if abs(local_dir.z) > abs(local_dir.x):
			dir = "front" if local_dir.z < 0 else "back"
		else:
			dir = "left" if local_dir.x < 0 else "right"
	
	if animation_controller:
		animation_controller.play_death(dir)
	
	# Disable controls
	set_physics_process(false)
	set_process_input(false)
	
	print("Player died!")
	# TODO: Respawn logic
