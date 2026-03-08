# Neural Strike – Unity 6 Port

This branch contains the **Unity 6** version of Neural Strike. The original
Godot version lives on the `main` branch.

## How to use this branch

1. **Clone the repo** (if you haven't already):
   ```bash
   git clone https://github.com/electric13k/neural-strike.git
   cd neural-strike
   ```
2. **Switch to the Unity branch**:
   ```bash
   git checkout unity
   ```
3. **Create the Unity project** using Unity Hub, pointing it to:
   ```
   <path-to-your-clone>/neural-strike/UnityProject
   ```
4. Commit the generated Unity project files on this branch.
5. After that, the assistant will provide C# scripts and setup guidance under:
   ```
   UnityProject/Assets/Game/
   ```

## Status

- `unity` branch: created.
- Unity project folder: **pending** (to be created by you via Unity Hub).
- C# gameplay code: will be added after the Unity project exists so .meta files and folders are correct.

## Design parity

The goal is for the Unity version to implement **all major mechanics** defined for Neural Strike:

- Core FPS movement and combat.
- Bot squads with roles (Medic, Sniper, Spy, etc.).
- Tracker bullets and remote viewing.
- Data Knife virus hacking.
- Battle Pad operator UI.
- All 4 game modes described in your docs.

Nothing will be skipped conceptually; some features may be staged in iterations (basic → advanced behaviour) for easier testing.
