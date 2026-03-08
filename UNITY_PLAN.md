# Unity 6 Migration Plan for Neural Strike

This branch (`unity`) is dedicated to the **Unity 6** version of Neural Strike. The Godot version stays on `main`.

## High-level strategy

- Keep **Godot** implementation on `main` untouched.
- Use **`unity`** branch for all Unity-specific work.
- Share raw assets (models, textures, audio, docs) between engines via the top-level `assets/` and `docs/` folders.
- Place the Unity project in `UnityProject/` so it never conflicts with Godot files.

## Target repo layout on this branch

```text
(neural-strike root)
├─ assets/                 # Shared raw content: models, textures, audio
├─ docs/                   # Design docs, game rules, etc.
├─ godot/                  # (optional) Godot project, if you keep a copy here
└─ UnityProject/           # Unity 6 project root (created by you via Unity Hub)
   ├─ Assets/
   │  ├─ Game/             # Our scripts, prefabs, scenes
   │  ├─ SharedAssets/     # Imported models/textures/audio from ../assets/
   │  └─ Scenes/
   ├─ ProjectSettings/
   ├─ Packages/
   └─ (Unity-generated .meta files, etc.)
```

## Workflow

- **Godot work** → `main` branch.
- **Unity work** → `unity` branch.
- **Shared assets/docs** → committed on both branches as needed.

You can switch locally with:

```bash
git checkout main    # Godot
git checkout unity   # Unity
```

## Next steps (you)

1. Open **Unity Hub**.
2. Create a **new 3D (URP or Core)** project **in this repo** at:
   - `path/to/neural-strike/UnityProject`
3. Commit the generated Unity project structure on the `unity` branch (Unity will create `Assets/`, `ProjectSettings/`, `Packages/`, etc.).
4. After that, we will start adding C# scripts under `UnityProject/Assets/Game/` and wiring them up in the editor.

## Next steps (assistant)

Once the Unity project folder exists in this branch, I will:

- Add a `UnityProject/Assets/Game/README.md` explaining structure and setup.
- Add core C# scripts for:
  - Player controller + mouse look.
  - Weapon and damage system.
  - Bot controller skeletons (roles: Medic, Sniper, Spy, etc.).
  - Tracker bullet and Data Knife mechanics.
  - Battle Pad manager and hooks for UI.
- Make sure nothing is skipped conceptually: every major mechanic documented in `GAME_MODES.md` and the design doc will have a Unity-side counterpart.

## Unity version

Use **Unity 6 (or latest 2025+ LTS)** with:
- Rendering pipeline: **URP** or **3D (HDRP later if you want)**.
- Scripting backend: **IL2CPP** for final builds (Mono is fine for development).

## .gitignore

Unity generates a lot of temporary files. This branch includes a Unity-aware `.gitignore` so only the necessary project files are tracked.
