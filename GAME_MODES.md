# NEURAL STRIKE - GAME MODES

**Neural Strike** features 4 distinct game modes with different objectives, team structures, and match lengths.

---

## 📊 SCORING SYSTEM (ALL MODES)

**Universal Score Formula:**
```
Score = (player_kills) + (robot_kills × 2) - (player_deaths) - (robot_deaths × 100)
```

**Key Points:**
- Killing a player: +1 point
- Killing a robot: +2 points (robots worth double)
- Player death: -1 point
- Robot death: -100 points (massive penalty)

**This scoring emphasizes keeping your robots alive while eliminating enemies.**

---

## 🎮 MODE 1: TEAM DEATHMATCH (TDM)

### **Overview:**
- **Teams:** 2 (Alpha & Bravo) OR 4 (Alpha, Bravo, Tango, Charlie)
- **Match Length:** 10-25 minutes
- **Objective:** Team with highest score wins

### **Victory Condition:**
- Highest score at time limit
- OR first team to score threshold (if set)

### **Gameplay:**
- Pure combat focus
- No capture points or flags
- Kill enemy players and robots
- Protect your robots (massive penalty if they die)
- Coordinate bot abilities (medic heals, sniper covers, spy scouts)

---

## 👥 MODE 2: DUOS

### **Overview:**
- **Teams:** Color-coded teams (2 players each)
  - Available colors: Red, Blue, Green, Yellow, Teal, Cyan, Purple, Pink, Orange
- **Match Length:** 10-15 minutes
- **Objective:** Duo with highest score wins

### **Victory Condition:**
- Highest combined score at time limit

### **Gameplay:**
- Small team coordination (2 players)
- Each player has their own bot
- 2 players + 2 bots = 4-unit squad
- Focus on teamwork and bot synergy
- Same scoring as TDM

---

## ⚔️ MODE 3: DEATHMATCH (DM)

### **Overview:**
- **Teams:** None (Free-for-all)
- **Match Length:** 7-12 minutes
- **Objective:** Player with highest individual score wins

### **Victory Condition:**
- Highest individual score at time limit

### **Gameplay:**
- Every player is an enemy
- You + your bot vs everyone
- Fast-paced, aggressive combat
- No team coordination
- Robot management is critical (you're outnumbered)

---

## 🚩 MODE 4: BASE CAPTURE

### **Overview:**
- **Teams:** 2 (Alpha & Bravo)
- **Match Length:** 15-40 minutes
- **Objective:** Capture all 4 enemy flags while protecting your own

### **Match Structure:**

#### **Phase 1: Preparation (10 minutes)**
- Teams spawn in their respective bases
- **Base Layout:** Each base divided into 4 land sectors
- Each sector has 1 fixed flag (cannot be moved)
- **Fortify your base:**
  - Place deployables (tripwires, land mines, cameras)
  - Position bots strategically
  - Set up defensive positions
- **Cannot attack** during prep phase (practice defenses only)

#### **Phase 2: Assault (15-40 minutes)**
- Attack enemy base and replace their flags
- Defend your own flags from enemy attackers
- **⚠️ CRITICAL: Only HUMAN PLAYERS can capture/replace flags**
- **Robots CANNOT capture flags** - they provide support only

### **Flag Mechanics:**

**Flag Replacement:**
1. **Human player** approaches enemy flag
2. Hold interaction key (F) for 3-5 seconds
3. Enemy flag is replaced with your team's flag
4. That land sector now belongs to your team

**Flag Recapture:**
- Enemy can reclaim lost sectors by replacing your flag
- Flags change hands repeatedly during battle

### **Victory Conditions:**

**Option A: Total Capture**
- Capture all 4 enemy flags
- Instant victory (no time limit needed)

**Option B: Majority Control at Time Limit**
- Control all 4 enemy sectors
- Retain at least 3 of your own sectors
- When time expires

**Defeat:**
- Lose more than 1 flag at time limit
- Lose all 4 flags before time expires

### **Gameplay Strategy:**

**Offensive:**
- Coordinate multi-sector attacks
- Use spy bots for reconnaissance
- Assault bots for front-line attack
- Sniper bots cover from distance
- **Remember: YOU must capture flags, bots cannot!**

**Defensive:**
- Guard critical flags
- Place tripwires/land mines around flags
- Use medic bots to sustain defenders
- Camera deployables for early warning
- Position sniper bots at vantage points

**Bot Roles in Base Capture:**
- ✅ **Assault bots:** Attack enemy players near flags
- ✅ **Medic bots:** Heal defenders
- ✅ **Sniper bots:** Cover flag zones from distance
- ✅ **Spy bots:** Scout enemy positions, mark targets
- ❌ **NO BOT can capture or replace flags!**

---

## 🤖 ROBOT BEHAVIOR (ALL MODES)

### **What Robots CAN Do:**
- Attack enemy players
- Attack enemy robots
- Heal friendly players (Medic)
- Provide reconnaissance (Spy)
- Fetch items/loot (Courier)
- Provide fire support (Sniper, Assault)
- Be hacked by enemy Data Knife

### **What Robots CANNOT Do:**
- ❌ Capture flags (Base Capture mode)
- ❌ Replace flags
- ❌ Interact with objectives
- ❌ Use deployables (grenades, tripwires, cameras)

**Robots are SUPPORT units, not objective-takers!**

---

## 📈 SCORE IMPACT EXAMPLES

### **TDM/Duos/DM Scenarios:**

**Scenario 1: Aggressive Push**
- Kill 3 enemy players: +3
- Kill 1 enemy robot: +2
- Die once: -1
- **Net Score: +4**

**Scenario 2: Robot Death Penalty**
- Kill 5 enemy players: +5
- Your robot dies: -100
- **Net Score: -95** (MASSIVE LOSS!)

**Scenario 3: Bot Protection**
- Kill 2 enemy players: +2
- Kill 2 enemy robots: +4
- Keep your robot alive: 0
- **Net Score: +6**

**Key Takeaway:** Protect your robot at all costs!

---

## 🎯 MODE SELECTION GUIDE

**Want fast-paced combat?**
→ **Deathmatch (7-12 min)**

**Want team coordination?**
→ **Team Deathmatch (10-25 min)** or **Duos (10-15 min)**

**Want strategic, objective-based gameplay?**
→ **Base Capture (15-40 min)**

**Want small squad tactics?**
→ **Duos (2 players + 2 bots)**

---

## 🗺️ MAP DESIGN CONSIDERATIONS

### **TDM/Duos/DM Maps:**
- Medium-large size
- Vertical gameplay (multi-level)
- Multiple engagement ranges
- Cover scattered throughout
- Loot spawns for weapons/ammo

### **Base Capture Maps:**
- Very large size
- Two distinct base areas (Alpha & Bravo)
- Each base has 4 clearly marked sectors
- Open areas between bases ("no man's land")
- Defensive positions around flags
- Long sightlines for sniper bots
- Cover for attackers to advance

---

## 🎮 DEPLOYMENT SPAWNING

**All Modes:**
- Players spawn at designated spawn points
- Bots spawn with their assigned player
- Spawn protection: 3 seconds invulnerability
- Cannot shoot during spawn protection

**Respawn Times:**
- **TDM/Duos/DM:** Immediate respawn (0s)
- **Base Capture:** 10-second respawn delay

**Robot Respawn:**
- Dead robots respawn with their player
- Hacked robots stay with new owner until:
  - Robot dies (respawns with original owner)
  - Original owner disconnects (robot is destroyed)

---

**For implementation details, see `/scripts/core/game_manager.gd` and `/scripts/networking/multiplayer_manager.gd`**
