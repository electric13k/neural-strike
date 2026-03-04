extends Node3D
class_name Weapon

# Base class for all weapons
# Handles firing, reloading, and ammo management

@export var weapon_name: String = "Weapon"
@export var max_ammo: int = 30
@export var reload_time: float = 1.5
@export var fire_rate: float = 0.1  # Time between shots
@export var damage: float = 10.0
@export var range: float = 100.0

var current_ammo: int = 30
var is_reloading: bool = false
var can_fire: bool = true

signal fired()
signal reloaded()
signal ammo_changed(current, max)

func _ready():
	current_ammo = max_ammo
	ammo_changed.emit(current_ammo, max_ammo)

func fire() -> bool:
	"""Attempt to fire the weapon. Returns true if fired successfully."""
	if current_ammo > 0 and not is_reloading and can_fire:
		current_ammo -= 1
		ammo_changed.emit(current_ammo, max_ammo)
		fired.emit()
		
		# Fire rate cooldown
		can_fire = false
		await get_tree().create_timer(fire_rate).timeout
		can_fire = true
		
		# Auto-reload if empty
		if current_ammo == 0:
			reload()
		
		return true
	return false

func reload():
	"""Reload the weapon."""
	if is_reloading or current_ammo == max_ammo:
		return
	
	is_reloading = true
	await get_tree().create_timer(reload_time).timeout
	current_ammo = max_ammo
	is_reloading = false
	reloaded.emit()
	ammo_changed.emit(current_ammo, max_ammo)

func get_ammo_count() -> int:
	return current_ammo

func is_empty() -> bool:
	return current_ammo == 0
