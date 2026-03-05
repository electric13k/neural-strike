# Player Animation System Setup Guide

## Overview
Complete animation system for Neural Strike with support for:
- **4 weapon types:** Great Sword, Pistol, Data Knife, Hammer
- **Movement states:** Idle, Walk, Sprint, Crouch, Strafe
- **Combat actions:** Attack, Block, Reload, Dodge, Death
- **100+ animations** organized by weapon and context

---

## Animation List by Category

### Great Sword Animations (49 files)

**Idle & Movement:**
- `great sword idle.fbx` - Standing still
- `great sword idle (2-5).fbx` - Idle variations
- `great sword walk.fbx` - Walking forward
- `great sword run.fbx` - Running forward
- `walk forward/backward/left/right.fbx` - Directional walking
- `walk crouching forward/backward/left/right.fbx` - Crouched movement

**Attacks:**
- `great sword attack.fbx` - Basic attack
- `draw a great sword 1.fbx` - Draw and slash
- `draw a great sword 2.fbx` - Draw variation
- `great sword slash (2-5).fbx` - Combo slashes
- `great sword 180 turn (2).fbx` - Spin attack
- `great sword high spin attack.fbx` - Jump spin
- `great sword kick (2).fbx` - Kick follow-up
- `great sword slide attack.fbx` - Slide slash
- `great sword power up.fbx` - Charge attack

**Defense:**
- `great sword blocking.fbx` - Standing block
- `great sword blocking (2-3).fbx` - Moving block

**Special:**
- `great sword casting.fbx` - Special ability
- `great sword crouching (2-6).fbx` - Crouch states
- `great sword strafe (2-4).fbx` - Strafing
- `great sword impact (2-5).fbx` - Hit reactions
- `great sword jump (2).fbx` - Jump attack
- `great sword turn.fbx` - Quick turn

### Pistol Animations (25 files)

**Idle & Movement:**
- `pistol idle.fbx` - Standing still
- `pistol walk.fbx` - Walking forward
- `pistol walk arc (2).fbx` - Walking with turns
- `pistol walk backward.fbx` - Walking back
- `pistol walk backward arc (2).fbx` - Backing up with turns
- `pistol run.fbx` - Running
- `pistol run arc (2).fbx` - Running with turns
- `pistol run backward.fbx` - Running backward
- `pistol run backward arc (2).fbx` - Backing up fast

**Combat:**
- `pistol jump (2).fbx` - Jump shot
- `pistol jump attack.fbx` - Jump kick
- `pistol strafe (2).fbx` - Strafe shooting
- `pistol kneel to stand.fbx` - Cover transition
- `pistol kneeling idle.fbx` - Crouched

### Melee Animations (Knife/Hammer - 3 files)

**Attacks:**
- `Stabbing (1).fbx` - Quick stab
- `Stabbing (2) data knife virus on robot move.fbx` - Hacking stab (used for data knife virus)
- `Stabbing.fbx` - Heavy stab (used for hammer)

### Universal Animations (50 files)

**Movement:**
- `idle.fbx` - Default idle
- `idle aiming.fbx` - Aiming stance
- `idle crouching.fbx` - Crouched idle
- `idle crouching aiming.fbx` - Crouched aim
- `walk forward/backward/left/right.fbx` - Basic walking
- `walk crouching forward/backward/left/right.fbx` - Crouched walking
- `run forward/backward/left/right.fbx` - Running
- `sprint forward/backward/left/right.fbx` - Sprinting

**Jumping:**
- `jump up.fbx` - Jump start
- `jump down.fbx` - Landing
- `jump loop.fbx` - In-air

**Turning:**
- `turn 90 left/right.fbx` - Quick turns
- `crouching turn 90 left/right.fbx` - Crouched turns

**Death Animations:**
- `death from the front.fbx` - Shot from front
- `death from the back.fbx` - Shot from back
- `death from left/right.fbx` - Side deaths
- `death crouching headshot front.fbx` - Headshot while crouched
- `death from back headshot.fbx` - Headshot from behind
- `death from front headshot.fbx` - Headshot from front

**Special:**
- `spell cast.fbx` - Ability casting
- `two handed sword death (2).fbx` - Sword death animation

---

## Setup in Godot

### Step 1: Import Animations

1. Place all `.fbx` files in `assets/models/animations/player/`
2. In Godot, select all animation files
3. Inspector → Import tab:
   - **Animation → FPS:** 30
   - **Animation → Trimming:** Enabled
   - **Animation → Remove Immutable Tracks:** Enabled
   - **Meshes → Light Baking:** Disabled (animations only)
4. Click **Reimport**

### Step 2: Setup Player Scene

1. Open `scenes/player/player.tscn`
2. Add `AnimationPlayer` node as child of Player root
3. Add `Node` named `AnimationController` as child of Player
4. Attach script `scripts/player/animation_controller.gd` to AnimationController
5. In Inspector for AnimationController:
   - **Animation Player:** Drag AnimationPlayer node
   - **Character Body:** Drag Player root node

### Step 3: Link Animations to AnimationPlayer

**For each weapon set, create animation library:**

1. Select AnimationPlayer
2. Animation panel (bottom) → Click folder icon → **Add Animation Library**
3. Name it: `greatsword`, `pistol`, `knife`, `universal`
4. For each library, click **+** → **Load Animation from File**
5. Select corresponding `.fbx` files

**Alternative: Use AnimationTree (Advanced)**

1. Add `AnimationTree` node
2. Create state machine with blend trees:
   - Idle → Walk → Sprint (blend by speed)
   - Attack state (oneshot)
   - Death state (oneshot, terminal)

### Step 4: Configure Input Actions

In **Project → Project Settings → Input Map**, add:

- `weapon_1` → Key: 1 (Great Sword)
- `weapon_2` → Key: 2 (Pistol)
- `weapon_3` → Key: 3 (Data Knife)
- `weapon_4` → Key: 4 (Hammer)
- `crouch` → Key: Ctrl
- `sprint` → Key: Shift
- `block` → Key: Q (Great Sword)
- `dodge` → Key: Space + Movement
- `aim` → Key: Right Mouse Button

### Step 5: Update Player Script

The provided `player.gd` already includes:
- Movement input detection
- Weapon switching (1-4 keys)
- Combat action triggers
- Animation controller integration

---

## Usage Examples

### Basic Movement
```gdscript
# Player walks forward → AnimationController automatically plays:
# - "great sword walk.fbx" (if greatsword equipped)
# - "pistol walk.fbx" (if pistol equipped)
# - "walk forward.fbx" (if knife/hammer equipped)
```

### Attacking
```gdscript
# Player presses shoot/attack button:
if Input.is_action_just_pressed("shoot"):
    animation_controller.play_attack()
    # Plays random attack animation based on current weapon
```

### Weapon Switching
```gdscript
# Player presses 2 (switch to pistol):
switch_weapon("pistol")
# AnimationController updates, next movement uses pistol animations
```

### Death from Behind
```gdscript
# Enemy shoots player from behind:
player.take_damage(100, enemy.global_position - player.global_position)
# Automatically detects direction and plays "death from the back.fbx"
```

---

## Animation State Priority

**Priority order (highest to lowest):**

1. **Death** - Cannot be interrupted
2. **Attack/Stab/Shoot** - Plays to completion
3. **Block/Dodge** - Can be interrupted by death
4. **Sprint** - Can transition to walk
5. **Walk/Crouch** - Can transition freely
6. **Idle** - Lowest priority

---

## Customization

### Add New Attack Combo

In `animation_controller.gd`, edit `_play_greatsword_attack()`:

```gdscript
var attacks = [
    "great sword attack.fbx",
    "great sword slash.fbx",
    "your_custom_attack.fbx"  # Add here
]
```

### Change Animation Speed

In AnimationPlayer:
1. Select animation
2. Inspector → **Speed Scale:** 1.5 (150% speed)

### Blend Between Animations

Use AnimationTree with blend nodes:
1. Create BlendSpace2D for 8-directional movement
2. Blend based on `velocity.x` and `velocity.z`

---

## Troubleshooting

**Q: Animations don't play**
- Check AnimationPlayer has animations imported
- Verify `animation_controller.animation_player` is assigned in Inspector
- Check console for "No AnimationPlayer assigned!" error

**Q: Wrong animation plays**
- Verify `current_weapon_type` matches equipped weapon
- Check `set_weapon_type()` is called when switching weapons

**Q: Animations are choppy**
- Increase FPS on import (try 60 FPS)
- Enable interpolation in AnimationPlayer
- Check if physics FPS is too low (Project Settings → Physics → FPS)

**Q: Player slides during animation**
- Use root motion (advanced): Enable in AnimationPlayer
- Or disable movement input during attack animations

**Q: Death animation doesn't finish**
- Make sure `set_physics_process(false)` is called in `die()`
- Check animation isn't being interrupted by idle

---

## Next Steps

1. **Test each weapon:**
   - Press 1-4 to switch weapons
   - Move in all directions
   - Attack, block, crouch, sprint
   - Verify animations transition smoothly

2. **Add sound effects:**
   - Use AnimationPlayer's "Call Method Track"
   - Trigger sound on attack frame
   - Example: Frame 10 → `audio_player.play("sword_slash.wav")`

3. **Add hit detection:**
   - Create `Area3D` on weapon
   - Enable during attack animation frames
   - Disable when animation ends

4. **Polish:**
   - Add camera shake on heavy attacks
   - Add particle effects (slash trails)
   - Add motion blur during spin attacks

---

## File Structure

```
res://
├── scenes/
│   └── player/
│       └── player.tscn (AnimationPlayer + AnimationController)
├── scripts/
│   └── player/
│       ├── player.gd (updated with weapon switching)
│       └── animation_controller.gd (NEW - handles all animations)
├── assets/
│   └── models/
│       └── animations/
│           └── player/
│               ├── great_sword/
│               ├── pistol/
│               ├── melee/
│               └── universal/
```

---

**Animation system is now complete!** Pull the changes and follow the setup guide to integrate into your player scene.
