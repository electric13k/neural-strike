extends BotBase
class_name AssaultBot

# Assault Bot - Aggressive combat role
# Rushes enemies and uses weapons at close-medium range

@export var attack_range: float = 15.0
@export var melee_range: float = 3.0
@export var fire_rate: float = 0.5

var last_fire_time: float = 0.0

@onready var weapon_mount: Node3D = null
@onready var raycast: RayCast3D = null

func _ready():
	super._ready()
	if has_node("WeaponMount"):
		weapon_mount = $WeaponMount
	if has_node("RayCast3D"):
		raycast = $RayCast3D

func _physics_process(delta):
	super._physics_process(delta)
	
	# Assault-specific combat logic
	if current_state == State.COMBAT and target:
		var distance = global_position.distance_to(target.global_position)
		
		if distance < melee_range:
			melee_attack()
		elif distance < attack_range:
			shoot()

func shoot():
	"""Fire weapon at target."""
	var current_time = Time.get_ticks_msec() / 1000.0
	if current_time - last_fire_time < fire_rate:
		return
	
	last_fire_time = current_time
	
	if not raycast or not target:
		return
	
	# Aim at target
	raycast.look_at(target.global_position, Vector3.UP)
	raycast.force_raycast_update()
	
	if raycast.is_colliding():
		var hit = raycast.get_collider()
		if hit and hit.has_method("take_damage"):
			hit.take_damage(10.0, self)
			print("[AssaultBot] Hit target for 10 damage")

func melee_attack():
	"""Melee attack when very close."""
	if target and target.has_method("take_damage"):
		target.take_damage(20.0, self)
		print("[AssaultBot] Melee attack for 20 damage")

func get_role() -> String:
	return "Assault"
