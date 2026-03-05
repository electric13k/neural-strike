extends Node
class_name AnimationController

# References
@export var animation_player: AnimationPlayer
@export var character_body: CharacterBody3D

# Animation state
var current_weapon_type: String = "greatsword"  # greatsword, pistol, dataknife, hammer
var is_aiming: bool = false
var is_crouching: bool = false
var is_sprinting: bool = false

# Movement state tracking
var movement_direction: Vector2 = Vector2.ZERO
var is_moving: bool = false
var is_moving_backward: bool = false

func _ready():
	if not animation_player:
		push_error("AnimationController: No AnimationPlayer assigned!")
		return
	
	if not character_body:
		push_error("AnimationController: No CharacterBody3D assigned!")
		return

func _process(_delta):
	if not animation_player or not character_body:
		return
	
	# Update movement state
	_update_movement_state()
	
	# Play appropriate animation based on current state
	_update_animation()

# Called by player script to update movement direction
func set_movement_input(input_dir: Vector2):
	movement_direction = input_dir
	is_moving = input_dir.length() > 0.1
	is_moving_backward = input_dir.y > 0.1  # Forward is negative in Godot

# Called by player script when weapon changes
func set_weapon_type(weapon: String):
	current_weapon_type = weapon

# Called by player script for actions
func play_attack():
	match current_weapon_type:
		"greatsword":
			_play_greatsword_attack()
		"pistol":
			_play_pistol_shoot()
		"dataknife", "hammer":
			_play_stab_attack()

func play_reload():
	if current_weapon_type == "pistol":
		animation_player.play("pistol walk backward.fbx")

func play_block():
	if current_weapon_type == "greatsword":
		if is_moving:
			animation_player.play("great sword blocking (2).fbx")
		else:
			animation_player.play("great sword blocking.fbx")

func play_dodge_roll():
	if movement_direction.x < 0:
		animation_player.play("run backward left.fbx")
	elif movement_direction.x > 0:
		animation_player.play("run backward right.fbx")
	else:
		animation_player.play("run backward.fbx")

func play_death(from_direction: String = "front"):
	var death_anims = {
		"front": "death from the front.fbx",
		"back": "death from the back.fbx",
		"left": "death from left.fbx",
		"right": "death from right.fbx",
		"headshot_front": "death crouching headshot front.fbx",
		"headshot_back": "death from back headshot.fbx"
	}
	
	if death_anims.has(from_direction):
		animation_player.play(death_anims[from_direction])
	else:
		animation_player.play("death from the front.fbx")

# Internal animation logic
func _update_movement_state():
	if not character_body:
		return
	
	var velocity = character_body.velocity
	is_sprinting = Input.is_action_pressed("sprint") and is_moving and not is_moving_backward
	is_crouching = Input.is_action_pressed("crouch")
	is_aiming = Input.is_action_pressed("aim") and current_weapon_type in ["pistol", "greatsword"]

func _update_animation():
	if animation_player.is_playing() and not _is_looping_animation(animation_player.current_animation):
		return  # Don't interrupt non-looping animations
	
	# Priority: Death > Attack > Special > Movement
	if is_crouching:
		_play_crouch_animation()
	elif is_aiming:
		_play_aim_animation()
	elif is_sprinting:
		_play_sprint_animation()
	elif is_moving:
		_play_walk_animation()
	else:
		_play_idle_animation()

func _play_idle_animation():
	match current_weapon_type:
		"greatsword":
			animation_player.play("great sword idle.fbx")
		"pistol":
			animation_player.play("pistol idle.fbx")
		"dataknife", "hammer":
			animation_player.play("idle.fbx")

func _play_walk_animation():
	var forward = movement_direction.y < -0.1
	var backward = movement_direction.y > 0.1
	var left = movement_direction.x < -0.1
	var right = movement_direction.x > 0.1
	
	match current_weapon_type:
		"greatsword":
			if is_crouching:
				if forward and left:
					animation_player.play("walk crouching forward left.fbx")
				elif forward and right:
					animation_player.play("walk crouching forward right.fbx")
				elif backward and left:
					animation_player.play("walk crouching backward left.fbx")
				elif backward and right:
					animation_player.play("walk crouching backward right.fbx")
				elif forward:
					animation_player.play("walk crouching forward.fbx")
				elif backward:
					animation_player.play("walk crouching backward.fbx")
				elif left:
					animation_player.play("walk crouching left.fbx")
				elif right:
					animation_player.play("walk crouching right.fbx")
			else:
				if forward and left:
					animation_player.play("walk forward left.fbx")
				elif forward and right:
					animation_player.play("walk forward right.fbx")
				elif backward and left:
					animation_player.play("walk backward left.fbx")
				elif backward and right:
					animation_player.play("walk backward right.fbx")
				elif forward:
					animation_player.play("great sword walk.fbx")
				elif backward:
					animation_player.play("walk backward.fbx")
				elif left:
					animation_player.play("walk left.fbx")
				elif right:
					animation_player.play("walk right.fbx")
		
		"pistol":
			if forward and left:
				animation_player.play("pistol walk arc (2).fbx")
			elif forward and right:
				animation_player.play("pistol walk arc.fbx")
			elif backward and left:
				animation_player.play("pistol walk backward arc (2).fbx")
			elif backward and right:
				animation_player.play("pistol walk backward arc.fbx")
			elif forward:
				animation_player.play("pistol walk.fbx")
			elif backward:
				animation_player.play("pistol walk backward.fbx")
		
		"dataknife", "hammer":
			if forward:
				animation_player.play("walk forward.fbx")
			elif backward:
				animation_player.play("walk backward.fbx")
			elif left:
				animation_player.play("walk left.fbx")
			elif right:
				animation_player.play("walk right.fbx")

func _play_sprint_animation():
	var left = movement_direction.x < -0.1
	var right = movement_direction.x > 0.1
	
	if left:
		animation_player.play("sprint forward left.fbx")
	elif right:
		animation_player.play("sprint forward right.fbx")
	else:
		animation_player.play("sprint forward.fbx")

func _play_crouch_animation():
	if is_moving:
		_play_walk_animation()  # Already handles crouching
	else:
		if is_aiming:
			animation_player.play("idle crouching aiming.fbx")
		else:
			animation_player.play("idle crouching.fbx")

func _play_aim_animation():
	match current_weapon_type:
		"pistol":
			if is_crouching:
				animation_player.play("idle crouching aiming.fbx")
			else:
				animation_player.play("idle aiming.fbx")
		"greatsword":
			animation_player.play("great sword idle (2).fbx")  # Guard stance

func _play_greatsword_attack():
	var attacks = [
		"great sword attack.fbx",
		"draw a great sword 1.fbx",
		"draw a great sword 2.fbx",
		"great sword slash.fbx",
		"great sword slash (2).fbx",
		"great sword slash (3).fbx",
		"great sword slash (4).fbx",
		"great sword slash (5).fbx"
	]
	var random_attack = attacks[randi() % attacks.size()]
	animation_player.play(random_attack)

func _play_pistol_shoot():
	if is_aiming:
		animation_player.play("pistol strafe.fbx")
	else:
		animation_player.play("pistol run.fbx")

func _play_stab_attack():
	# Used for both data knife and hammer
	var stab_anims = [
		"Stabbing (1).fbx",
		"Stabbing (2) data knife virus on robot move.fbx",
		"Stabbing.fbx"
	]
	var random_stab = stab_anims[randi() % stab_anims.size()]
	animation_player.play(random_stab)

func _is_looping_animation(anim_name: String) -> bool:
	# Looping animations can be interrupted, non-looping ones finish first
	var non_looping = [
		"attack", "slash", "shoot", "stab", "death", "jump", "kick",
		"block", "draw", "impact", "power up", "cast"
	]
	
	for keyword in non_looping:
		if keyword in anim_name.to_lower():
			return false
	
	return true
