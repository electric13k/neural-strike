extends Node3D
class_name HealthBar3D

@export var character: Node3D
@export var is_friendly: bool = true
@export var offset_y: float = 2.5

var progress_bar: ProgressBar
var camera: Camera3D

func _ready():
	# Create SubViewport for 3D UI
	var subviewport = SubViewport.new()
	subviewport.size = Vector2(200, 20)
	subviewport.transparent_bg = true
	add_child(subviewport)
	
	# Create ProgressBar in viewport
	progress_bar = ProgressBar.new()
	progress_bar.custom_minimum_size = Vector2(200, 20)
	progress_bar.show_percentage = false
	subviewport.add_child(progress_bar)
	
	# Create Sprite3D to display the viewport
	var sprite = Sprite3D.new()
	sprite.texture = subviewport.get_texture()
	sprite.billboard = BaseMaterial3D.BILLBOARD_ENABLED
	sprite.pixel_size = 0.01
	add_child(sprite)
	
	# Setup initial values
	if character and character.has_method("get"):
		if character.has_meta("max_health"):
			progress_bar.max_value = character.get_meta("max_health")
		elif "max_health" in character:
			progress_bar.max_value = character.max_health
		else:
			progress_bar.max_value = 100
		
		if character.has_meta("current_health"):
			progress_bar.value = character.get_meta("current_health")
		elif "current_health" in character:
			progress_bar.value = character.current_health
		else:
			progress_bar.value = progress_bar.max_value
	else:
		progress_bar.max_value = 100
		progress_bar.value = 100
	
	_update_color()
	
	camera = get_viewport().get_camera_3d()

func _process(_delta):
	if not character or not is_instance_valid(character):
		queue_free()
		return
	
	# Follow character position (above head)
	global_position = character.global_position + Vector3(0, offset_y, 0)
	
	# Update health value
	if character.has_meta("current_health"):
		progress_bar.value = clamp(character.get_meta("current_health"), 0, progress_bar.max_value)
	elif "current_health" in character:
		progress_bar.value = clamp(character.current_health, 0, progress_bar.max_value)

func set_friendly(friendly: bool):
	is_friendly = friendly
	_update_color()

func _update_color():
	if not progress_bar:
		return
	
	# Create custom StyleBoxFlat for the fill
	var fill_style = StyleBoxFlat.new()
	if is_friendly:
		fill_style.bg_color = Color(0.1, 0.9, 0.1)  # Green for teammates
	else:
		fill_style.bg_color = Color(0.9, 0.1, 0.1)  # Red for enemies
	
	progress_bar.add_theme_stylebox_override("fill", fill_style)
	
	# Background style (darker)
	var bg_style = StyleBoxFlat.new()
	bg_style.bg_color = Color(0.2, 0.2, 0.2, 0.8)
	progress_bar.add_theme_stylebox_override("background", bg_style)
