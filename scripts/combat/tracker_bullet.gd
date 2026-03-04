extends Node3D
class_name TrackerBullet

# Tracker Bullet - Rare ammo with embedded camera
# Shows POV from hit body part via Battle Pad

@export var camera_duration: float = 30.0
@export var camera_fov: float = 70.0

var shooter_id: int = -1
var hit_body_part: String = ""
var active_camera: Camera3D = null

signal tracker_hit(shooter_id, target, body_part, camera)

func initialize(shooter: int):
	shooter_id = shooter

func on_hit(target: Node3D, hit_position: Vector3, hit_normal: Vector3):
	# Determine body part based on hit height relative to target
	var target_height = target.global_position.y
	var hit_height = hit_position.y
	var relative_height = (hit_height - target_height) / 2.0  # Assuming ~2m character
	
	if relative_height > 0.8:
		hit_body_part = "head"
	elif relative_height > 0.5:
		hit_body_part = "torso"
	elif relative_height > 0.2:
		hit_body_part = "leg"
	else:
		hit_body_part = "foot"
	
	# Create tracker camera at hit location
	active_camera = Camera3D.new()
	active_camera.name = "TrackerCam_%d" % shooter_id
	active_camera.fov = camera_fov
	active_camera.position = hit_position
	
	# Attach to target so it moves with them
	target.add_child(active_camera)
	
	# Orient based on body part for realistic POV
	if hit_body_part == "head":
		active_camera.rotation_degrees = Vector3(-10, 0, 0)  # Slight downward
	elif hit_body_part == "leg" or hit_body_part == "foot":
		active_camera.rotation_degrees = Vector3(45, 0, 0)  # Upward view
	
	tracker_hit.emit(shooter_id, target, hit_body_part, active_camera)
	
	# Register with BattlePad if it exists
	var battle_pad = get_tree().get_first_node_in_group("battle_pad")
	if battle_pad:
		battle_pad.register_camera(
			"tracker_%d_%s" % [shooter_id, hit_body_part],
			active_camera,
			"Tracker Bullet"
		)
	
	# Auto-cleanup after duration
	await get_tree().create_timer(camera_duration).timeout
	if active_camera:
		if battle_pad:
			battle_pad.unregister_camera("tracker_%d_%s" % [shooter_id, hit_body_part])
		active_camera.queue_free()
		active_camera = null

func get_camera_feed() -> Camera3D:
	return active_camera
