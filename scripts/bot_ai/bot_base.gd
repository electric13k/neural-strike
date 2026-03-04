extends CharacterBody3D
class_name BotBase

@export var data: BotData
@export var target: Node3D

enum State { IDLE, PATROL, CHASE, COMBAT, CAPTURE }
var current_state: State = State.IDLE

@onready var nav_agent = $NavigationAgent3D

func _physics_process(delta):
	match current_state:
		State.IDLE: _idle_logic()
		State.CHASE: _chase_logic()
		State.COMBAT: _combat_logic()

func _idle_logic():
	if target: current_state = State.CHASE

func _chase_logic():
	if not target: 
		current_state = State.IDLE
		return
	nav_agent.target_position = target.global_position
	var next_path_pos = nav_agent.get_next_path_position()
	velocity = (next_path_pos - global_position).normalized() * 5.0 # Base speed
	move_and_slide()
	
	if global_position.distance_to(target.global_position) < 10.0:
		current_state = State.COMBAT

func _combat_logic():
	# Attack target logic
	pass

func take_damage(amount: float):
	# Damage logic
	pass
