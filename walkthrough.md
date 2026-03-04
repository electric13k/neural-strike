# NEURAL STRIKE: WALKTHROUGH

This guide walks you through the initial setup and core loop implementation for **Neural Strike**.

## 1. Project Initialization
1.  **Engine**: Install Godot 4.3+.
2.  **Organization**: Replicate the folder structure from this repo (`/scripts/`, `/data/`, `/scenes/`, etc.).
3.  **Autoloads**: Open `Project Settings > Autoload` and add:
    *   `MatchManager`: `res://scripts/game_logic/match_manager.gd`
    *   `NetworkManager`: `res://scripts/networking/network_manager.gd`
    *   `CameraFeedManager`: `res://scripts/utilities/camera_feed_manager.gd`

## 2. Character Setup
1.  **Player Scene**:
    *   Root: `CharacterBody3D`
    *   Add `Camera3D` at head level.
    *   Add `RayCast3D` (child of Camera) for aiming.
    *   Attach `player_controller.gd`.
2.  **Assault Bot (Offense Bot)**:
    *   Root: `CharacterBody3D`
    *   Add `NavigationAgent3D` for movement.
    *   Add `WeaponMount` (Node3D) and `MeleeMount` (Node3D).
    *   Attach `assault_bot.gd`.
    *   *Tip*: Assign an Assault Rifle to the `primary_weapon_data` slot in the Inspector.

## 3. Combat Mechanics
1.  **Weapon Resources**: Right-click in FileSystem > `New Resource` > `WeaponData`. Define stats for your Assault Rifle, SMG, and Sword.
2.  **Hacking**: Ensure bots have a `NeckDataSlot` (Marker3D or Area3D) so the `MeleeSystem` can detect the back-stab interaction.

## 4. Map & Assets
1.  **AI Maps**: Use the prompt in `AI_PROMPTS.md` with **Leonardo.ai** to generate a top-down tactical layout.
2.  **Modeling**: Import CC0 kits from **Kenney.nl** to quickly build platforms and rusty steam pipes.
3.  **Skybox**: Use **Blockade Labs** to generate a neon cyberpunk 360 view.

## 5. Testing
1.  Run two instances of Godot (`Debug > Run Multiple Instances > 2 Instances`).
2.  Host on one, join on the other via localhost (`127.0.0.1`).
3.  Test bot following and the virus-knife hacking.
