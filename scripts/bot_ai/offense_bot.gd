extends BotBase
class_name OffenseBot

# Offense Bot - Similar to assault but more aggressive
# Can be used as a variant of AssaultBot

@export var aggression_factor: float = 1.5  # How much more aggressive than base

func _ready():
	super._ready()
	speed *= aggression_factor
	detection_range *= 1.2

func get_role() -> String:
	return "Offense"
