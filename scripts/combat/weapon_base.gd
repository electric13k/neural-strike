extends Node3D
class_name Weapon

@export var data: WeaponData
@export var ammo: int = 30
@export var is_reloading: bool = false

signal fired
signal reloaded

func fire():
	if ammo > 0 and not is_reloading:
		ammo -= 1
		fired.emit()
		return true
	return false

func reload():
	if is_reloading: return
	is_reloading = true
	await get_tree().create_timer(1.5).timeout # Default reload time
	ammo = 30
	is_reloading = false
	reloaded.emit()
