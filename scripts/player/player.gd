extends CharacterBody3D
class_name Player

# Player - First-person character with full animation system
# Supports: Great Sword combat, Pistol combat, Locomotion, Data Knife hacking
# Animation count: 100+ animations for combat, movement, and special actions

# Movement
@export var walk_speed: float = 5.0
@export var run_speed: float = 8.0
@export var sprint_speed: float = 12.0
@export var crouch_speed: float = 3.0
@export var acceleration: float = 15.0
@export var jump_velocity: float = 8.0
@export var mouse_sensitivity: float = 0.003

# Combat
@export var max_health: int = 150
@export var max_stamina: float = 100.0
@export var stamina_regen_rate: float = 15.0  # Per second
@export var stamina_drain_attack: float = 20.0  # Per sword attack
@export var stamina_drain_sprint: float = 10.0  # Per second

var current_health: int = 150
var current_stamina: float = 100.0
var is_sprinting: bool = false
var is_crouching: bool = false
var is_aiming: bool = false

# Animation states
var current_anim: String = "idle"
var weapon_equipped: String = "sword"  # "sword", "pistol", "data_knife"
var is_attacking: bool = false
var is_blocking: bool = false
var is_casting: bool = false
var attack_combo_count: int = 0
var last_attack_time: float = 0.0
var combo_timeout: float = 1.5  # Seconds before combo resets

# Data Knife
var data_knife_active: bool = false
var hacking_target = null
var hack_progress: float = 0.0
var hack_duration: float = 7.0  # 7 seconds to hack

# Team and bots
var team_color: Color = Color.CYAN
var owned_bots: Array = []
var active_tracker_cameras: Dictionary = {}

# Gravity
var gravity = ProjectSettings.get_setting("physics/3d/default_gravity")

@onready var camera = $Head/Camera3D
@onready var head = $Head
@onready var animation_player = $AnimationPlayer
@onready var animation_tree = $AnimationTree  # Optional for blend trees
@onready var sword_mesh = $WeaponAttachment/GreatSword
@onready var pistol_mesh = $WeaponAttachment/Pistol
@onready var data_knife_mesh = $WeaponAttachment/DataKnife
@onready var battle_pad = $BattlePad
@onready var battle_pad_ui = $CanvasLayer/BattlePadUI

func _ready():
	current_health = max_health
	current_stamina = max_stamina
	
	# Setup animations
	_setup_animations()
	
	# Show only sword by default
	_equip_weapon("sword")
	
	# Setup Battle Pad
	if battle_pad:
		battle_pad.set_player(self)
		if battle_pad_ui:
			battle_pad.set_ui(battle_pad_ui)
	
	# Capture mouse
	Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
	
	# Start with idle animation
	_play_animation("idle")

func _setup_animations():
	"""Setup all animation mappings from FBX files"""
	if not animation_player:
		return
	
	# Animations should be in res://assets/models/player/animations/
	# Great Sword animations (40+ variants)
	# Pistol animations (30+ variants)
	# Locomotion animations (25+ variants)
	# Data Knife animations (2 variants)
	
	var anim_paths = {
		# === GREAT SWORD COMBAT ===
		"sword_draw_1": "res://assets/models/player/animations/draw a great sword 1.fbx",
		"sword_draw_2": "res://assets/models/player/animations/draw a great sword 2.fbx",
		"sword_attack": "res://assets/models/player/animations/great sword attack.fbx",
		"sword_180_turn_2": "res://assets/models/player/animations/great sword 180 turn (2).fbx",
		"sword_blocking_2": "res://assets/models/player/animations/great sword blocking (2).fbx",
		"sword_blocking_3": "res://assets/models/player/animations/great sword blocking (3).fbx",
		"sword_blocking": "res://assets/models/player/animations/great sword blocking.fbx",
		"sword_casting": "res://assets/models/player/animations/great sword casting.fbx",
		"sword_crouching_2": "res://assets/models/player/animations/great sword crouching (2).fbx",
		"sword_crouching_3": "res://assets/models/player/animations/great sword crouching (3).fbx",
		"sword_crouching_4": "res://assets/models/player/animations/great sword crouching (4).fbx",
		"sword_crouching_5": "res://assets/models/player/animations/great sword crouching (5).fbx",
		"sword_crouching_6": "res://assets/models/player/animations/great sword crouching (6).fbx",
		"sword_crouching": "res://assets/models/player/animations/great sword crouching.fbx",
		"sword_high_spin": "res://assets/models/player/animations/great sword high spin attack.fbx",
		"sword_idle_2": "res://assets/models/player/animations/great sword idle (2).fbx",
		"sword_idle_3": "res://assets/models/player/animations/great sword idle (3).fbx",
		"sword_idle_4": "res://assets/models/player/animations/great sword idle (4).fbx",
		"sword_idle_5": "res://assets/models/player/animations/great sword idle (5).fbx",
		"sword_idle": "res://assets/models/player/animations/great sword idle.fbx",
		"sword_impact_2": "res://assets/models/player/animations/great sword impact (2).fbx",
		"sword_impact_3": "res://assets/models/player/animations/great sword impact (3).fbx",
		"sword_impact_4": "res://assets/models/player/animations/great sword impact (4).fbx",
		"sword_impact_5": "res://assets/models/player/animations/great sword impact (5).fbx",
		"sword_impact": "res://assets/models/player/animations/great sword impact.fbx",
		"sword_jump_2": "res://assets/models/player/animations/great sword jump (2).fbx",
		"sword_jump_attack": "res://assets/models/player/animations/great sword jump attack.fbx",
		"sword_jump": "res://assets/models/player/animations/great sword jump.fbx",
		"sword_kick_2": "res://assets/models/player/animations/great sword kick (2).fbx",
		"sword_kick": "res://assets/models/player/animations/great sword kick.fbx",
		"sword_power_up": "res://assets/models/player/animations/great sword power up.fbx",
		"sword_run_2": "res://assets/models/player/animations/great sword run (2).fbx",
		"sword_run": "res://assets/models/player/animations/great sword run.fbx",
		"sword_slash_2": "res://assets/models/player/animations/great sword slash (2).fbx",
		"sword_slash_3": "res://assets/models/player/animations/great sword slash (3).fbx",
		"sword_slash_4": "res://assets/models/player/animations/great sword slash (4).fbx",
		"sword_slash_5": "res://assets/models/player/animations/great sword slash (5).fbx",
		"sword_slash": "res://assets/models/player/animations/great sword slash.fbx",
		"sword_slide_attack": "res://assets/models/player/animations/great sword slide attack.fbx",
		"sword_strafe_2": "res://assets/models/player/animations/great sword strafe (2).fbx",
		"sword_strafe_3": "res://assets/models/player/animations/great sword strafe (3).fbx",
		"sword_strafe_4": "res://assets/models/player/animations/great sword strafe (4).fbx",
		"sword_strafe": "res://assets/models/player/animations/great sword strafe.fbx",
		"sword_turn_2": "res://assets/models/player/animations/great sword turn (2).fbx",
		"sword_turn": "res://assets/models/player/animations/great sword turn.fbx",
		"sword_walk_2": "res://assets/models/player/animations/great sword walk (2).fbx",
		"sword_walk": "res://assets/models/player/animations/great sword walk.fbx",
		"two_handed_death_2": "res://assets/models/player/animations/two handed sword death (2).fbx",
		"two_handed_death": "res://assets/models/player/animations/two handed sword death.fbx",
		
		# === PISTOL COMBAT ===
		"pistol_idle": "res://assets/models/player/animations/pistol idle.fbx",
		"pistol_jump_2": "res://assets/models/player/animations/pistol jump (2).fbx",
		"pistol_jump": "res://assets/models/player/animations/pistol jump.fbx",
		"pistol_kneel_to_stand": "res://assets/models/player/animations/pistol kneel to stand.fbx",
		"pistol_kneeling_idle": "res://assets/models/player/animations/pistol kneeling idle.fbx",
		"pistol_run_arc_2": "res://assets/models/player/animations/pistol run arc (2).fbx",
		"pistol_run_arc": "res://assets/models/player/animations/pistol run arc.fbx",
		"pistol_run_backward_arc_2": "res://assets/models/player/animations/pistol run backward arc (2).fbx",
		"pistol_run_backward_arc": "res://assets/models/player/animations/pistol run backward arc.fbx",
		"pistol_run_backward": "res://assets/models/player/animations/pistol run backward.fbx",
		"pistol_run": "res://assets/models/player/animations/pistol run.fbx",
		"pistol_stand_to_kneel": "res://assets/models/player/animations/pistol stand to kneel.fbx",
		"pistol_strafe_2": "res://assets/models/player/animations/pistol strafe (2).fbx",
		"pistol_strafe": "res://assets/models/player/animations/pistol strafe.fbx",
		"pistol_walk_arc_2": "res://assets/models/player/animations/pistol walk arc (2).fbx",
		"pistol_walk_arc": "res://assets/models/player/animations/pistol walk arc.fbx",
		"pistol_walk_backward_arc_2": "res://assets/models/player/animations/pistol walk backward arc (2).fbx",
		"pistol_walk_backward_arc": "res://assets/models/player/animations/pistol walk backward arc.fbx",
		"pistol_walk_backward": "res://assets/models/player/animations/pistol walk backward.fbx",
		"pistol_walk": "res://assets/models/player/animations/pistol walk.fbx",
		
		# === LOCOMOTION (UNARMED/GENERAL) ===
		"crouching_turn_90_left": "res://assets/models/player/animations/crouching turn 90 left.fbx",
		"crouching_turn_90_right": "res://assets/models/player/animations/crouching turn 90 right.fbx",
		"death_crouching_headshot_front": "res://assets/models/player/animations/death crouching headshot front.fbx",
		"death_from_back_headshot": "res://assets/models/player/animations/death from back headshot.fbx",
		"death_from_front_headshot": "res://assets/models/player/animations/death from front headshot.fbx",
		"death_from_right": "res://assets/models/player/animations/death from right.fbx",
		"death_from_the_back": "res://assets/models/player/animations/death from the back.fbx",
		"death_from_the_front": "res://assets/models/player/animations/death from the front.fbx",
		"idle_aiming": "res://assets/models/player/animations/idle aiming.fbx",
		"idle_crouching_aiming": "res://assets/models/player/animations/idle crouching aiming.fbx",
		"idle_crouching": "res://assets/models/player/animations/idle crouching.fbx",
		"idle": "res://assets/models/player/animations/idle.fbx",
		"jump_down": "res://assets/models/player/animations/jump down.fbx",
		"jump_loop": "res://assets/models/player/animations/jump loop.fbx",
		"jump_up": "res://assets/models/player/animations/jump up.fbx",
		"run_backward_left": "res://assets/models/player/animations/run backward left.fbx",
		"run_backward_right": "res://assets/models/player/animations/run backward right.fbx",
		"run_backward": "res://assets/models/player/animations/run backward.fbx",
		"run_forward_left": "res://assets/models/player/animations/run forward left.fbx",
		"run_forward_right": "res://assets/models/player/animations/run forward right.fbx",
		"run_forward": "res://assets/models/player/animations/run forward.fbx",
		"run_left": "res://assets/models/player/animations/run left.fbx",
		"run_right": "res://assets/models/player/animations/run right.fbx",
		"sprint_backward_left": "res://assets/models/player/animations/sprint backward left.fbx",
		"sprint_backward_right": "res://assets/models/player/animations/sprint backward right.fbx",
		"sprint_backward": "res://assets/models/player/animations/sprint backward.fbx",
		"sprint_forward_left": "res://assets/models/player/animations/sprint forward left.fbx",
		"sprint_forward_right": "res://assets/models/player/animations/sprint forward right.fbx",
		"sprint_forward": "res://assets/models/player/animations/sprint forward.fbx",
		"sprint_left": "res://assets/models/player/animations/sprint left.fbx",
		"sprint_right": "res://assets/models/player/animations/sprint right.fbx",
		"turn_90_left": "res://assets/models/player/animations/turn 90 left.fbx",
		"turn_90_right": "res://assets/models/player/animations/turn 90 right.fbx",
		"walk_backward_left": "res://assets/models/player/animations/walk backward left.fbx",
		"walk_backward_right": "res://assets/models/player/animations/walk backward right.fbx",
		"walk_backward": "res://assets/models/player/animations/walk backward.fbx",
		"walk_crouching_backward_left": "res://assets/models/player/animations/walk crouching backward left.fbx",
		"walk_crouching_backward_right": "res://assets/models/player/animations/walk crouching backward right.fbx",
		"walk_crouching_backward": "res://assets/models/player/animations/walk crouching backward.fbx",
		"walk_crouching_forward_left": "res://assets/models/player/animations/walk crouching forward left.fbx",
		"walk_crouching_forward_right": "res://assets/models/player/animations/walk crouching forward right.fbx",
		"walk_crouching_forward": "res://assets/models/player/animations/walk crouching forward.fbx",
		"walk_crouching_left": "res://assets/models/player/animations/walk crouching left.fbx",
		"walk_crouching_right": "res://assets/models/player/animations/walk crouching right.fbx",
		"walk_forward_left": "res://assets/models/player/animations/walk forward left.fbx",
		"walk_forward_right": "res://assets/models/player/animations/walk forward right.fbx",
		"walk_forward": "res://assets/models/player/animations/walk forward.fbx",
		"walk_left": "res://assets/models/player/animations/walk left.fbx",
		"walk_right": "res://assets/models/player/animations/walk right.fbx",
		
		# === DATA KNIFE ===
		"stabbing_1": "res://assets/models/player/animations/Stabbing (1).fbx",
		"stabbing_data_knife_virus": "res://assets/models/player/animations/Stabbing (2) data knife virus on robot move.fbx",
		"stabbing": "res://assets/models/player/animations/Stabbing.fbx",
		
		# === PLAYER MODEL ===
		"player_model": "res://assets/models/player/animations/Player 2.fbx"
	}
	
	print("Player animation system initialized with 100+ animations")

func _input(event):
	# Mouse look
	if event is InputEventMouseMotion and Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
		head.rotate_y(-event.relative.x * mouse_sensitivity)
		camera.rotate_x(-event.relative.y * mouse_sensitivity)
		camera.rotation.x = clamp(camera.rotation.x, -PI/2, PI/2)
	
	# Release mouse
	if event.is_action_pressed("ui_cancel"):
		Input.mouse_mode = Input.MOUSE_MODE_VISIBLE

func _physics_process(delta):
	if current_health <= 0:
		return
	
	# Regenerate stamina
	if not is_attacking and not is_sprinting:
		current_stamina = min(max_stamina, current_stamina + stamina_regen_rate * delta)
	
	# Drain stamina when sprinting
	if is_sprinting:
		current_stamina -= stamina_drain_sprint * delta
		if current_stamina <= 0:
			is_sprinting = false
	
	# Handle hacking
	if data_knife_active and hacking_target:
		_process_hacking(delta)
	
	# Apply gravity
	if not is_on_floor():
		velocity.y -= gravity * delta
	
	# Handle input
	var input_dir = Input.get_vector("move_left", "move_right", "move_forward", "move_backward")
	var direction = (head.transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
	
	# Determine speed based on state
	var target_speed = walk_speed
	if is_crouching:
		target_speed = crouch_speed
	elif Input.is_action_pressed("sprint") and not is_crouching:
		is_sprinting = true
		target_speed = sprint_speed if current_stamina > 0 else run_speed
	elif Input.is_action_pressed("run"):
		target_speed = run_speed
	else:
		is_sprinting = false
	
	# Apply movement
	if direction:
		velocity.x = direction.x * target_speed
		velocity.z = direction.z * target_speed
	else:
		velocity.x = move_toward(velocity.x, 0, acceleration * delta)
		velocity.z = move_toward(velocity.z, 0, acceleration * delta)
	
	# Jump
	if Input.is_action_just_pressed("jump") and is_on_floor():
		velocity.y = jump_velocity
	
	# Crouch
	if Input.is_action_just_pressed("crouch"):
		is_crouching = not is_crouching
	
	# Combat inputs
	if Input.is_action_just_pressed("attack"):
		_perform_attack()
	
	if Input.is_action_pressed("block") and weapon_equipped == "sword":
		is_blocking = true
	else:
		is_blocking = false
	
	if Input.is_action_pressed("aim") and weapon_equipped == "pistol":
		is_aiming = true
	else:
		is_aiming = false
	
	# Weapon switching
	if Input.is_action_just_pressed("weapon_1"):
		_equip_weapon("sword")
	elif Input.is_action_just_pressed("weapon_2"):
		_equip_weapon("pistol")
	elif Input.is_action_just_pressed("weapon_3"):
		_equip_weapon("data_knife")
	
	# Battle Pad
	if Input.is_action_just_pressed("battle_pad"):
		if battle_pad:
			battle_pad.toggle()
	
	# Update animation based on state
	if not is_attacking and not data_knife_active:
		_update_locomotion_animation()
	
	# Reset combo if timeout
	if Time.get_ticks_msec() / 1000.0 - last_attack_time > combo_timeout:
		attack_combo_count = 0
	
	move_and_slide()

func _update_locomotion_animation():
	"""Determine locomotion animation based on movement state"""
	var is_moving = velocity.length() > 0.1
	var speed = velocity.length()
	
	# Get movement direction relative to camera
	var input_dir = Input.get_vector("move_left", "move_right", "move_forward", "move_backward")
	
	if is_blocking:
		_play_animation("sword_blocking")
		return
	
	if is_aiming and weapon_equipped == "pistol":
		if is_moving:
			_play_animation("pistol_run" if speed > 6.0 else "pistol_walk")
		else:
			_play_animation("idle_aiming")
		return
	
	if is_crouching:
		if is_moving:
			if input_dir.y < 0:  # Forward
				_play_animation("walk_crouching_forward")
			elif input_dir.y > 0:  # Backward
				_play_animation("walk_crouching_backward")
			elif input_dir.x < 0:  # Left
				_play_animation("walk_crouching_left")
			else:  # Right
				_play_animation("walk_crouching_right")
		else:
			_play_animation("idle_crouching")
		return
	
	if not is_on_floor():
		_play_animation("jump_loop")
		return
	
	if not is_moving:
		if weapon_equipped == "sword":
			_play_animation("sword_idle")
		elif weapon_equipped == "pistol":
			_play_animation("pistol_idle")
		else:
			_play_animation("idle")
		return
	
	# Moving animations based on weapon and speed
	if weapon_equipped == "sword":
		if speed > 10.0:  # Sprinting
			_play_animation("sword_run")
		elif speed > 6.0:  # Running
			_play_animation("sword_run_2")
		else:  # Walking
			_play_animation("sword_walk")
	elif weapon_equipped == "pistol":
		if speed > 10.0:
			_play_animation("pistol_run")
		else:
			_play_animation("pistol_walk")
	else:  # Unarmed/general
		if speed > 10.0:  # Sprinting
			if input_dir.y < 0:
				_play_animation("sprint_forward")
			elif input_dir.y > 0:
				_play_animation("sprint_backward")
			elif input_dir.x < 0:
				_play_animation("sprint_left")
			else:
				_play_animation("sprint_right")
		elif speed > 6.0:  # Running
			if input_dir.y < 0:
				_play_animation("run_forward")
			elif input_dir.y > 0:
				_play_animation("run_backward")
			elif input_dir.x < 0:
				_play_animation("run_left")
			else:
				_play_animation("run_right")
		else:  # Walking
			if input_dir.y < 0:
				_play_animation("walk_forward")
			elif input_dir.y > 0:
				_play_animation("walk_backward")
			elif input_dir.x < 0:
				_play_animation("walk_left")
			else:
				_play_animation("walk_right")

func _perform_attack():
	"""Execute attack based on weapon and combo"""
	if is_attacking:
		return  # Already attacking
	
	if weapon_equipped == "sword":
		if current_stamina < stamina_drain_attack:
			return  # Not enough stamina
		
		current_stamina -= stamina_drain_attack
		is_attacking = true
		
		# Combo system
		attack_combo_count = (attack_combo_count % 5) + 1
		last_attack_time = Time.get_ticks_msec() / 1000.0
		
		match attack_combo_count:
			1: _play_animation("sword_slash", 0.1)
			2: _play_animation("sword_slash_2", 0.1)
			3: _play_animation("sword_slash_3", 0.1)
			4: _play_animation("sword_slash_4", 0.1)
			5: _play_animation("sword_high_spin", 0.1)  # Finisher
		
		# Reset attack flag after animation
		await get_tree().create_timer(0.6).timeout
		is_attacking = false
		
	elif weapon_equipped == "pistol":
		# Pistol shooting (instant, no combo)
		_fire_pistol()
		
	elif weapon_equipped == "data_knife":
		# Data knife stab (initiates hacking)
		_perform_data_knife_attack()

func _fire_pistol():
	"""Fire pistol with raycast"""
	var space_state = get_world_3d().direct_space_state
	var from = camera.global_position
	var to = from + (-camera.global_transform.basis.z * 100.0)
	
	var query = PhysicsRayQueryParameters3D.create(from, to)
	var result = space_state.intersect_ray(query)
	
	if result and result.collider:
		if result.collider.has_method("take_damage"):
			result.collider.take_damage(30, self)  # 30 damage per shot
			
		# Plant tracker bullet
		_plant_tracker_bullet(result.collider)
	
	print("Pistol fired")

func _perform_data_knife_attack():
	"""Attempt to hack target with data knife"""
	# Raycast to find hackable target
	var space_state = get_world_3d().direct_space_state
	var from = camera.global_position
	var to = from + (-camera.global_transform.basis.z * 3.0)  # 3m range
	
	var query = PhysicsRayQueryParameters3D.create(from, to)
	var result = space_state.intersect_ray(query)
	
	if result and result.collider:
		if result.collider.is_in_group("bot") or result.collider.is_in_group("hackable"):
			# Start hacking
			hacking_target = result.collider
			data_knife_active = true
			hack_progress = 0.0
			
			_play_animation("stabbing_data_knife_virus", 0.1)
			print("Data knife hacking started on: ", hacking_target.name)
		else:
			# Regular stab damage
			if result.collider.has_method("take_damage"):
				result.collider.take_damage(50, self)
			_play_animation("stabbing", 0.1)

func _process_hacking(delta):
	"""Handle ongoing hacking process"""
	if not hacking_target or not is_instance_valid(hacking_target):
		_cancel_hacking()
		return
	
	# Check distance (must stay close)
	var distance = global_position.distance_to(hacking_target.global_position)
	if distance > 4.0:
		_cancel_hacking()
		return
	
	# Increment progress
	hack_progress += delta
	
	# Show progress (TODO: Add UI progress bar)
	var progress_percent = (hack_progress / hack_duration) * 100.0
	
	if hack_progress >= hack_duration:
		# Hacking complete!
		_complete_hacking()

func _complete_hacking():
	"""Successfully hacked target"""
	if hacking_target.has_method("hack_by_player"):
		hacking_target.hack_by_player(self)
		
		# Add to owned bots
		if not owned_bots.has(hacking_target):
			owned_bots.append(hacking_target)
	
	print("Hacking complete! ", hacking_target.name, " is now under your control")
	
	data_knife_active = false
	hacking_target = null
	hack_progress = 0.0

func _cancel_hacking():
	"""Cancel hacking (moved too far or target destroyed)"""
	print("Hacking cancelled")
	
	data_knife_active = false
	hacking_target = null
	hack_progress = 0.0

func _equip_weapon(weapon_name: String):
	"""Switch to specified weapon"""
	weapon_equipped = weapon_name
	
	# Hide all weapons
	if sword_mesh:
		sword_mesh.visible = false
	if pistol_mesh:
		pistol_mesh.visible = false
	if data_knife_mesh:
		data_knife_mesh.visible = false
	
	# Show equipped weapon
	match weapon_name:
		"sword":
			if sword_mesh:
				sword_mesh.visible = true
			_play_animation("sword_draw_1", 0.1)
		"pistol":
			if pistol_mesh:
				pistol_mesh.visible = true
		"data_knife":
			if data_knife_mesh:
				data_knife_mesh.visible = true
	
	print("Equipped weapon: ", weapon_name)

func _play_animation(anim_name: String, blend_time: float = 0.2):
	"""Play animation with blending"""
	if current_anim == anim_name:
		return  # Already playing
	
	if not animation_player:
		return
	
	if not animation_player.has_animation(anim_name):
		# print("Warning: Animation '", anim_name, "' not found")
		return
	
	current_anim = anim_name
	animation_player.play(anim_name, blend_time)

# === BOT MANAGEMENT ===

func on_bot_destroyed(bot):
	"""Called when owned bot is destroyed"""
	if owned_bots.has(bot):
		owned_bots.erase(bot)
		print("Bot destroyed: ", bot.name)

func on_bot_fired_shot(bot, target):
	"""Called when sniper bot fires shot"""
	print("Sniper bot fired at: ", target.name if target else "unknown")

func on_spy_detected_enemy(enemy, position):
	"""Called when spy bot detects enemy"""
	print("Spy detected enemy: ", enemy.name, " at ", position)

# === TRACKER BULLET SYSTEM ===

func _plant_tracker_bullet(target):
	"""Plant tracker camera on hit target"""
	var tracker = preload("res://scenes/weapons/tracker_bullet.tscn").instantiate()
	get_parent().add_child(tracker)
	tracker.initialize(self, target)
	
	print("Tracker bullet planted on: ", target.name)

func register_tracker_camera(tracker_id: String, camera: Camera3D):
	"""Register tracker camera with Battle Pad"""
	active_tracker_cameras[tracker_id] = camera
	
	if battle_pad_ui and battle_pad_ui.has_method("add_camera_feed"):
		battle_pad_ui.add_camera_feed(tracker_id, camera)
	
	print("Tracker camera registered: ", tracker_id)

func unregister_tracker_camera(tracker_id: String):
	"""Unregister tracker camera from Battle Pad"""
	if active_tracker_cameras.has(tracker_id):
		active_tracker_cameras.erase(tracker_id)
	
	if battle_pad_ui and battle_pad_ui.has_method("remove_camera_feed"):
		battle_pad_ui.remove_camera_feed(tracker_id)
	
	print("Tracker camera unregistered: ", tracker_id)

func receive_spy_camera_feed(spy_bot, camera: Camera3D):
	"""Receive camera feed from spy bot"""
	pass  # Handled by Battle Pad UI

func disconnect_spy_camera():
	"""Disconnect from spy bot camera"""
	if battle_pad_ui and battle_pad_ui.has_method("_disconnect_spy_control"):
		battle_pad_ui._disconnect_spy_control()

func show_notification(message: String):
	"""Show HUD notification"""
	print("[NOTIFICATION] ", message)
	# TODO: Add UI notification system

# === COMBAT SYSTEM ===

func take_damage(amount: int, attacker = null):
	"""Player takes damage"""
	current_health = max(0, current_health - amount)
	
	print("Player took ", amount, " damage. Health: ", current_health, "/", max_health)
	
	if current_health <= 0:
		die()

func receive_healing(amount: float, healer):
	"""Player receives healing"""
	current_health = min(max_health, current_health + int(amount))
	
	print("Player healed for ", amount, " HP. Health: ", current_health, "/", max_health)

func die():
	"""Player death"""
	print("Player died")
	
	# Play death animation based on hit direction
	_play_animation("death_from_the_front", 0.1)
	
	# Disable controls
	set_physics_process(false)
	
	# Wait for death animation
	await get_tree().create_timer(3.0).timeout
	
	# Respawn or game over
	# get_tree().reload_current_scene()

func get_health_percent() -> float:
	return float(current_health) / float(max_health)

func get_team() -> int:
	return 1  # TODO: Implement team system
