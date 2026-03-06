# NEURAL STRIKE

**Neural Strike** is a tactical cyberpunk + light-steampunk multiplayer FPS built with Godot 4. Fight alongside AI bots, hack enemy robots with a virus-loaded Data Knife, and dominate across 4 unique game modes: TDM, Duos, Deathmatch, and Base Capture.

---

## 🎮 GAME FEATURES

### **Core Gameplay:**
- **First-Person Tactical Combat** - Fast-paced gunplay with modular weapons
- **Bot Companions** - Each player controls 1 specialized AI bot (Assault, Medic, Sniper, Spy, Courier)
- **Data Knife Hacking** - Hack enemy bots and turn them to your side (7-10 second interaction)
- **Battle Pad Device** - View camera feeds, control bots, monitor deployables
- **Tracker Bullets** - Rare ammo with embedded cameras that show hit body part POV
- **Exotic Deployables** - Black hole grenades, white hole repulsors, electric tripwires, camera grenades

### **4 Game Modes:**
- **Team Deathmatch (TDM)** - 2 or 4 teams, 10-25 minutes
- **Duos** - 2-player squads, 10-15 minutes  
- **Deathmatch (DM)** - Free-for-all, 7-12 minutes
- **Base Capture** - Strategic flag warfare with 10-min prep phase, 15-40 minutes
  - **⚠️ Only human players can capture flags - robots provide support only!**

### **Scoring System (All Modes):**
```
Score = (player_kills) + (robot_kills × 2) - (player_deaths) - (robot_deaths × 100)
```
*Robot deaths have massive penalty - protect your bots!*

---

## 🤖 BOT TYPES & ROLES

### **Assault Bot**
- Aggressive front-line combatant
- Equipped with rapid-fire weapons
- High armor, medium speed
- **Cannot capture flags**

### **Medic Bot**
- Heals nearby allies up to capped amount
- Healing cooldown system
- Carries melee for self-defense
- **Cannot capture flags**

### **Sniper Bot**
- Long-range precision fire
- Finds elevated positions
- Stationary when engaging
- **Cannot capture flags**

### **Spy Bot**
- Stealth reconnaissance
- Onboard camera feed to Battle Pad
- Places camera grenades
- Marks enemy positions
- **Cannot capture flags**

### **Courier Bot**
- Fetches loot and items
- Delivers to master on command
- Can respond to requests from other bots (with approval)
- **Cannot capture flags**

**⚠️ IMPORTANT: Robots are support units - they CANNOT capture objectives!**

---

## 🚀 QUICK START & IMPLEMENTATION ROADMAP

### **Phase 0: Preparation (1-2 hours)**
1. Install Godot 4.3+, Blender 4.0, Audacity, Git
2. Clone this repository: `git clone https://github.com/electric13k/neural-strike.git`
3. Create AI tool accounts: Meshy.ai, Leonardo.ai, ElevenLabs, Suno.ai, Blockade Labs

### **Phase 1: Project Setup (30 minutes)**
1. Open project in Godot 4.3+
2. Configure Project Settings (input maps, display, rendering)
3. Create folder structure: `/scenes/`, `/assets/`, `/scripts/`
4. Set up autoloads: `GameManager`, `MultiplayerManager`

### **Phase 2: 3D Models (3-4 hours)**
1. Generate player model with AI (Meshy.ai)
2. Generate 4 bot models (Assault, Medic, Spy, Sniper)
3. Generate 3 weapon models (Assault Rifle, Data Knife, Black Hole Grenade)
4. Import all `.glb` files to Godot

### **Phase 3: Level Environment (2-3 hours)**
1. Generate map layout concept (Leonardo.ai)
2. Build arena geometry with CSG nodes
3. Generate cyberpunk skybox (Blockade Labs)
4. Configure lighting and WorldEnvironment

### **Phase 4: Player Character (2 hours)**
1. Build `player.tscn` with `CharacterBody3D`
2. Add first-person camera and mouse look
3. Integrate player model
4. Set up weapon viewmodel
5. Attach `player.gd` script

### **Phase 5: Bot AI (3-4 hours)**
1. Build base bot scene with NavigationAgent3D
2. Create 4 bot variants (Assault, Medic, Spy, Sniper)
3. Implement bot state machines
4. Add bot-to-bot communication

### **Phase 6: Weapons & Combat (3 hours)**
1. Create modular weapon system
2. Implement RayCast hitscan
3. Add tracker bullet cameras
4. Create black hole grenade physics

### **Phase 7: Textures & Materials (2 hours)**
1. Generate PBR textures (Leonardo.ai)
2. Apply materials to models
3. Configure emission for glowing elements

### **Phase 8: VFX & Particles (2 hours)**
1. Muzzle flash effects
2. Bullet impact particles
3. Hacking progress visuals
4. Black/white hole vortex effects

### **Phase 9: Audio & Music (1-2 hours)**
1. Generate gunshot SFX (ElevenLabs)
2. Generate background music (Suno.ai)
3. Add UI sounds
4. Configure audio buses

### **Phase 10: UI Creation (4-5 hours)**
1. Build Battle Pad interface
2. Create main menu
3. Build HUD (health, ammo, score)
4. Style with cyberpunk theme

### **Phase 11: Integration & Scripting (3-4 hours)**
1. Connect player to weapons
2. Set up multiplayer synchronization
3. Implement score system
4. Wire up Data Knife hacking

### **Phase 12: Testing & Polish (2-3 hours)**
1. Test single-player
2. Test multiplayer (LAN)
3. Performance optimization
4. Add polish effects (screen shake, tracers)

### **Phase 13: Build & Distribution (1 hour)**
1. Configure export settings
2. Export Windows/Linux builds
3. Create launcher script
4. Create GitHub release

### **Phase 14: Advanced Features (Optional, 5-10 hours)**
1. Add more game modes
2. Weapon attachment system
3. Bot role expansion
4. Additional maps

**Estimated Total Time: 40-60 hours (2-3 weeks at 2-3 hrs/day)**

---

## 📂 REPOSITORY STRUCTURE

```
neural-strike/
├── scripts/
│   ├── core/              # GameManager, HackingSystem
│   ├── bot_ai/            # Bot AI scripts (assault, medic, spy, sniper, courier)
│   ├── combat/            # WeaponBase, grenades, tracker bullets
│   ├── networking/        # MultiplayerManager, sync
│   └── ui/                # Battle Pad, HUD, menus
├── scenes/                # .tscn files (create during walkthrough)
├── assets/                # Models, textures, audio (generate during walkthrough)
├── AI_PROMPTS.md          # Master prompts for AI generation
├── ASSETS_AND_TOOLS.md    # Free AI tools and resources
├── GAME_MODES.md          # Detailed game mode documentation
├── INTEGRATION_GUIDE.md   # Step-by-step assembly guide
└── README.md              # This file
```

---

## 🛠️ REQUIREMENTS

### **Software:**
- **Godot Engine 4.3+** (free, open-source)
- **Git** for version control
- **Blender 4.0** (optional, for model editing)
- **Audacity** (optional, for audio cleanup)

### **AI Tools (Free Tiers):**
- **Meshy.ai** - 3D model generation (200 credits free)
- **Leonardo.ai** - Textures & concept art (150 tokens/day)
- **ElevenLabs** - Sound effects (10,000 chars/month)
- **Suno.ai** - Music generation (10 songs/day)
- **Blockade Labs** - Skyboxes (15 free)

---

## 📚 DOCUMENTATION

- **[GAME_MODES.md](GAME_MODES.md)** - Detailed breakdown of all 4 game modes
- **[AI_PROMPTS.md](AI_PROMPTS.md)** - AI generation prompts for models, textures, audio
- **[ASSETS_AND_TOOLS.md](ASSETS_AND_TOOLS.md)** - Free resources and tool links
- **[INTEGRATION_GUIDE.md](INTEGRATION_GUIDE.md)** - Step-by-step Godot assembly

---

## 🎨 ART STYLE

**Cyberpunk + Light Steampunk:**
- Neon-drenched megacity aesthetic
- Industrial machinery and gears
- Glowing accent lights (cyan, orange, purple)
- Weathered military textures
- Hard-surface sci-fi armor
- Inspired by Titanfall 2, Deus Ex, Apex Legends

**Stylized rather than ultra-realistic** - cheap assets and simple shaders still look great!

---

## 🌐 MULTIPLAYER

**Built on Godot 4's High-Level Multiplayer API:**
- Host/Join server system
- Player synchronization
- Bot ownership transfer (when hacked)
- Camera feed replication
- Deployable synchronization
- Score tracking across clients

**Supports:**
- LAN multiplayer
- Dedicated servers
- Up to 16 players (recommended: 4-8)

---

## 🔧 CUSTOMIZATION

### **Weapon Modding:**
- Extend `WeaponBase` class
- Add attachments (scopes, silencers, extended mags)
- Create custom bullet types (explosive, poison, tracker)

### **Bot Customization:**
- Adjust aggression factor
- Change loadouts
- Modify behavior states
- Add new bot roles

### **Map Creation:**
- Use CSG nodes for rapid prototyping
- Import custom models
- Set up NavMesh for bot pathfinding
- Place spawn points and flag zones

---

## 📜 LICENSE

This project is provided as-is for educational and hobbyist use.

**Assets:**
- AI-generated models (Meshy.ai) - Check Meshy's license
- AI-generated audio (ElevenLabs, Suno) - Check individual tool licenses
- Free textures from OpenGameArt.org - Respect CC0/CC-BY licenses

**Code:**
- GDScript in `/scripts/` is freely modifiable
- Attribution appreciated but not required

---

## 🤝 CONTRIBUTING

Contributions welcome! Areas for improvement:
- Additional bot roles
- New weapon types
- More game modes
- Performance optimizations
- UI/UX enhancements
- Bug fixes

**Please open issues or pull requests on GitHub.**

---

## 📞 SUPPORT

**Having trouble? Check these resources:**
- **[INTEGRATION_GUIDE.md](INTEGRATION_GUIDE.md)** - Detailed walkthrough
- **GitHub Issues** - Report bugs or ask questions
- **Godot Docs** - https://docs.godotengine.org
- **Godot Discord** - Community support

---

## 🎮 GET STARTED NOW!

1. **Clone the repo:** `git clone https://github.com/electric13k/neural-strike.git`
2. **Open in Godot 4.3+**
3. **Follow INTEGRATION_GUIDE.md**
4. **Start with Phase 0!**

**Build your cyberpunk FPS in 2-3 weeks!** 🚀

---

*Neural Strike - Hack. Fight. Dominate.*
