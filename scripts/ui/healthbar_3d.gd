extends Node3D
class_name HealthBar3D

# 3D Healthbar that floats above character/bot
# Green = Friendly, Red = Enemy

@export var max_health: float = 100.0
@export var current_health: float = 100.0
@export var is_enemy: bool = true  # true = red, false = green
@export var offset_y: float = 2.5  # Height above character
@export var bar_width: float = 1.0
@export var bar_height: float = 0.15

var bar_mesh: MeshInstance3D
var background_mesh: MeshInstance3D

func _ready():
	create_healthbar()
	update_health(current_health)

func create_healthbar():
	# Create background (dark grey)
	background_mesh = MeshInstance3D.new()
	var bg_box = BoxMesh.new()
	bg_box.size = Vector3(bar_width, bar_height, 0.05)
	background_mesh.mesh = bg_box
	
	var bg_mat = StandardMaterial3D.new()
	bg_mat.albedo_color = Color(0.2, 0.2, 0.2, 0.8)
	bg_mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	background_mesh.material_override = bg_mat
	
	add_child(background_mesh)
	background_mesh.position = Vector3(0, offset_y, 0)
	
	# Create health bar (colored)
	bar_mesh = MeshInstance3D.new()
	var bar_box = BoxMesh.new()
	bar_box.size = Vector3(bar_width, bar_height, 0.06)
	bar_mesh.mesh = bar_box
	
	var bar_mat = StandardMaterial3D.new()
	bar_mat.albedo_color = Color.RED if is_enemy else Color.GREEN
	bar_mat.emission_enabled = true
	bar_mat.emission = bar_mat.albedo_color
	bar_mat.emission_energy = 0.5
	bar_mesh.material_override = bar_mat
	
	add_child(bar_mesh)
	bar_mesh.position = Vector3(0, offset_y, 0.01)

func update_health(new_health: float):
	current_health = clamp(new_health, 0, max_health)
	
	if bar_mesh:
		var health_percent = current_health / max_health
		var new_width = bar_width * health_percent
		
		# Update bar width
		var bar_box = bar_mesh.mesh as BoxMesh
		bar_box.size.x = new_width
		
		# Shift position to keep left-aligned
		var offset = (bar_width - new_width) / 2.0
		bar_mesh.position.x = -offset
		
		# Change color based on health percentage
		if health_percent > 0.6:
			# Healthy - keep team color
			pass
		elif health_percent > 0.3:
			# Damaged - yellow
			var mat = bar_mesh.material_override as StandardMaterial3D
			mat.albedo_color = Color.YELLOW
			mat.emission = Color.YELLOW
		else:
			# Critical - orange
			var mat = bar_mesh.material_override as StandardMaterial3D
			mat.albedo_color = Color.ORANGE
			mat.emission = Color.ORANGE

func set_team(is_enemy_team: bool):
	is_enemy = is_enemy_team
	if bar_mesh:
		var mat = bar_mesh.material_override as StandardMaterial3D
		mat.albedo_color = Color.RED if is_enemy else Color.GREEN
		mat.emission = mat.albedo_color

func _process(_delta):
	# Billboard effect - always face camera
	var camera = get_viewport().get_camera_3d()
	if camera:
		look_at(camera.global_position, Vector3.UP)
		rotation.x = 0  # Keep horizontal
		rotation.z = 0
