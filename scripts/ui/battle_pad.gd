extends Control
class_name BattlePad

# Battle Pad - Tactical device for camera feeds and bot control
# Toggle with Tab key, shows all tracker bullets, spy cams, static cameras

@onready var camera_feeds_grid = $MarginContainer/VBoxContainer/CameraFeeds/GridContainer
@onready var bot_control_panel = $MarginContainer/VBoxContainer/BotControl
@onready var minimap = $MarginContainer/VBoxContainer/Minimap
@onready var hack_progress_bar = $MarginContainer/VBoxContainer/HackProgress

var active_cameras: Dictionary = {}  # camera_id -> {camera: Camera3D, viewport: SubViewport}
var player_bot: Node3D = null
var data_knife_virus: DataKnifeVirus = null

signal bot_command_issued(command, target)

func _ready():
	visible = false  # Hidden by default
	add_to_group("battle_pad")  # For easy lookup
	
	# Connect to virus system if it exists
	await get_tree().process_frame
	data_knife_virus = get_tree().get_first_node_in_group("data_knife")
	if data_knife_virus:
		data_knife_virus.hack_started.connect(_on_hack_started)
		data_knife_virus.hack_progress_updated.connect(_on_hack_progress)
		data_knife_virus.hack_completed.connect(_on_hack_completed)
		data_knife_virus.hack_interrupted.connect(_on_hack_interrupted)

func _input(event):
	if event.is_action_pressed("toggle_battle_pad"):  # Bind to Tab in Input Map
		visible = not visible
		if visible:
			Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
		else:
			Input.mouse_mode = Input.MOUSE_MODE_CAPTURED

func register_camera(camera_id: String, camera: Camera3D, source_type: String):
	"""Register a camera feed (tracker bullet, spy bot, deployable camera)"""
	if camera_id in active_cameras:
		return
	
	# Create SubViewport for camera feed
	var feed_container = SubViewportContainer.new()
	feed_container.name = "Feed_%s" % camera_id
	feed_container.custom_minimum_size = Vector2(200, 150)
	feed_container.stretch = true
	
	var viewport = SubViewport.new()
	viewport.size = Vector2(200, 150)
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	viewport.transparent_bg = true
	feed_container.add_child(viewport)
	
	# Add camera to viewport
	var cam_clone = camera.duplicate()
	viewport.add_child(cam_clone)
	viewport.world_3d = camera.get_viewport().world_3d  # Share same world
	
	# Add label overlay
	var label = Label.new()
	label.text = "%s: %s" % [source_type, camera_id.substr(0, 20)]
	label.add_theme_color_override("font_color", Color(0, 1, 1, 0.9))  # Cyan
	label.add_theme_font_size_override("font_size", 10)
	label.position = Vector2(5, 5)
	feed_container.add_child(label)
	
	# Store reference
	active_cameras[camera_id] = {"camera": camera, "viewport": viewport, "container": feed_container}
	
	camera_feeds_grid.add_child(feed_container)
	print("Registered camera feed: %s" % camera_id)

func unregister_camera(camera_id: String):
	if camera_id not in active_cameras:
		return
	
	var feed_data = active_cameras[camera_id]
	if feed_data.container:
		feed_data.container.queue_free()
	
	active_cameras.erase(camera_id)
	print("Unregistered camera feed: %s" % camera_id)

func set_bot(bot: Node3D):
	player_bot = bot
	update_bot_panel()

func update_bot_panel():
	if not player_bot or not is_instance_valid(player_bot):
		bot_control_panel.visible = false
		return
	
	bot_control_panel.visible = true
	
	# Update bot status if methods exist
	if player_bot.has_method("get_role"):
		var role_label = bot_control_panel.get_node_or_null("RoleLabel")
		if role_label:
			role_label.text = "Role: %s" % player_bot.get_role()
	
	if player_bot.has_method("get_health_percent"):
		var health_bar = bot_control_panel.get_node_or_null("HealthBar")
		if health_bar:
			health_bar.value = player_bot.get_health_percent()

# Bot command buttons (connect these in the scene editor)
func _on_bot_follow_pressed():
	bot_command_issued.emit("follow", null)
	if player_bot and player_bot.has_method("set_behavior"):
		player_bot.set_behavior("follow")

func _on_bot_hold_pressed():
	var hold_pos = player_bot.global_position if player_bot else Vector3.ZERO
	bot_command_issued.emit("hold_position", hold_pos)
	if player_bot and player_bot.has_method("set_behavior"):
		player_bot.set_behavior("hold", hold_pos)

func _on_bot_fetch_pressed():
	# TODO: Open item selector UI or fetch nearest loot
	bot_command_issued.emit("fetch_item", null)
	if player_bot and player_bot.has_method("fetch_nearest_item"):
		player_bot.fetch_nearest_item()

func _on_bot_aggressive_pressed():
	bot_command_issued.emit("aggressive", null)
	if player_bot and player_bot.has_method("set_behavior"):
		player_bot.set_behavior("aggressive")

# Hack progress callbacks
func _on_hack_started(target):
	hack_progress_bar.visible = true
	hack_progress_bar.value = 0

func _on_hack_progress(percent: float):
	hack_progress_bar.value = percent

func _on_hack_completed(target):
	hack_progress_bar.visible = false
	# Optional: Show success notification

func _on_hack_interrupted(reason: String):
	hack_progress_bar.visible = false
	# Optional: Show interrupt notification
