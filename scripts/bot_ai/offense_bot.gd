extends BotBase

@export_group("Equipment")
@export var assault_rifle_scene: PackedScene
@export var melee_weapon_scene: PackedScene

@export_group("Aggression")
@export var firing_range: float = 20.0
@export var melee_range: float = 2.5
@export var aggression_factor: float = 1.2

var primary_weapon: WeaponBase
var melee_weapon: Node3D # Replace with MeleeSystem if available

func _ready():
	super._ready()
	_equip_loadout()

func _equip_loadout():
	if assault_rifle_scene:
		primary_weapon = assault_rifle_scene.instantiate()
		add_child(primary_weapon)
	
	if melee_weapon_scene:
		melee_weapon = melee_weapon_scene.instantiate()
		add_child(melee_weapon)

func _physics_process(delta):
	if not target:
		return

	var dist = global_position.distance_to(target.global_position)
	
	if dist <= melee_range:
		_perform_melee()
	elif dist <= firing_range:
		_perform_shooting()
	
	# Move toward target based on aggression
	var direction = (target.global_position - global_position).normalized()
	velocity = direction * speed * aggression_factor
	move_and_slide()

func _perform_shooting():
	if primary_weapon and primary_weapon.has_method("fire"):
		primary_weapon.fire()

func _perform_melee():
	if melee_weapon and melee_weapon.has_method("attack"):
		melee_weapon.attack()
	elif primary_weapon and primary_weapon.has_method("melee_strike"):
		primary_weapon.melee_strike()
