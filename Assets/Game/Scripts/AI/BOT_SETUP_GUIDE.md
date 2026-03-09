# Bot AI Setup Guide

## Overview

Bot AI system with state machine (Idle, Patrol, Chase, Attack, Retreat) and 5 specialized roles:
- **Assault**: Aggressive flanking
- **Medic**: Heals allies, defensive
- **Sniper**: Long range, finds vantage points  
- **Spy**: Fast, backstabs from behind
- **Heavy**: High HP, suppressive fire, holds positions

---

## Prerequisites

1. **Batch 1 scripts** (Player, Weapon, Health) working
2. **Layers**:
   - `Player` (layer 6)
   - `Enemy` (layer 7)
3. **NavMesh baked** (see below)

---

## NavMesh Setup (REQUIRED)

1. Select all ground/walkable surfaces
2. Inspector: Check **Navigation Static**
3. Window → AI → Navigation
4. Settings:
   - Agent Radius: `0.5`
   - Agent Height: `2`
   - Max Slope: `45`
   - Step Height: `0.4`
5. Click **Bake**
6. Blue overlay = walkable NavMesh

---

## Bot Prefab Setup

### Base Bot (Assault)

1. Create Empty → `Bot_Assault`
2. Add Components:
   - `Capsule Collider`
   - `Rigidbody` (Kinematic ✓)
   - `NavMeshAgent`
   - `Health`
   - `BotController`
   - `BotPerception`
   - `AssaultBot`

3. **NavMeshAgent**:
   - Speed: `5`
   - Acceleration: `8`
   - Stopping Distance: `1`
   - Auto Braking: ✓
   - Radius: `0.5`
   - Height: `2`

4. **Health**:
   - Max Health: `100`
   - Destroy On Death: ✗

5. **BotController**:
   - Role: `Assault`
   - Perception: (assign BotPerception component)
   - Patrol Radius: `20`
   - Chase Distance: `30`
   - Attack Distance: `15`
   - Retreat Health Percent: `0.3`
   - Aim Speed: `5`
   - Fire Interval: `0.5`

6. **BotPerception**:
   - Vision Range: `30`
   - Vision Angle: `90`
   - Target Mask: `Player`
   - Obstacle Mask: `Default`
   - Hearing Range: `15`
   - Update Interval: `0.2`

7. **Add Weapon**:
   - Under Bot: Create `WeaponHolder` child (position `0,1,0`)
   - Under WeaponHolder: Create `Rifle` child
   - On Rifle: Add `HitscanWeapon`
   - Under Rifle: Create `Muzzle` child (position `0,0,1`)
   - Configure HitscanWeapon:
     - Damage: `25`
     - Fire Rate: `10`
     - Range: `150`
     - Hit Mask: `Player` + `Default`
     - Muzzle: (assign Muzzle transform)
   - On BotController:
     - Weapon: (assign HitscanWeapon)
     - Fire Point: (assign Muzzle)

8. **Set Layer**: Bot → `Enemy`

---

## Other Roles

### Medic

Duplicate Assault → `Bot_Medic`:
- Remove `AssaultBot`, add `MedicBot`
- MedicBot settings:
  - Heal Range: `10`
  - Heal Amount: `20`
  - Heal Cooldown: `5`
  - Ally Mask: `Enemy` (heals other bots)

### Sniper

Duplicate Assault → `Bot_Sniper`:
- Remove `AssaultBot`, add `SniperBot`
- Weapon:
  - Damage: `50`
  - Fire Rate: `2`
  - Spread: `0.5`
- BotController:
  - Attack Distance: `50`

### Spy

Duplicate Assault → `Bot_Spy`:
- Remove `AssaultBot`, add `SpyBot`
- SpyBot:
  - Backstab Angle: `60`
  - Backstab Multiplier: `3`
- BotController:
  - Attack Distance: `5`

### Heavy

Duplicate Assault → `Bot_Heavy`:
- Remove `AssaultBot`, add `HeavyBot`
- Weapon:
  - Damage: `15`
  - Fire Rate: `15`
  - Magazine: `100`
  - Spread: `3`
- Health: `150`

---

## Testing

1. Place bots on NavMesh (blue overlay)
2. Configure layers:
   - Player: `Player`
   - Bots: `Enemy`
   - BotPerception Target: `Player`
   - Weapons Hit Mask: `Player` + `Enemy`

3. Test states:
   - **Patrol**: Bot wanders randomly
   - **Chase**: Walk into vision range
   - **Attack**: Get within attack range
   - **Retreat**: Damage bot to low HP

---

## Debugging

**Bot doesn't move**:
- NavMesh baked?
- NavMeshAgent enabled?
- Bot on NavMesh surface?

**Bot doesn't see player**:
- Target Mask = `Player`?
- Player has `Player` layer?
- Check Gizmos (vision cone)

**Bot doesn't shoot**:
- Weapon assigned?
- Muzzle assigned?
- Hit Mask includes `Player`?
- Check bot state in Inspector

**No damage**:
- Player has `Health`?
- Player has collider?
- Check weapon raycast

---

## Gizmos

**BotController**:
- Blue: Patrol radius
- Yellow: Chase distance
- Red: Attack distance

**BotPerception**:
- Yellow: Vision range
- Cyan: Vision cone
- Green: Hearing range

**Roles**:
- Medic: Green (heal range)
- Sniper: Magenta (vantage point)
- Spy: Red cone (backstab)
- Heavy: Blue (hold position)

---

## Next Batch

Tracker bullets + Data knife + Battle Pad UI
