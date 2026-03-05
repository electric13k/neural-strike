extends Node3D
class_name BattlePad

# Battle Pad - Physical holographic tablet held by player
# Shows tactical interface with camera feeds and minimap

var player: CharacterBody3D
var ui: Control  # Reference to BattlePadUI
var is_deployed: bool = false

@onready var tablet_mesh = $TabletMesh
@onready var hologram_screen = $HologramScreen
@onready var viewport_display = $ViewportDisplay

func _ready():
	visible = false
	
	# Create UI viewport
	_setup_ui()

func _setup_ui():
	# UI is created separately in battle_pad_ui.gd
	# This just handles the 3D tablet model
	pass

func deploy():
	"""Player pulls out Battle Pad"""
	if is_deployed:
		return
	
	is_deployed = true
	visible = true
	
	# Position in front of player
	if player:
		global_position = player.global_position + player.transform.basis.z * -0.5
		global_position.y = player.global_position.y + 0.3
		look_at(player.global_position + player.transform.basis.z * -2, Vector3.UP)
	
	# Open UI
	if ui:
		ui.open_battle_pad()
	
	print("Battle Pad deployed")

func stow():
	"""Player puts away Battle Pad"""
	if not is_deployed:
		return
	
	is_deployed = false
	visible = false
	
	# Close UI
	if ui:
		ui.close_battle_pad()
	
	print("Battle Pad stowed")

func _process(delta):
	if not is_deployed:
		return
	
	# Keep tablet positioned in front of player
	if player and is_instance_valid(player):
		var target_pos = player.global_position + player.transform.basis.z * -0.5
		target_pos.y = player.global_position.y + 0.3
		
		global_position = global_position.lerp(target_pos, delta * 10.0)
		
		# Face player
		var look_target = player.global_position + player.transform.basis.z * -2
		look_at(look_target, Vector3.UP)

func set_player(p: CharacterBody3D):
	player = p

func set_ui(battle_pad_ui: Control):
	ui = battle_pad_ui
	if ui:
		ui.set_player(player)

func toggle():
	"""Toggle Battle Pad on/off"""
	if is_deployed:
		stow()
	else:
		deploy()
