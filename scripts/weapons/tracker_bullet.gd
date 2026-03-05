extends Node3D
class_name TrackerBullet

# Tracker Bullet - Smart ammunition that plants camera on hit target
# Every shot creates a temporary camera feed visible on Battle Pad

var shooter = null  # Player who fired this bullet
var target = null  # Entity that was hit
var camera: Camera3D
var lifetime: float = 30.0  # Camera feed duration (seconds)
var timer: float = 0.0
var is_active: bool = false

@onready var camera_gimbal = $CameraGimbal
@onready var tracker_light = $TrackerLight  # Visual indicator LED

func _ready():
	# Create camera for this tracker
	camera = Camera3D.new()
	camera.current = false
	camera_gimbal.add_child(camera)
	
	if tracker_light:
		tracker_light.visible = true

func _process(delta):
	if not is_active:
		return
	
	timer += delta
	
	# Expire after lifetime
	if timer >= lifetime:
		_expire()
		return
	
	# Follow target if still valid
	if target and is_instance_valid(target):
		global_position = target.global_position
		
		# Rotate camera to face forward from target's perspective
		if target.has_method("get_look_direction"):
			var look_dir = target.get_look_direction()
			camera_gimbal.look_at(global_position + look_dir, Vector3.UP)
	else:
		# Target destroyed, expire
		_expire()

func initialize(fired_by, hit_target):
	"""Called when bullet hits target"""
	shooter = fired_by
	target = hit_target
	is_active = true
	
	if target:
		global_position = target.global_position
	
	# Register camera feed with shooter's Battle Pad
	if shooter and shooter.has_method("register_tracker_camera"):
		var tracker_id = "Tracker_" + str(get_instance_id())
		shooter.register_tracker_camera(tracker_id, camera)
		
		print("Tracker bullet planted on: ", target.name if target else "unknown")

func _expire():
	"""Remove camera feed and destroy tracker"""
	is_active = false
	
	# Unregister from Battle Pad
	if shooter and shooter.has_method("unregister_tracker_camera"):
		var tracker_id = "Tracker_" + str(get_instance_id())
		shooter.unregister_tracker_camera(tracker_id)
	
	print("Tracker bullet expired")
	queue_free()

func get_remaining_time() -> float:
	"""Returns remaining camera feed time"""
	return max(0.0, lifetime - timer)

func extend_lifetime(additional_seconds: float):
	"""Extend tracker lifetime (e.g., by shooting same target again)"""
	lifetime += additional_seconds
	print("Tracker lifetime extended by ", additional_seconds, "s")
