extends Node3D

@export var damage: float = 40.0
@export var hacking_time: float = 3.0
@export var range: float = 1.5

@onready var raycast = $RayCast3D

func attack():
	if raycast.is_colliding():
		var target = raycast.get_collider()
		if target.has_method("take_damage"):
			target.take_damage(damage)
		
		# Hacking / Virus Logic
		if target.is_in_group("base_console") or target.is_in_group("hackable"):
			start_hacking(target)

func start_hacking(target):
	print("Hacking started on: ", target.name)
	await get_tree().create_timer(hacking_time).timeout
	if target.has_method("on_hacked"):
		target.on_hacked()
