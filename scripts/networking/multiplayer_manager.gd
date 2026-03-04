extends Node

# Multiplayer Manager - Handles server/client connections and player sync
# Uses Godot 4's high-level multiplayer with ENet

const PORT = 7777
const MAX_PLAYERS = 16

var peer: ENetMultiplayerPeer = null
var players: Dictionary = {}  # peer_id -> player_data {name, team, score, bot_id}

signal player_connected(peer_id, player_data)
signal player_disconnected(peer_id)
signal server_started()
signal server_stopped()
signal connection_succeeded()
signal connection_failed()

func _ready():
	# Connect to multiplayer signals
	multiplayer.peer_connected.connect(_on_player_connected)
	multiplayer.peer_disconnected.connect(_on_player_disconnected)
	multiplayer.connected_to_server.connect(_on_connected_ok)
	multiplayer.connection_failed.connect(_on_connected_fail)
	multiplayer.server_disconnected.connect(_on_server_disconnected)

func create_server() -> bool:
	"""Start a dedicated server or host"""
	peer = ENetMultiplayerPeer.new()
	var result = peer.create_server(PORT, MAX_PLAYERS)
	
	if result == OK:
		multiplayer.multiplayer_peer = peer
		print("[SERVER] Started on port %d (max %d players)" % [PORT, MAX_PLAYERS])
		
		# Register server as player ID 1
		players[1] = {
			"name": "Server",
			"team": "alpha",
			"score": 0,
			"bot_id": -1
		}
		
		server_started.emit()
		return true
	else:
		printerr("[SERVER] Failed to start: Error %d" % result)
		return false

func join_server(address: String) -> bool:
	"""Connect to a server as a client"""
	peer = ENetMultiplayerPeer.new()
	var result = peer.create_client(address, PORT)
	
	if result == OK:
		multiplayer.multiplayer_peer = peer
		print("[CLIENT] Connecting to %s:%d" % [address, PORT])
		return true
	else:
		printerr("[CLIENT] Failed to connect: Error %d" % result)
		return false

func stop_multiplayer():
	"""Disconnect and cleanup"""
	if peer:
		peer.close()
		peer = null
		multiplayer.multiplayer_peer = null
		players.clear()
		print("[NETWORK] Disconnected")
		server_stopped.emit()

# ========== SERVER-SIDE EVENTS ==========

func _on_player_connected(id: int):
	"""Called on server when a client connects"""
	if not multiplayer.is_server():
		return
	
	print("[SERVER] Player %d connected" % id)
	
	# Register new player with default data
	players[id] = {
		"name": "Player%d" % id,
		"team": "alpha" if (id % 2 == 0) else "bravo",  # Auto team balance
		"score": 0,
		"bot_id": -1  # Will be assigned when bot spawns
	}
	
	# Sync existing players to new client
	rpc_id(id, "_receive_player_list", players)
	
	# Notify all clients about new player
	rpc("_on_player_joined", id, players[id])

func _on_player_disconnected(id: int):
	"""Called when a player disconnects"""
	print("[NETWORK] Player %d disconnected" % id)
	
	if id in players:
		# Notify all clients before removing
		rpc("_on_player_left", id)
		players.erase(id)
	
	player_disconnected.emit(id)

# ========== CLIENT-SIDE EVENTS ==========

func _on_connected_ok():
	"""Called on client when successfully connected to server"""
	print("[CLIENT] Successfully connected to server")
	connection_succeeded.emit()

func _on_connected_fail():
	"""Called on client when connection fails"""
	printerr("[CLIENT] Connection to server failed")
	connection_failed.emit()

func _on_server_disconnected():
	"""Called on client when server shuts down"""
	print("[CLIENT] Server disconnected")
	stop_multiplayer()

# ========== RPC CALLS ==========

@rpc("authority", "reliable")
func _receive_player_list(player_list: Dictionary):
	"""Sent from server to new client with all existing players"""
	players = player_list
	print("[CLIENT] Received player list: %d players" % players.size())

@rpc("authority", "call_local", "reliable")
func _on_player_joined(peer_id: int, player_data: Dictionary):
	"""Broadcast when a new player joins"""
	players[peer_id] = player_data
	player_connected.emit(peer_id, player_data)
	print("[NETWORK] %s joined (Team: %s)" % [player_data.name, player_data.team])

@rpc("authority", "call_local", "reliable")
func _on_player_left(peer_id: int):
	"""Broadcast when a player disconnects"""
	if peer_id in players:
		var name = players[peer_id].name
		players.erase(peer_id)
		print("[NETWORK] %s left" % name)
	player_disconnected.emit(peer_id)

# ========== GAME STATE SYNC ==========

func update_player_score(peer_id: int, score_delta: int):
	"""Server-only: Update and sync a player's score"""
	if not multiplayer.is_server():
		return
	
	if peer_id in players:
		players[peer_id]["score"] += score_delta
		rpc("_sync_player_score", peer_id, players[peer_id]["score"])

@rpc("authority", "call_local", "reliable")
func _sync_player_score(peer_id: int, new_score: int):
	"""Broadcast score updates to all clients"""
	if peer_id in players:
		players[peer_id]["score"] = new_score

func assign_bot_to_player(peer_id: int, bot_node_id: int):
	"""Server-only: Assign a bot to a player"""
	if not multiplayer.is_server() or peer_id not in players:
		return
	
	players[peer_id]["bot_id"] = bot_node_id
	rpc("_sync_bot_assignment", peer_id, bot_node_id)

@rpc("authority", "call_local", "reliable")
func _sync_bot_assignment(peer_id: int, bot_node_id: int):
	if peer_id in players:
		players[peer_id]["bot_id"] = bot_node_id

# ========== UTILITY ==========

func get_player_data(peer_id: int) -> Dictionary:
	return players.get(peer_id, {})

func get_all_players() -> Dictionary:
	return players

func get_local_peer_id() -> int:
	return multiplayer.get_unique_id()

func is_server() -> bool:
	return multiplayer.is_server()

func get_player_count() -> int:
	return players.size()
