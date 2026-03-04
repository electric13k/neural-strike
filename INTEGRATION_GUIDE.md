# INTEGRATION GUIDE: New Features

This guide explains how to integrate the 4 newly added systems into your Neural Strike game.

---

## 1. Tracker Bullets

**File:** `scripts/combat/tracker_bullet.gd`

### Setup:
1. Create a new scene `tracker_bullet.tscn` with root node type `Node3D`
2. Attach the `tracker_bullet.gd` script
3. Add this as a child of your projectile/bullet scene

### Usage in weapon code:
```gdscript
var tracker_bullet_scene = preload("res://scenes/tracker_bullet.tscn")

func fire_tracker_round():
    var tracker = tracker_bullet_scene.instantiate()
    tracker.initialize(player_id)  # Pass shooter's peer ID
    
    # When bullet hits:
    if hit_result:
        tracker.on_hit(target, hit_position, hit_normal)
```

### Features:
- Automatically creates a camera at hit location
- Camera follows target's movement
- Registers with Battle Pad for viewing
- Auto-cleans up after 30 seconds (configurable)
- Shows POV from head/torso/leg based on hit height

---

## 2. Data Knife Virus

**File:** `scripts/combat/data_knife_virus.gd`

### Setup:
1. Add `DataKnifeVirus` node to your player scene
2. Add it to the "data_knife" group: `add_to_group("data_knife")`
3. Connect player damage signal to `on_hacker_damaged()`

### Usage in player/melee code:
```gdscript
@onready var virus_system = $DataKnifeVirus

func _on_melee_attack_hit(target):
    if Input.is_action_pressed("interact"):  # Hold to hack
        if virus_system.start_hack(target, self):
            # Hack started successfully
            pass

func _on_player_damaged(amount):
    virus_system.on_hacker_damaged()  # Interrupt hack on damage
```

### Bot requirements:
Your bot script must implement:
```gdscript
func is_bot() -> bool:
    return true

func get_owner_id() -> int:
    return owner_peer_id  # Current owner's peer ID

func transfer_ownership(new_owner_id: int):
    owner_peer_id = new_owner_id
    # Change team color, AI behavior, etc.
```

### Features:
- Random hack duration (7-10 seconds)
- Progress bar integration with Battle Pad
- Interrupts if player moves >2m away or takes damage
- Transfers bot ownership on success

---

## 3. Battle Pad UI

**File:** `scripts/ui/battle_pad.gd`

### Setup:
1. Create a UI scene `battle_pad.tscn` with root `Control` node
2. Attach `battle_pad.gd` script
3. Add these child nodes:
   ```
   BattlePad (Control)
   ├─ MarginContainer
      ├─ VBoxContainer
         ├─ CameraFeeds (VBoxContainer)
         │  └─ GridContainer (empty, feeds added dynamically)
         ├─ BotControl (Panel)
         │  ├─ RoleLabel (Label)
         │  ├─ HealthBar (ProgressBar)
         │  ├─ FollowButton (Button)
         │  ├─ HoldButton (Button)
         │  ├─ FetchButton (Button)
         │  └─ AggressiveButton (Button)
         ├─ HackProgress (ProgressBar)
         └─ Minimap (Panel - placeholder)
   ```
4. Add to player's HUD as a child
5. **Input Map**: Create action `toggle_battle_pad` and bind to **Tab** key

### Connect button signals:
- `FollowButton.pressed` → `_on_bot_follow_pressed()`
- `HoldButton.pressed` → `_on_bot_hold_pressed()`
- `FetchButton.pressed` → `_on_bot_fetch_pressed()`
- `AggressiveButton.pressed` → `_on_bot_aggressive_pressed()`

### Usage:
```gdscript
# Camera feeds are registered automatically by tracker_bullet.gd
# Or manually:
var battle_pad = get_tree().get_first_node_in_group("battle_pad")
battle_pad.register_camera("spy_cam_1", my_camera, "Spy Bot")

# Assign player's bot:
battle_pad.set_bot(my_bot_node)
```

### Features:
- Toggle with Tab key
- Grid of camera feeds (tracker bullets, spy cams, etc.)
- Bot control panel with 4 command buttons
- Hack progress bar (auto-updates from virus system)
- Mouse cursor appears when open

---

## 4. Multiplayer Netcode

**File:** `scripts/networking/multiplayer_manager.gd`

### Setup:
1. **Autoload**: Go to `Project Settings > Autoload`
2. Add `MultiplayerManager` → `res://scripts/networking/multiplayer_manager.gd`
3. Enable as singleton

### Usage:

#### Start a server:
```gdscript
func _on_host_button_pressed():
    if MultiplayerManager.create_server():
        # Server started, load game level
        get_tree().change_scene_to_file("res://scenes/levels/main_arena.tscn")
```

#### Join a server:
```gdscript
func _on_join_button_pressed():
    var ip = $IPInput.text  # Get from LineEdit
    if MultiplayerManager.join_server(ip):
        # Connecting...
        pass

# Listen for connection result:
func _ready():
    MultiplayerManager.connection_succeeded.connect(_on_connected)
    MultiplayerManager.connection_failed.connect(_on_connection_failed)
```

#### Sync player data:
```gdscript
# Server updates score when player gets kill:
MultiplayerManager.update_player_score(killer_peer_id, 100)

# Any client can read player data:
var player_data = MultiplayerManager.get_player_data(peer_id)
print(player_data.name, player_data.team, player_data.score)
```

#### Assign bots to players:
```gdscript
# Server assigns bot when spawned:
var bot_node_id = bot.get_instance_id()
MultiplayerManager.assign_bot_to_player(player_peer_id, bot_node_id)
```

### Features:
- ENet peer-to-peer networking (up to 16 players)
- Auto team balancing (alpha/bravo)
- Player name, team, score, bot assignment sync
- RPC calls for reliable state sync
- Clean disconnect handling

---

## Quick Test Checklist

### Tracker Bullets:
- [ ] Fire tracker bullet at enemy
- [ ] Open Battle Pad (Tab)
- [ ] See camera feed from hit body part
- [ ] Camera moves with target for 30s

### Data Knife:
- [ ] Melee attack enemy bot while holding Interact
- [ ] See hack progress bar in Battle Pad
- [ ] Wait 7-10s without taking damage
- [ ] Bot changes to your team

### Battle Pad:
- [ ] Press Tab to open/close
- [ ] See registered camera feeds in grid
- [ ] Bot control buttons issue commands
- [ ] Hack progress bar updates during virus hack

### Multiplayer:
- [ ] Host creates server (port 7777)
- [ ] Client joins via IP
- [ ] Both players see each other's names
- [ ] Score updates sync to all clients
- [ ] Bot assignments sync correctly

---

## Next Steps

1. **Create UI scenes** for Battle Pad (use the node structure above)
2. **Implement bot roles** (Medic, Spy, Courier, Sniper) inheriting from `bot_base.gd`
3. **Add weapon mods system** (attachments, bullet types) extending `WeaponBase`
4. **Create multiplayer lobby** scene with Host/Join buttons
5. **Add scoring system** for kills, captures, hacks (integrate with MultiplayerManager)

All core systems are now in place. Focus on content creation (maps, weapons, bots) using the existing framework!
