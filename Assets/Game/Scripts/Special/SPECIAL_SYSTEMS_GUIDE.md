# Special Systems Setup Guide

## Batch 3: Tracker Bullets, Data Knife, Camera Feeds

This batch includes:
- **Tracker Bullets**: 20% chance to tag enemies, allows camera switching
- **Data Knife**: Hold E to hack bots (3s), applies virus damage
- **Camera Feed Manager**: Switch view to tracked targets with T key

---

## 1. Tracker Bullets

### Setup

1. **Create Tracker Weapon**:
   - Duplicate your existing rifle prefab → rename to `TrackerRifle`
   - Remove `HitscanWeapon` component
   - Add `TrackerWeapon` component instead
   - Configure:
     - All normal weapon settings (damage, fire rate, etc.)
     - Tracker Chance: `0.2` (20% chance)

2. **Give to Player**:
   - On `PlayerWeaponController`, assign `currentWeapon` = `TrackerWeapon`

### How It Works

- Each shot has 20% chance to attach invisible tracker
- Only one tracker active at a time (new tracker replaces old)
- Tracker lasts 30 seconds
- Press **T** to switch camera to tracked target
- Press **ESC** to return to player view

---

## 2. Data Knife

### Setup

1. **Add to Player**:
   - Select Player GameObject
   - Add `DataKnife` component
   - Configure:
     - Hack Range: `3`
     - Hack Duration: `3` (seconds to hold E)
     - Hackable Mask: `Enemy` layer
     - Hack Key: `E`

2. **Make Bots Hackable**:
   - Select bot prefab(s)
   - Add `HackableBot` component
   - Configure:
     - Hack Prompt: `"[E] Hack Bot"`
     - Virus Duration: `10` (seconds)
     - Virus Damage Per Second: `5`

### How It Works

1. Get within 3m of bot, face it
2. **Hold E** for 3 seconds
3. Bot is disabled for 2 seconds
4. Virus activates: 5 damage/sec for 10 seconds (50 total damage)
5. Bot re-enables but continues taking virus damage

### Events (for UI later)

`DataKnife` has UnityEvents:
- `onHackStart` - when hack begins
- `onHackComplete` - when hack succeeds
- `onHackCancel` - when hack is cancelled (moved away, released E)

Use `GetHackProgress()` to show progress bar (returns 0-1).

---

## 3. Camera Feed Manager

### Setup

1. **Add to Scene**:
   - Create Empty GameObject → name it `CameraFeedManager`
   - Add `CameraFeedManager` component
   - Configure:
     - Main Camera: (drag Main Camera)
     - Switch To Tracker Key: `T`
     - Return To Player Key: `Escape`

### How It Works

**Switching Views**:
1. Shoot enemy with tracker bullet (20% chance)
2. Press **T** to switch camera to tracked enemy
3. Camera follows tracked enemy
4. Press **ESC** to return to player

**View Modes**:
- Player view: Normal FPS camera
- Tracked view: Third-person camera behind tracked enemy

---

## Testing

### Test Tracker Bullets

1. Equip `TrackerRifle`
2. Shoot at bot multiple times (20% chance per shot)
3. When tracker attaches, console shows: `"Tracker attached to Bot_X"`
4. Press **T** → camera should switch to bot
5. Press **ESC** → camera returns to player

### Test Data Knife

1. Run up to bot (within 3m)
2. Face bot directly
3. **Hold E** for 3 seconds
4. Bot should freeze briefly, then resume
5. Bot takes 5 damage/sec for 10 seconds
6. Watch bot health decrease

### Test Combined

1. Tag bot with tracker
2. Hack bot with data knife
3. Switch camera to tracked bot with **T**
4. Watch from bot's perspective as virus kills it

---

## Input Summary

- **Left Click**: Fire weapon
- **R**: Reload
- **E** (hold): Hack nearby bot
- **T**: Switch camera to tracked target
- **ESC**: Return to player camera

---

## Debugging

### Tracker not working
- Check console for "Tracker attached" message
- 20% chance → may take 5-10 shots
- Only one tracker at a time

### Can't switch to tracker view
- Check if tracker is active (shoot more)
- Check `CameraFeedManager` has Main Camera assigned
- Check console for "No tracked target" message

### Data knife not working
- Check distance (must be < 3m)
- Check you're facing bot (use center of screen)
- Check bot has `HackableBot` component
- Check `Hackable Mask` includes `Enemy` layer
- Must **hold** E for 3 seconds (don't tap)

### Hack progress not showing
- Use `DataKnife.GetHackProgress()` in UI code
- Returns 0.0 to 1.0 during hack
- Hook into `onHackStart` event to show UI

---

## Advanced: Custom Hackables

To make other objects hackable:

```csharp
public class CustomHackable : MonoBehaviour, IHackable
{
    public void OnHackComplete(GameObject hacker)
    {
        // Your hack logic here
        Debug.Log("Custom object hacked!");
    }
    
    public string GetHackPrompt()
    {
        return "[E] Hack Custom Object";
    }
}
```

Add to any GameObject, set layer to match `Hackable Mask`.

---

## Next Batch

Battle Pad UI:
- Bot list panel
- Multiple camera feeds
- Bot command system
- Health/status displays

---

## Performance Notes

- Only one tracker active at a time (low overhead)
- Data knife raycasts only when player presses E
- Camera switching is instant (no rendering overhead)
- Virus damage calculated per-frame but very lightweight
