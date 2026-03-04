extends Node

var current_level = null
var players = {}

func _ready():
	process_mode = PROCESS_MODE_ALWAYS

func load_level(level_path: String):
	if current_level:
		current_level.queue_free()
	
	var level_scene = load(level_path)
	current_level = level_scene.instantiate()
	add_child(current_level)
	print(\"Level loaded: \", level_path)

func register_player(id: int, player_node: Node):
	players[id] = player_node
	print(\"Player registered: \", id)

func unregister_player(id: int):
	if players.has(id):
		players.erase(id)
		print(\"Player unregistered: \", id)
