# NEURAL STRIKE

**Neural Strike** is a tactical cyberpunk + light-steampunk FPS built with Godot 4. This repository provides the complete foundation—from modular weapon systems and virus-hacking mechanics to specialized AI bots and procedural generation prompts.

---

## 🚀 Quick Start & Implementation Roadmap

### Phase 1: Core Foundation (The Controller)
1. **Setup**: Create a Godot 4.3+ project.
2. **Player Scene**: Build `player.tscn` using a `CharacterBody3D`.
3. **Scripts**: Attach `player.gd` (provided in `/scripts/`).
4. **Input Map**: Define `move_forward`, `move_back`, `move_left`, `move_right`, `jump`, `shoot`, `melee`, `interact`.

### Phase 2: Weapons & Combat
1. **Modular Weapons**: Use `WeaponBase` (in `/scripts/combat/`) as a parent for all firearms.
2. **Hit Detection**: Implement RayCast3D for standard projectiles and Area3D for explosive payloads.
3. **Hacking Interaction**: Use the `HackingSystem` (in `/scripts/core/`) to allow players to compromise enemy systems using the Data Knife.

### Phase 3: Bot AI (The Offense Bot)
1. **AI Setup**: Use Godot's `NavigationServer3D` for pathfinding.
2. **Offense Bot**: Utilize `offense_bot.gd` (in `/scripts/bot_ai/`) for aggressive combatants.
   - **Customizable Loadouts**: Easily switch between the Assault Rifle and Melee fallback via the Inspector.
   - **Aggression Tuning**: Adjust the `aggression_factor` to change how the bot pushes toward the player.

---

## 📂 Repository Structure

- `/scripts/`: Core game logic (Player, Bot AI, Combat, Networking).
- `AI_PROMPTS.md`: Master prompts for AI generation of world maps, 3D models (no faces), textures, and synthwave SFX.
- `ASSETS_AND_TOOLS.md`: List of free AI tools and royalty-free resources for vfx/sfx.
- `walkthrough.md`: Step-by-step assembly guide for the Godot Editor.

---

## 🛠️ Requirements
- Godot Engine 4.3 or higher.
- Basic knowledge of GDScript for custom weapon data.
