# NEURAL STRIKE

**Neural Strike** is a tactical cyberpunk + light‑steampunk FPS built with Godot 4. This repository contains the complete design documentation, source code architecture, and implementation-ready scripts.

## 🚀 Quick Start & Implementation Roadmap

### Phase 1: Core Foundation (The Controller)
1. **Setup**: Create a Godot 4 project.
2. **Player Scene**: Build `player.tscn` using a `CharacterBody3D`.
3. **Scripts**: Attach `player_controller.gd` (provided in `/scripts/player/`).
4. **Input Map**: Define `move_forward`, `move_back`, `move_left`, `move_right`, `jump`, `sprint`, `crouch`, `shoot`, `melee`, `throw_grenade`, `interact`.

### Phase 2: Weapons & Melee
1. **Resources**: Define your weapon types using the `WeaponData.gd` resource.
2. **Hit Detection**: Implement hitscan (RayCast3D) for standard bullets and Area3D for explosive rounds.
3. **Data Knife**: Implement the `MeleeSystem.gd` to handle both combat damage and the virus-hacking interaction.

### Phase 3: Bot AI & Battle Pad
1. **AI Setup**: Use Godot's `NavigationServer3D` for bot pathfinding.
2. **States**: Bots use a Finite State Machine (FSM) to switch between Follow, Combat, and role-specific states (Medic/Spy/Sniper/Courier).
3. **UI**: Build the `BattlePad.tscn` to display remote camera feeds via `SubViewport`.

### Phase 4: Deployables & Special Grenades
1. **Force Fields**: Black and white hole grenades use radial forces applied to `CharacterBody3D.velocity` and `RigidBody3D.apply_central_force`.
2. **Persistence**: Ensure all deployables are linked to a `player_id` and despawn correctly.

---

## 📂 Project Structure
- `/scripts/`: All GDScript logic for players, bots, weapons, and networking.
- `/data/`: Resource-based data models (.tres templates).
- `/docs/`: Full game design documentation.
- `AI_PROMPTS.md`: Master prompts for generating world maps, models, and audio.
- `ASSETS_AND_TOOLS.md`: Directory of free AI tools and royalty-free resources.
