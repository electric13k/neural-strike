# PHASE 3 SCENE QUICK-SETUP SCRIPT
# This script creates the main_arena scene structure automatically
# NOT meant to run in-game - use this as reference for manual setup

# HOW TO USE THIS REFERENCE:
# 1. Create new 3D Scene in Godot
# 2. Follow the node structure below
# 3. Copy the property values into Inspector

# ROOT NODE: Node3D (rename to "Main_Arena")
# Save scene as: res://scenes/levels/main_arena.tscn

class_name MainArenaSetup
extends Node

# This is a REFERENCE GUIDE showing exact values
# You'll create nodes manually in Godot Editor

const SCENE_STRUCTURE = {
	"root": {
		"type": "Node3D",
		"name": "Main_Arena",
		"children": [
			{
				"type": "WorldEnvironment",
				"name": "WorldEnvironment",
				"properties": {
					"environment": "New Environment (create in Inspector)"
				}
			},
			{
				"type": "DirectionalLight3D",
				"name": "Sun",
				"transform": {
					"rotation_degrees": Vector3(-45, 45, 0)
				},
				"properties": {
					"light_energy": 0.8,
					"shadow_enabled": true
				}
			},
			{
				"type": "CSGBox3D",
				"name": "Ground",
				"transform": {
					"position": Vector3(0, -0.5, 0)
				},
				"properties": {
					"size": Vector3(100, 1, 100),
					"material": "StandardMaterial3D with Albedo #2a2a2a"
				}
			},
			{
				"type": "Node3D",
				"name": "Walls",
				"children": [
					{
						"type": "CSGBox3D",
						"name": "WallNorth",
						"transform": {"position": Vector3(0, 5, -50)},
						"properties": {
							"size": Vector3(100, 10, 2),
							"material": "StandardMaterial3D with Albedo #4a4a4a"
						}
					},
					{
						"type": "CSGBox3D",
						"name": "WallSouth",
						"transform": {"position": Vector3(0, 5, 50)},
						"properties": {
							"size": Vector3(100, 10, 2),
							"material": "StandardMaterial3D with Albedo #4a4a4a"
						}
					},
					{
						"type": "CSGBox3D",
						"name": "WallEast",
						"transform": {"position": Vector3(50, 5, 0)},
						"properties": {
							"size": Vector3(2, 10, 100),
							"material": "StandardMaterial3D with Albedo #4a4a4a"
						}
					},
					{
						"type": "CSGBox3D",
						"name": "WallWest",
						"transform": {"position": Vector3(-50, 5, 0)},
						"properties": {
							"size": Vector3(2, 10, 100),
							"material": "StandardMaterial3D with Albedo #4a4a4a"
						}
					}
				]
			},
			{
				"type": "Node3D",
				"name": "SpawnPoints",
				"children": [
					# ALPHA TEAM SPAWNS
					{"type": "Marker3D", "name": "SpawnAlpha1", "position": Vector3(-40, 0.5, -40), "group": "spawn_points"},
					{"type": "Marker3D", "name": "SpawnAlpha2", "position": Vector3(-40, 0.5, -30), "group": "spawn_points"},
					{"type": "Marker3D", "name": "SpawnAlpha3", "position": Vector3(-30, 0.5, -40), "group": "spawn_points"},
					{"type": "Marker3D", "name": "SpawnAlpha4", "position": Vector3(-30, 0.5, -30), "group": "spawn_points"},
					# BRAVO TEAM SPAWNS
					{"type": "Marker3D", "name": "SpawnBravo1", "position": Vector3(40, 0.5, 40), "group": "spawn_points"},
					{"type": "Marker3D", "name": "SpawnBravo2", "position": Vector3(40, 0.5, 30), "group": "spawn_points"},
					{"type": "Marker3D", "name": "SpawnBravo3", "position": Vector3(30, 0.5, 40), "group": "spawn_points"},
					{"type": "Marker3D", "name": "SpawnBravo4", "position": Vector3(30, 0.5, 30), "group": "spawn_points"},
				]
			},
			{
				"type": "Camera3D",
				"name": "TestCamera",
				"transform": {
					"position": Vector3(0, 10, 20),
					"rotation_degrees": Vector3(-20, 0, 0)
				},
				"properties": {
					"fov": 75
				},
				"note": "Temporary camera for testing - delete later"
			}
		]
	}
}

# MATERIAL REFERENCES
const MATERIALS = {
	"ground": {
		"type": "StandardMaterial3D",
		"albedo_color": Color(0.164, 0.164, 0.164), # #2a2a2a
		"roughness": 0.8
	},
	"walls": {
		"type": "StandardMaterial3D",
		"albedo_color": Color(0.290, 0.290, 0.290), # #4a4a4a
		"roughness": 0.7
	}
}

# SPAWN POINT POSITIONS
const SPAWN_POSITIONS = {
	"alpha": [
		Vector3(-40, 0.5, -40),
		Vector3(-40, 0.5, -30),
		Vector3(-30, 0.5, -40),
		Vector3(-30, 0.5, -30)
	],
	"bravo": [
		Vector3(40, 0.5, 40),
		Vector3(40, 0.5, 30),
		Vector3(30, 0.5, 40),
		Vector3(30, 0.5, 30)
	]
}

# COPY THESE VALUES INTO GODOT INSPECTOR WHEN CREATING NODES MANUALLY
