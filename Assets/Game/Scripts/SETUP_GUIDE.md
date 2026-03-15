# Neural Strike — Unity Setup Guide

## Build Settings (File → Build Settings)
1. Add scenes in this order:
   - `MainMenu`     (index 0 — opens on launch)
   - `Game_TDM`     (index 1)
   - `Game_DM`      (index 2)
   - `Game_BaseCapture` (index 3)

---

## Layers (Edit → Project Settings → Tags & Layers)
Create these layers:
| Layer Name | Use |
|-----------|-----|
| Ground    | Floor, terrain, platforms |
| Player    | Player capsule |
| Bot       | AI bots |
| Projectile| Bullets / grenades |

---

## Player Prefab Hierarchy
```
Player  [CharacterController, PlayerController, HealthSystem, TeamMember,
         GrenadeController, TeleportAbility, AudioSource]
├── Camera  [Camera, AudioListener, MouseLook]
│   └── WeaponMount  [child: AssaultRifle or SniperRifle prefab]
├── GroundCheck  (empty, position = 0, -1.02, 0)
└── ModelRoot  [PlayerModelHook]
    └── YourCharacterMesh.fbx  (imported mesh, no Rigidbody)
```
### Wiring checklist
- `PlayerController.groundCheck`  → GroundCheck
- `PlayerController.groundMask`   → Ground layer
- `MouseLook.playerBody`          → Player root
- `MouseLook` lives on Camera child
- `GrenadeController.throwOrigin` → Camera
- `TeleportAbility` on Player root
- `PlayerModelHook.playerRoot`    → Player root
- `PlayerModelHook.cameraTransform` → Camera

---

## Game Scene Hierarchy
```
Game_TDM  (scene)
├── GameManager  [TeamDeathmatch, SceneBootstrap]
├── HUD  [Canvas, HUDManager]
│   ├── HealthSlider
│   ├── ArmourSlider
│   ├── AmmoText
│   ├── GrenadeText
│   ├── TeleportFill
│   └── TimerText
├── Environment
│   ├── Ground  (layer: Ground)
│   ├── FloodedWater  (plane at Y=0, water material)
│   └── Walls / Props
├── Lighting
│   ├── DirectionalLight
│   └── ReflectionProbe
├── SpawnPoints_Alpha  [PlayerSpawner (team=Alpha)]
│   ├── SpawnPoint_0 … SpawnPoint_3
└── SpawnPoints_Bravo  [PlayerSpawner (team=Bravo)]
    └── SpawnPoint_0 … SpawnPoint_3
```

---

## Main Menu Scene
```
MainMenu  (scene)
├── Main Camera
├── EventSystem
└── Canvas
    ├── Background Image  (cyberpunk art)
    ├── TitleText         "NEURAL STRIKE"
    ├── Panel_Main  [MainMenuController]
    │   ├── Button_PlayTDM   → MainMenuController.PlayTDM()
    │   ├── Button_PlayDM    → MainMenuController.PlayDM()
    │   ├── Button_Settings  → MainMenuController.OpenSettings()
    │   └── Button_Quit      → MainMenuController.QuitGame()
    └── Panel_Settings  (hidden by default)
        ├── SliderSensitivity
        └── Button_Back      → MainMenuController.CloseSettings()
```

---

## Flooded Ground
1. In Environment, create a Plane GameObject named `FloodedWater`.
2. Set Y = 0 (or wherever water surface should be).
3. Give it a blue/teal material with transparency.
4. SceneBootstrap.killPlaneY = -8  (adjust per level).
5. Set SceneBootstrap.respawnPoint to a safe spawn Transform.

---

## Common "Player doesn't move" Fixes
| Symptom | Fix |
|---------|-----|
| No movement at all | Check `groundMask` is set to Ground layer |
| Floats / falls through | CharacterController center/height wrong |
| Camera jitter | MouseLook sensitivity too high |
| Cursor stuck | MouseLook.lockCursor = true, no EventSystem conflict |
| Input not responding | Edit → Project Settings → Input Manager: ensure Horizontal/Vertical/Mouse X/Mouse Y axes exist |
