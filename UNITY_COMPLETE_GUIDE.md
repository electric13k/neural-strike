# Neural Strike - Complete Unity Setup Guide

## Overview

Complete FPS game with bots, tracker bullets, data knife hacking, and battle pad UI.

**All batches pushed:**
- ✅ Batch 1: Player movement, camera, health, weapons
- ✅ Batch 2: Bot AI (5 roles), NavMesh, perception
- ✅ Batch 3: Tracker bullets, data knife, camera feeds
- ✅ Batch 4: Battle Pad UI (bot management, commands)
- ✅ Batch 5: Game modes (Deathmatch, Elimination), spawning
- ✅ Batch 6: VFX/audio (muzzle flash, impacts, footsteps)

---

## Quick Start (30 minutes)

### 1. Pull Code

```bash
cd "D:\Projects\FPS GAME\FINAL\V2 Unity\fpsgame"
git pull origin unity
```

### 2. Open Project

- Open in Unity Hub
- Wait for scripts to compile
- Check Console for errors (should be none)

### 3. Setup Layers

Edit → Project Settings → Tags and Layers:
- Layer 6: `Player`
- Layer 7: `Enemy`

### 4. Bake NavMesh

1. Create ground plane (scale 10x10)
2. Mark as **Navigation Static**
3. Window → AI → Navigation
4. Click **Bake**

### 5. Create Player

Follow: `Assets/Game/Scripts/SETUP_GUIDE.md`

Quick version:
- Empty GameObject `Player`
- Add: CharacterController, PlayerController, PlayerWeaponController, Health, DataKnife
- Camera child with MouseLook
- Weapon child with TrackerWeapon
- Layer: `Player`

### 6. Create Bot

Follow: `Assets/Game/Scripts/AI/BOT_SETUP_GUIDE.md`

Quick version:
- Empty GameObject `Bot_Assault`
- Add: NavMeshAgent, Health, BotController, BotPerception, AssaultBot, HackableBot
- Weapon child with HitscanWeapon
- Layer: `Enemy`

### 7. Setup Game Manager

1. Create Empty `MatchManager`
2. Add: MatchManager, DeathmatchMode, SpawnManager
3. Assign references
4. Create spawn points (empty transforms)

### 8. Test Play

Press Play:
- WASD move
- Mouse look
- Click shoot
- R reload
- E hack bot (hold 3s)
- T switch to tracked view
- Tab open Battle Pad

---

## File Structure

```
Assets/Game/Scripts/
├── Player/
│   ├── PlayerController.cs
│   ├── MouseLook.cs
│   └── PlayerWeaponController.cs
├── Health/
│   ├── Health.cs
│   ├── IDamageable.cs
│   └── DamageInfo.cs
├── Weapons/
│   ├── Weapon.cs
│   ├── HitscanWeapon.cs
│   └── TrackerWeapon.cs
├── AI/
│   ├── BotController.cs
│   ├── BotPerception.cs
│   └── Roles/
│       ├── AssaultBot.cs
│       ├── MedicBot.cs
│       ├── SniperBot.cs
│       ├── SpyBot.cs
│       └── HeavyBot.cs
├── Special/
│   ├── TrackerBullet.cs
│   ├── DataKnife.cs
│   └── HackableBot.cs
├── Camera/
│   └── CameraFeedManager.cs
├── UI/
│   ├── BattlePadManager.cs
│   ├── BotListPanel.cs
│   ├── BotListEntry.cs
│   ├── CameraFeedPanel.cs
│   ├── CameraFeed.cs
│   └── CommandPanel.cs
├── GameModes/
│   ├── GameMode.cs
│   ├── DeathmatchMode.cs
│   ├── EliminationMode.cs
│   ├── SpawnManager.cs
│   └── MatchManager.cs
├── Effects/
│   ├── MuzzleFlash.cs
│   └── ImpactEffect.cs
├── Audio/
│   ├── AudioManager.cs
│   ├── WeaponAudio.cs
│   └── FootstepAudio.cs
└── Core/
    └── CoroutineRunner.cs
```

---

## Controls

### Movement
- **WASD**: Move
- **Mouse**: Look
- **Space**: Jump
- **Left Shift**: Sprint

### Combat
- **Left Click**: Fire weapon
- **R**: Reload
- **E** (hold 3s): Hack bot

### Camera
- **T**: Switch to tracked target
- **ESC**: Return to player view

### UI
- **Tab**: Toggle Battle Pad

---

## Features Breakdown

### Batch 1: Core FPS
- Character controller with WASD + jump
- Mouse look camera
- Health system with damage events
- Hitscan weapons with fire rate + spread
- Ammo + reload system

### Batch 2: Bot AI
- State machine: Idle, Patrol, Chase, Attack, Retreat
- Vision cone + hearing perception
- NavMesh pathfinding
- 5 roles:
  - **Assault**: Flanks enemies
  - **Medic**: Heals allies
  - **Sniper**: Long range, finds high ground
  - **Spy**: Backstabs (3x damage)
  - **Heavy**: 150 HP, suppressive fire

### Batch 3: Special Systems
- **Tracker bullets**: 20% chance to tag enemies
- **Camera switching**: Press T to view tracked target
- **Data knife**: Hold E for 3s to hack bots
- **Virus**: 5 dmg/sec for 10 seconds (50 total)

### Batch 4: Battle Pad UI
- Press Tab to open
- Bot list with health bars
- Camera feeds (render textures)
- Command panel (placeholder)
- Pauses game (Time.timeScale = 0)

### Batch 5: Game Modes
- **Deathmatch**: First to 30 kills
- **Elimination**: Best of 5 rounds
- Spawn system for players + bots
- Match manager with timer

### Batch 6: VFX/Audio
- Muzzle flash (particles + light)
- Impact effects (per surface type)
- Audio manager (pooled sources)
- Weapon sounds (fire, reload, dry)
- Footstep system

---

## Detailed Setup Guides

Each batch has detailed setup guide:

1. **Batch 1**: `Assets/Game/Scripts/SETUP_GUIDE.md`
2. **Batch 2**: `Assets/Game/Scripts/AI/BOT_SETUP_GUIDE.md`
3. **Batch 3**: `Assets/Game/Scripts/Special/SPECIAL_SYSTEMS_GUIDE.md`
4. **Batch 4**: (UI setup in Unity Editor)
5. **Batch 5**: (Game mode setup in scene)
6. **Batch 6**: (VFX/audio component hookup)

---

## Common Issues

### Scripts won't compile
- Close Unity
- Delete `Library/` folder
- Reopen project

### Player can't move
- Check CharacterController is enabled
- Check Input axes exist (Edit → Project Settings → Input)

### Bots don't move
- Check NavMesh is baked (blue overlay)
- Check bot is on NavMesh surface
- Check NavMeshAgent is enabled

### Weapons don't fire
- Check weapon is assigned to PlayerWeaponController
- Check muzzle transform is assigned
- Check hitMask includes target layers

### No sound
- Check AudioManager exists in scene
- Check AudioListener on camera
- Check audio clips are assigned

---

## Next Steps

### Polish (Do yourself)
1. Import gun models from Sketchfab
2. Create muzzle flash particle effects
3. Add impact decals
4. Create UI sprites for Battle Pad
5. Add background music
6. Create main menu scene

### Multiplayer (Future)
1. Install Netcode for GameObjects
2. Make player/bot prefabs networked
3. Add spawn synchronization
4. Test in local network

### Advanced Features (Future)
1. Exotic grenades (EMP, smoke, flashbang)
2. Killstreaks
3. Weapon attachments
4. Bot loadout customization
5. Spectator mode

---

## Performance Tips

- Bots update perception every 0.2s (not every frame)
- Audio uses pooled sources (reuses 10 AudioSources)
- Camera feeds use low-res render textures (256x144)
- Only one tracker bullet active at a time
- Battle Pad pauses game (Time.timeScale = 0)

---

## Credits

All code generated for Neural Strike Unity port.

Original Godot version: `main` branch

Unity version: `unity` branch

---

## Support

If you have issues:
1. Check the batch-specific setup guides
2. Check Console for error messages
3. Verify all components are assigned in Inspector
4. Test each batch incrementally

All core systems are complete and ready to wire up in Unity Editor!
