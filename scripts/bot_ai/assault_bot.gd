extends BotBase
class_name AssaultBot

# The offense bot - carries a customizable weapon and a melee weapon
@export var primary_weapon_data: WeaponData
@export var melee_weapon_data: WeaponData # Sword or Hammer

@onready var weapon_mount := $WeaponMount
@onready var melee_mount := $MeleeMount

func _physics_process(delta):
    super._physics_process(delta)
    
    # Offense Bot Logic: Aggressively pushes enemies
    if current_state == State.COMBAT and target_enemy:
        var distance = global_position.distance_to(target_enemy.global_position)
        
        if distance < 3.0:
            # Very close: Switch to Melee
            melee_attack()
        else:
            # In range: Shoot with customizable primary weapon
            shoot_primary()
            # Pathfind to keep pressure
            nav_agent.target_position = target_enemy.global_position
            var next_pos = nav_agent.get_next_path_position()
            velocity = (next_pos - global_position).normalized() * move_speed

func shoot_primary():
    # Uses the customizable primary_weapon_data
    if weapon_mount.get_child_count() > 0:
        var weapon = weapon_mount.get_child(0)
        if weapon.has_method("fire_weapon"):
            weapon.fire_weapon()

func melee_attack():
    # Uses the melee weapon (e.g., Sword)
    if melee_mount.get_child_count() > 0:
        var melee = melee_mount.get_child(0)
        if melee.has_method("swing_melee"):
            melee.swing_melee()
