# 3D Health Bar Setup Guide

## Overview
This system adds red/green health bars above characters in 3D space. Green = teammate, Red = enemy.

## What Was Added

### 1. Script: `scripts/ui/health_bar_3d.gd`
- Creates a 3D billboard health bar using SubViewport + Sprite3D
- Automatically follows assigned character
- Updates color based on `is_friendly` flag
- Auto-removes when character is destroyed

### 2. Updated: `scripts/player/player.gd`
- Added `max_health` and `current_health` properties
- Added `take_damage()` and `heal()` methods
- Added `die()` placeholder

### 3. Updated: `scripts/bot_ai/bot_base.gd`
- Added `max_health` and `current_health` properties
- Added `take_damage()` and `heal()` methods
- Added `is_friendly_to(peer_id)` helper method
- Added `set_owner(peer_id)` for hacking system integration

## How to Set Up in Godot

### Step 1: Create Health Bar Scene

1. In Godot, create a new scene: **Scene → New Scene → Other Node → Node3D**
2. Save as `res://scenes/ui/health_bar_3d.tscn`
3. Select the root Node3D
4. In Inspector → **Script** → Click folder icon → Select `scripts/ui/health_bar_3d.gd`
5. Save the scene (Ctrl+S)

**That's it!** The script automatically creates all child nodes (SubViewport, ProgressBar, Sprite3D).

### Step 2: Add Health Bar to Player

1. Open `res://scenes/player/player.tscn`
2. Right-click the root **Player** node → **Instance Child Scene**
3. Select `res://scenes/ui/health_bar_3d.tscn`
4. Select the new **HealthBar3D** node
5. In Inspector, set:
   - **Character:** `..` (drag the Player root node here)
   - **Is Friendly:** `true` (always show green for your own health)
   - **Offset Y:** `2.5` (adjust if needed)
6. Save the scene

### Step 3: Add Health Bar to Bots

1. Open `res://scenes/bots/assault_bot.tscn` (or any bot scene)
2. Right-click the root **AssaultBot** node → **Instance Child Scene**
3. Select `res://scenes/ui/health_bar_3d.tscn`
4. Select the new **HealthBar3D** node
5. In Inspector, set:
   - **Character:** `..` (drag the bot root node here)
   - **Is Friendly:** `false` (default for enemies - will be set dynamically later)
   - **Offset Y:** `2.5`
6. Save the scene
7. Repeat for all bot types (medic, spy, sniper)

### Step 4: Dynamic Team Colors (Optional - For Multiplayer)

In your bot setup code (e.g., in `assault_bot.gd`):

```gdscript
func _ready():
    super._ready()
    
    # Get the health bar node
    var health_bar = $HealthBar3D
    if health_bar:
        health_bar.character = self
        
        # Check if this bot is friendly to local player
        var local_peer_id = multiplayer.get_unique_id()
        health_bar.set_friendly(is_friendly_to(local_peer_id))
```

## Testing

1. **Pull the changes:**
   ```bash
   cd D:\Projects\neural-strike
   git pull origin main
   ```

2. **Open Godot and reload the project**

3. **Follow Steps 1-3 above**

4. **Run the game (F5)**
   - You should see a green health bar above the player
   - Enemy bots should have red health bars
   - Health bars always face the camera
   - Health bars update when taking damage

## Customization

### Change Colors

In `health_bar_3d.gd`, modify the `_update_color()` method:

```gdscript
if is_friendly:
    fill_style.bg_color = Color(0.1, 0.9, 0.1)  # Green
else:
    fill_style.bg_color = Color(0.9, 0.1, 0.1)  # Red
```

### Change Size

In `health_bar_3d.gd`, modify the `_ready()` method:

```gdscript
subviewport.size = Vector2(200, 20)  # Width, Height
progress_bar.custom_minimum_size = Vector2(200, 20)
sprite.pixel_size = 0.01  # Smaller = bigger in 3D space
```

### Change Height

In the Inspector for each HealthBar3D instance:
- Adjust **Offset Y** property (default: 2.5)

## Troubleshooting

**Q: Health bar doesn't appear**
- Make sure you set the `character` property in Inspector
- Check that the character has `current_health` and `max_health` properties

**Q: Health bar is the wrong color**
- Check the `is_friendly` property in Inspector
- For multiplayer, make sure you're calling `set_friendly()` in code

**Q: Health bar doesn't follow character**
- Make sure the HealthBar3D is a **child** of the character scene, not a separate instance
- Or keep it separate and assign `character` property

**Q: Health bar doesn't update**
- Make sure you're using `take_damage()` and `heal()` methods, not directly modifying health
- Or use `set_meta("current_health", value)` instead

## Next Steps

- Add damage numbers popup when hit
- Add shield bar (second progress bar)
- Add name labels above health bars
- Integrate with multiplayer team system
