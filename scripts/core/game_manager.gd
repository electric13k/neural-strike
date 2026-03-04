extends Node

# GameManager - Handles global game state, scoring, and round management
# Auto-loaded as singleton

var player_scores: Dictionary = {}  # peer_id -> score
var round_time: float = 0.0
var game_started: bool = false

enum GameMode { DEATHMATCH, TEAM_DEATHMATCH, CAPTURE, FREE_FOR_ALL }
var current_mode: GameMode = GameMode.TEAM_DEATHMATCH

signal score_updated(peer_id, new_score)
signal round_ended(winner_team)
signal game_started()

func _ready():
	print("[GameManager] Initialized")

func _process(delta):
	if game_started:
		round_time += delta

func start_game():
	game_started = true
	round_time = 0.0
	player_scores.clear()
	game_started.emit()
	print("[GameManager] Game started")

func end_game():
	game_started = false
	var winner = get_winner()
	round_ended.emit(winner)
	print("[GameManager] Game ended, winner: %s" % winner)

func add_score(peer_id: int, points: int, reason: String = ""):
	"""Add score to a player and sync via MultiplayerManager."""
	if not peer_id in player_scores:
		player_scores[peer_id] = 0
	
	player_scores[peer_id] += points
	score_updated.emit(peer_id, player_scores[peer_id])
	
	# Sync to all clients if server
	if MultiplayerManager.is_server():
		MultiplayerManager.update_player_score(peer_id, points)
	
	if reason:
		print("[GameManager] Player %d earned %d points (%s)" % [peer_id, points, reason])

func get_score(peer_id: int) -> int:
	return player_scores.get(peer_id, 0)

func get_winner() -> String:
	if player_scores.is_empty():
		return "No players"
	
	var highest_score = 0
	var winner_id = -1
	
	for peer_id in player_scores:
		if player_scores[peer_id] > highest_score:
			highest_score = player_scores[peer_id]
			winner_id = peer_id
	
	if winner_id == -1:
		return "Draw"
	
	var player_data = MultiplayerManager.get_player_data(winner_id)
	return player_data.get("name", "Player%d" % winner_id)

func on_player_kill(killer_id: int, victim_id: int):
	"""Called when a player kills another player."""
	add_score(killer_id, 100, "Kill")
	
	# Respawn victim after delay
	if MultiplayerManager.is_server():
		await get_tree().create_timer(3.0).timeout
		respawn_player(victim_id)

func respawn_player(peer_id: int):
	"""Respawn a player at a random spawn point."""
	var spawn_points = get_tree().get_nodes_in_group("spawn_points")
	if spawn_points.is_empty():
		print("[GameManager] No spawn points found!")
		return
	
	var spawn = spawn_points[randi() % spawn_points.size()]
	rpc_id(peer_id, "_respawn_at", spawn.global_position)

@rpc("authority", "call_local")
func _respawn_at(pos: Vector3):
	"""Client-side respawn."""
	var player = get_tree().get_first_node_in_group("local_player")
	if player:
		player.global_position = pos
		player.velocity = Vector3.ZERO
		if player.has_method("reset_health"):
			player.reset_health()
