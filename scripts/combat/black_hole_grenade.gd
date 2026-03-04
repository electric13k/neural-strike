extends RigidBody3D

@export var pull_strength: float = 20.0
@export var duration: float = 5.0
@export var radius: float = 10.0

@onready var area = $Area3D

func _ready():
	await get_tree().create_timer(3.0).timeout # Fuse time
	activate()

func activate():
	# Pull logic for black hole
	var timer = get_tree().create_timer(duration)
	while timer.time_left > 0:
		for body in area.get_overlapping_bodies():
			if body is CharacterBody3D or body is RigidBody3D:
				var dir = (global_position - body.global_position).normalized()
				if body is CharacterBody3D:
					body.velocity += dir * pull_strength * 0.1
				else:
					body.apply_central_force(dir * pull_strength)
		await get_tree().process_frame
	queue_free()
