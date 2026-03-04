# NEURAL STRIKE: ASSEMBLY WALKTHROUGH

This guide walks you through the step-by-step assembly of the **Neural Strike** foundation in Godot 4.

---

## 1. Project Setup
1. **Engine**: Install Godot 4.3+.
2. **Folders**: Replicate the folder structure from this repository (specifically `/scripts/` and its subdirectories).
3. **Autoloads**: Open `Project Settings > Autoload` and add:
   - `GameManager`: `res://scripts/core/game_manager.gd`
   - `NetworkManager`: `res://scripts/networking/network_manager.gd`

---

## 2. Character & Bot Assembly

### Player Scene
- **Root**: `CharacterBody3D` (Name: "Player")
- **Head**: `Node3D` at head level (Name: "Head")
- **Camera**: `Camera3D` child of Head.
- **Script**: Attach `player.gd`.
- **Inputs**: Ensure `shoot`, `melee`, and `interact` are bound in the Input Map.

### Offense Bot Scene
- **Root**: `CharacterBody3D` (Name: "OffenseBot")
- **Navigation**: Add a `NavigationAgent3D` child.
- **Weapon Mounts**: Add two `Node3D` markers for `PrimaryWeapon` and `MeleeWeapon`.
- **Script**: Attach `offense_bot.gd`.
- **Inspector**: Assign an Assault Rifle scene to the `Assault Rifle Scene` slot.

---

## 3. Gameplay Mechanics

### Combat & Hacking
1. **Weapon Base**: Create scenes for your guns and inherit from `WeaponBase` to use the firing and ammo logic.
2. **Data Knife**: The player's melee attack should check for a `HackingSystem` component on the target.
3. **Hacking**: Use `hacking_system.gd` to manage the progress bar and success/failure states of a breach.

---

## 4. AI & Assets
1. **Maps**: Use the prompts in `AI_PROMPTS.md` with **Leonardo.ai** to generate top-down layouts.
2. **Models**: Import CC0 kits (like Kenney.nl) and apply the "Cyber-Steampunk" textures generated using the prompts.
3. **SFX**: Use the Suno/Stable Audio prompts for weapon fire and hacking hums.
