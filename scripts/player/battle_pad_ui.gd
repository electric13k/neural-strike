extends Control
class_name BattlePadUI

# Battle Pad - Holographic tactical tablet UI
# Shows minimap, camera feeds from bots/tracker bullets, spy bot remote control

@export var minimap: TextureRect
@export var camera_grid: GridContainer  # Grid of camera feeds
@export var spy_camera_viewport: SubViewport  # Large viewport for spy bot POV
@export var cloak_energy_bar: ProgressBar
@export var waypoint_indicator: Control

var player: CharacterBody3D
var active_spy_bot: Node = null
var camera_feeds: Dictionary = {}  # Bot/bullet ID -> ViewportTexture
var is_controlling_spy: bool = false

@onready var minimap_camera = $MinimapCamera

func _ready():
	visible = false  # Hidden by default
	
	if spy_camera_viewport:
		spy_camera_viewport.visible = false
	
	if cloak_energy_bar:
		cloak_energy_bar.visible = false

func _input(event):
	if not visible:
		return
	
	# Click on minimap to set waypoint for spy bot
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		if is_controlling_spy and active_spy_bot:
			var minimap_pos = _get_minimap_click_position(event.position)
			if minimap_pos:
				_set_spy_waypoint(minimap_pos)
	
	# Right click to clear waypoints
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_RIGHT:
		if is_controlling_spy and active_spy_bot:
			active_spy_bot.clear_waypoints()
	
	# Toggle cloak (C key)
	if event.is_action_pressed("toggle_cloak") and is_controlling_spy:
		if active_spy_bot and active_spy_bot.has_method("toggle_cloak"):
			active_spy_bot.toggle_cloak()

func _process(delta):
	if not visible:
		return
	
	# Update spy bot cloak energy display
	if is_controlling_spy and active_spy_bot:
		if cloak_energy_bar and active_spy_bot.has_method("get_cloak_energy"):
			cloak_energy_bar.value = active_spy_bot.get_cloak_energy()

func open_battle_pad():
	"""Called when player presses Battle Pad button (Tab key)"""
	visible = true
	
	# Slow down time slightly (optional tactical pause)
	# Engine.time_scale = 0.3
	
	print("Battle Pad opened")

func close_battle_pad():
	"""Called when player releases Battle Pad button"""
	visible = false
	
	# Restore time scale
	# Engine.time_scale = 1.0
	
	# Disconnect spy control if active
	if is_controlling_spy:
		_disconnect_spy_control()
	
	print("Battle Pad closed")

func add_camera_feed(source_id: String, camera: Camera3D):
	"""Add a new camera feed to the grid (from bot or tracker bullet)"""
	if not camera_grid:
		return
	
	# Create a SubViewport for this camera
	var viewport = SubViewport.new()
	viewport.size = Vector2i(320, 240)  # Small feed resolution
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	
	# Create TextureRect to display the viewport
	var feed_display = TextureRect.new()
	feed_display.custom_minimum_size = Vector2(160, 120)
	feed_display.texture = viewport.get_texture()
	feed_display.expand_mode = TextureRect.EXPAND_FIT_WIDTH_PROPORTIONAL
	
	# Add label showing source
	var label = Label.new()
	label.text = source_id
	label.add_theme_color_override("font_color", Color.CYAN)
	
	var container = VBoxContainer.new()
	container.add_child(label)
	container.add_child(feed_display)
	
	camera_grid.add_child(container)
	
	# Store reference
	camera_feeds[source_id] = {
		"viewport": viewport,
		"camera": camera,
		"display": feed_display,
		"container": container
	}
	
	print("Camera feed added: ", source_id)

func remove_camera_feed(source_id: String):
	"""Remove camera feed when bot dies or bullet expires"""
	if not camera_feeds.has(source_id):
		return
	
	var feed = camera_feeds[source_id]
	feed["viewport"].queue_free()
	feed["container"].queue_free()
	camera_feeds.erase(source_id)
	
	print("Camera feed removed: ", source_id)

func connect_to_spy_bot(spy_bot: Node):
	"""Enter remote control mode for spy bot"""
	if not spy_bot or not spy_bot.has_method("enable_remote_control"):
		return
	
	# Disconnect previous spy if any
	if is_controlling_spy:
		_disconnect_spy_control()
	
	active_spy_bot = spy_bot
	is_controlling_spy = true
	
	# Enable remote control on spy bot
	spy_bot.enable_remote_control(player)
	
	# Show spy camera viewport
	if spy_camera_viewport:
		spy_camera_viewport.visible = true
		var spy_camera = spy_bot.get_camera_feed()
		if spy_camera:
			spy_camera_viewport.world_3d = spy_camera.get_world_3d()
	
	# Show cloak energy bar
	if cloak_energy_bar:
		cloak_energy_bar.visible = true
	
	print("Connected to Spy Bot - Remote control active")

func _disconnect_spy_control():
	"""Exit spy bot remote control"""
	if active_spy_bot and active_spy_bot.has_method("disable_remote_control"):
		active_spy_bot.disable_remote_control()
	
	active_spy_bot = null
	is_controlling_spy = false
	
	# Hide spy camera viewport
	if spy_camera_viewport:
		spy_camera_viewport.visible = false
	
	# Hide cloak energy bar
	if cloak_energy_bar:
		cloak_energy_bar.visible = false
	
	print("Disconnected from Spy Bot")

func _get_minimap_click_position(screen_pos: Vector2) -> Vector3:
	"""Convert minimap click to world position"""
	if not minimap:
		return Vector3.ZERO
	
	# Check if click is within minimap bounds
	var minimap_rect = minimap.get_global_rect()
	if not minimap_rect.has_point(screen_pos):
		return Vector3.ZERO
	
	# Convert to local minimap coordinates (0-1)
	var local_pos = (screen_pos - minimap_rect.position) / minimap_rect.size
	
	# Convert to world coordinates (simplified - needs actual map bounds)
	var map_size = 1000.0  # Adjust based on your map size
	var world_pos = Vector3(
		(local_pos.x - 0.5) * map_size,
		0,  # Y height - should raycast to terrain
		(local_pos.y - 0.5) * map_size
	)
	
	return world_pos

func _set_spy_waypoint(world_pos: Vector3):
	"""Set waypoint for spy bot via minimap click"""
	if not active_spy_bot or not active_spy_bot.has_method("add_waypoint"):
		return
	
	active_spy_bot.add_waypoint(world_pos)
	
	# Show visual indicator on minimap
	if waypoint_indicator:
		waypoint_indicator.visible = true
		# Position indicator at clicked location
	
	print("Waypoint set: ", world_pos)

func set_player(p: CharacterBody3D):
	player = p
