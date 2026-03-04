extends Control

@onready var health_label = $HealthLabel
@onready var ammo_label = $AmmoLabel
@onready var crosshair = $Crosshair

func _ready():
	# Initial UI state
	update_health(100)
	update_ammo(30)

func update_health(value):
	health_label.text = "HP: " + str(value)

func update_ammo(value):
	ammo_label.text = "AMMO: " + str(value)

func show_hit_marker():
	# Implement hit marker animation
	pass
