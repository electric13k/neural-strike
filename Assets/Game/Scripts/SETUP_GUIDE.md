# Neural Strike - Unity Setup Guide

## Core FPS Scripts (Batch 1)

This batch includes:
- Player movement & camera
- Health & damage system
- Hitscan weapon system

---

## Folder Structure

```
Assets/Game/Scripts/
├── Player/
│   ├── PlayerController.cs
│   ├── MouseLook.cs
│   └── PlayerWeaponController.cs
├── Health/
│   ├── IDamageable.cs
│   ├── DamageInfo.cs
│   └── Health.cs
├── Weapons/
│   ├── Weapon.cs
│   └── HitscanWeapon.cs
├── Core/
│   └── CoroutineRunner.cs
└── SETUP_GUIDE.md (this file)
```

---

## Player Setup

### 1. Create Player GameObject

1. In Hierarchy: Create Empty → name it `Player`
2. Add Components:
   - `CharacterController`
   - `PlayerController` (script)
   - `PlayerWeaponController` (script)
   - `Health` (script)

### 2. CharacterController Settings

- Height: `2`
- Radius: `0.5`
- Center: `(0, 1, 0)`

### 3. Create Camera

1. Under Player, create Empty → name it `CameraRoot`
   - Position: `(0, 1.6, 0)` (eye height)
2. Under CameraRoot, create `Camera` (Main Camera)
   - Position: `(0, 0, 0)`
   - Add `MouseLook` script
   - Assign `playerBody` field = `Player` transform (drag from Hierarchy)

### 4. Create Ground Check

1. Under Player, create Empty → name it `GroundCheck`
   - Position: `(0, 0, 0)` (at player's feet)
2. On `PlayerController` component:
   - Assign `groundCheck` = `GroundCheck` transform
   - Set `groundCheckRadius` = `0.3`
   - Set `groundMask` = `Default` (or your ground layer)

---

## Weapon Setup

### 1. Create Weapon Holder

1. Under Player, create Empty → name it `WeaponHolder`
   - Position: `(0, 1.5, 0)` (chest height)
2. On `PlayerWeaponController`:
   - Assign `weaponHolder` = `WeaponHolder` transform

### 2. Create Rifle Weapon

1. Under WeaponHolder, create Empty → name it `Rifle`
   - Position: `(0.3, -0.2, 0.5)` (right side, slightly forward)
   - Add `HitscanWeapon` script
2. Under Rifle, create Empty → name it `Muzzle`
   - Position: `(0, 0, 1)` (at barrel end)
3. On `HitscanWeapon` component:
   - Assign `muzzle` = `Muzzle` transform
   - Set `hitMask` = `Default` + any enemy layers
   - Set `damage` = `25`
   - Set `fireRate` = `10` (600 RPM)
   - Set `range` = `150`
   - Set `magazineSize` = `30`
   - Set `ammoReserve` = `90`

4. On `PlayerWeaponController`:
   - Assign `currentWeapon` = `Rifle` HitscanWeapon component

---

## Input Settings

Unity's default Input Manager should work, but verify:

1. Edit → Project Settings → Input Manager
2. Required axes:
   - `Horizontal` (A/D, Left/Right arrows)
   - `Vertical` (W/S, Up/Down arrows)
   - `Mouse X` (mouse horizontal)
   - `Mouse Y` (mouse vertical)
   - `Jump` (Space)
   - `Fire1` (left mouse button)

All should be set up by default in new Unity projects.

---

## Layers Setup

1. Edit → Project Settings → Tags and Layers
2. Create layers:
   - `Player` (layer 6)
   - `Enemy` (layer 7)
3. Assign layers:
   - Player GameObject → Layer: `Player`
   - Ground/Environment → Layer: `Default`

---

## Testing

### 1. Create a Test Arena

1. Create a Plane (Ground):
   - Position: `(0, 0, 0)`
   - Scale: `(10, 1, 10)`
2. Add a Cube (Target):
   - Position: `(0, 1, 10)`
   - Add `Health` component
   - Set `maxHealth` = `100`

### 2. Test Movement

1. Press Play
2. Test:
   - WASD movement ✓
   - Mouse look ✓
   - Sprint (Left Shift) ✓
   - Jump (Space) ✓

### 3. Test Shooting

1. Aim at target cube
2. Click left mouse button
3. Cube should take damage (check console or add health bar)
4. Test reload (R key)

---

## Troubleshooting

### Player doesn't move
- Check `CharacterController` is enabled
- Check `PlayerController` script has no errors
- Verify Input axes are configured

### Mouse look doesn't work
- Check `MouseLook` script on Camera
- Verify `playerBody` is assigned to Player root
- Check cursor is locked (should be invisible)

### Shooting doesn't work
- Check `Muzzle` transform is assigned
- Verify `hitMask` includes target layers
- Check ammo is not 0
- Look for console warnings

### Ground check issues
- Check `GroundCheck` transform position (at feet)
- Verify `groundMask` includes ground layer
- Adjust `groundCheckRadius` if needed

---

## Next Steps

Once this batch is working:
- Next batch: Bot AI (NavMesh, perception, combat)
- Later batches: Tracker bullets, Data Knife, Battle Pad UI

---

## Notes

- All scripts use Unity 6 / 2025+ conventions
- Uses URP rendering pipeline
- Uses Unity's legacy Input Manager (can upgrade to new Input System later)
- Health system uses UnityEvents for easy UI hookup
- Weapon system is extensible (can add projectile weapons, etc.)
