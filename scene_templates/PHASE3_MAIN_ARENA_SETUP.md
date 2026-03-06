# PHASE 3: MAIN ARENA SCENE SETUP

**Target File:** `res://scenes/levels/main_arena.tscn`

---

## SCENE HIERARCHY

```
Node3D (Main_Arena)
├── WorldEnvironment
├── DirectionalLight3D (Sun)
├── Ground (CSGBox3D)
├── Walls (Node3D)
│   ├── WallNorth (CSGBox3D)
│   ├── WallSouth (CSGBox3D)
│   ├── WallEast (CSGBox3D)
│   └── WallWest (CSGBox3D)
└── SpawnPoints (Node3D)
    ├── SpawnAlpha1 (Marker3D)
    ├── SpawnAlpha2 (Marker3D)
    ├── SpawnAlpha3 (Marker3D)
    ├── SpawnAlpha4 (Marker3D)
    ├── SpawnBravo1 (Marker3D)
    ├── SpawnBravo2 (Marker3D)
    ├── SpawnBravo3 (Marker3D)
    └── SpawnBravo4 (Marker3D)
```

---

## STEP-BY-STEP CREATION

### 1. CREATE ROOT NODE
- Scene → New Scene → 3D Scene
- Root node: **Node3D** (rename to "Main_Arena")
- Scene → Save Scene As → `res://scenes/levels/main_arena.tscn`

---

### 2. ADD WORLDENVIRONMENT
- Select **Main_Arena** root
- Click **+** (Add Child Node)
- Search: `WorldEnvironment`
- Click **Create**

**Configure WorldEnvironment:**
- Inspector → Environment → Click dropdown → **New Environment**
- Will configure skybox in Step 15

---

### 3. ADD DIRECTIONALLIGHT3D (SUN)
- Select **Main_Arena** root  
- Add Child Node → `DirectionalLight3D`
- Rename to: **Sun**

**Configure Sun:**
- Inspector → Transform → Rotation (degrees):
  - X: `-45`
  - Y: `45`  
  - Z: `0`
- Light → Energy: `0.8`
- Shadow → Enabled: **ON** (checkmark)

---

### 4. ADD GROUND (CSGBox3D)
- Select **Main_Arena** root
- Add Child Node → `CSGBox3D`
- Rename to: **Ground**

**Configure Ground:**
- Inspector → Transform:
  - Position:
    - X: `0`
    - Y: `-0.5`
    - Z: `0`
  - Rotation: `0, 0, 0`
  - Scale: `1, 1, 1`

- CSGBox3D → Size:
  - X: `100`
  - Y: `1`
  - Z: `100`

- Material:
  - Click **Material** dropdown → **New StandardMaterial3D**
  - Click the material sphere icon to expand
  - Albedo → Click color box
  - Enter Hex: `#2a2a2a` (dark gray)
  - Click **OK**

---

### 5. ADD WALLS CONTAINER
- Select **Main_Arena** root
- Add Child Node → `Node3D`
- Rename to: **Walls**

---

### 6. ADD WALL NORTH
- Select **Walls** node
- Add Child Node → `CSGBox3D`
- Rename to: **WallNorth**

**Configure WallNorth:**
- Transform → Position: `0, 5, -50`
- CSGBox3D → Size: `100, 10, 2`
- Material → New StandardMaterial3D
  - Albedo → Color: `#4a4a4a` (light gray)

---

### 7. ADD WALL SOUTH
- Select **Walls** node
- Add Child Node → `CSGBox3D`
- Rename to: **WallSouth**

**Configure WallSouth:**
- Transform → Position: `0, 5, 50`
- CSGBox3D → Size: `100, 10, 2`
- Material → New StandardMaterial3D
  - Albedo → Color: `#4a4a4a`

---

### 8. ADD WALL EAST
- Select **Walls** node
- Add Child Node → `CSGBox3D`
- Rename to: **WallEast**

**Configure WallEast:**
- Transform → Position: `50, 5, 0`
- CSGBox3D → Size: `2, 10, 100`
- Material → New StandardMaterial3D
  - Albedo → Color: `#4a4a4a`

---

### 9. ADD WALL WEST
- Select **Walls** node
- Add Child Node → `CSGBox3D`
- Rename to: **WallWest**

**Configure WallWest:**
- Transform → Position: `-50, 5, 0`
- CSGBox3D → Size: `2, 10, 100`
- Material → New StandardMaterial3D
  - Albedo → Color: `#4a4a4a`

---

### 10. ADD SPAWNPOINTS CONTAINER
- Select **Main_Arena** root
- Add Child Node → `Node3D`
- Rename to: **SpawnPoints**

---

### 11. ADD ALPHA SPAWN POINTS (4 total)

**SpawnAlpha1:**
- Select **SpawnPoints**
- Add Child Node → `Marker3D`
- Rename to: **SpawnAlpha1**
- Transform → Position: `-40, 0.5, -40`

**SpawnAlpha2:**
- Select **SpawnPoints**
- Add Child Node → `Marker3D`
- Rename to: **SpawnAlpha2**
- Transform → Position: `-40, 0.5, -30`

**SpawnAlpha3:**
- Select **SpawnPoints**
- Add Child Node → `Marker3D`
- Rename to: **SpawnAlpha3**
- Transform → Position: `-30, 0.5, -40`

**SpawnAlpha4:**
- Select **SpawnPoints**
- Add Child Node → `Marker3D`
- Rename to: **SpawnAlpha4**
- Transform → Position: `-30, 0.5, -30`

---

### 12. ADD BRAVO SPAWN POINTS (4 total)

**SpawnBravo1:**
- Select **SpawnPoints**
- Add Child Node → `Marker3D`
- Rename to: **SpawnBravo1**
- Transform → Position: `40, 0.5, 40`

**SpawnBravo2:**
- Select **SpawnPoints**
- Add Child Node → `Marker3D`
- Rename to: **SpawnBravo2**
- Transform → Position: `40, 0.5, 30`

**SpawnBravo3:**
- Select **SpawnPoints**
- Add Child Node → `Marker3D`
- Rename to: **SpawnBravo3**
- Transform → Position: `30, 0.5, 40`

**SpawnBravo4:**
- Select **SpawnPoints**
- Add Child Node → `Marker3D`
- Rename to: **SpawnBravo4**
- Transform → Position: `30, 0.5, 30`

---

### 13. ADD SPAWN POINTS TO GROUPS

**Select ALL 8 Marker3D nodes:**
- Hold **Ctrl** (Windows/Linux) or **Cmd** (Mac)
- Click each Marker3D: SpawnAlpha1-4, SpawnBravo1-4

**Add to Group:**
- Node tab (next to Inspector) → Groups
- Type: `spawn_points`
- Click **Add**

---

### 14. ADD TEMPORARY CAMERA (FOR TESTING)

- Select **Main_Arena** root
- Add Child Node → `Camera3D`
- Rename to: **TestCamera**

**Configure TestCamera:**
- Transform → Position: `0, 10, 20`
- Transform → Rotation (degrees): `-20, 0, 0`
- Perspective → FOV: `75`

**Make it preview camera:**
- Click **TestCamera** node
- At top of 3D viewport, click **Preview** button (camera icon)

---

### 15. SAVE SCENE

- **Ctrl + S** or File → Save Scene
- Should save to: `res://scenes/levels/main_arena.tscn`

---

## ✅ VERIFICATION CHECKLIST

After completing setup, verify:

- [ ] Root node is **Node3D** named "Main_Arena"
- [ ] **WorldEnvironment** exists (will add skybox in Step 15)
- [ ] **Sun (DirectionalLight3D)** exists with shadows enabled
- [ ] **Ground** is 100x1x100 CSG box, dark gray color
- [ ] **4 Walls** surround the arena, light gray color
- [ ] **8 Spawn points** exist (4 Alpha, 4 Bravo)
- [ ] All spawn points are in "spawn_points" group
- [ ] **TestCamera** allows you to see the scene
- [ ] Scene saved to `res://scenes/levels/main_arena.tscn`

---

## 🎬 TEST THE SCENE

**Method 1: Run Scene Directly**
- Open `main_arena.tscn`
- Press **F6** (Run Current Scene)
- You should see: gray floor, gray walls, skybox placeholder

**Method 2: Frame View**
- Select **Ground** node
- Press **F** key (Frame Selected)
- Use middle mouse + drag to rotate view

---

## ⏭️ NEXT STEP

**After completing this scene:**
→ Continue to **Step 14: Generate Skybox with AI**
→ Apply skybox to WorldEnvironment

---

**Scene Template Version:** 1.0  
**Compatible with:** Godot 4.3+  
**Estimated Time:** 15-20 minutes
