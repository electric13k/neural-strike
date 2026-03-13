using System;
using System.Collections.Generic;
using UnityEngine;

public enum GrenadeType { Frag, Sticky, Impulse, Proximity, Smoke, TripMine }

[Serializable]
public class GrenadeSlot
{
    public GrenadeType type;
    [Range(0, 4)] public int points;  // 1 pt = 1 grenade, 0 = not equipped
}

/// <summary>
/// Persistent loadout: survives scene loads (DontDestroyOnLoad).
/// Rules enforced:
///   Skills  : jumpPoints + teleportPoints <= 3
///   Grenades: sum of all slot points <= 4, max 4 distinct equipped types
///   Weapons : 1 primary + 1 melee only
/// </summary>
public class PlayerLoadout : MonoBehaviour
{
    public static PlayerLoadout Instance { get; private set; }

    [Header("Weapons (assign prefabs in Main Menu scene)")]
    public GameObject primaryWeaponPrefab;
    public GameObject meleeWeaponPrefab;
    public GameObject botPrefab;

    [Header("Skill Points (total 3)")]
    [Range(0, 3)] public int jumpSkillPoints      = 0;
    [Range(0, 3)] public int teleportSkillPoints  = 0;

    [Header("Grenade Points (total 4, max 4 types)")]
    public List<GrenadeSlot> grenadeSlots = new List<GrenadeSlot>
    {
        new GrenadeSlot { type = GrenadeType.Frag,      points = 0 },
        new GrenadeSlot { type = GrenadeType.Sticky,    points = 0 },
        new GrenadeSlot { type = GrenadeType.Impulse,   points = 0 },
        new GrenadeSlot { type = GrenadeType.Proximity, points = 0 },
        new GrenadeSlot { type = GrenadeType.Smoke,     points = 0 },
        new GrenadeSlot { type = GrenadeType.TripMine,  points = 0 },
    };

    // ── public helpers ───────────────────────────────────────────────
    public const int MaxSkillPoints   = 3;
    public const int MaxGrenadePoints = 4;
    public const int MaxGrenadeTypes  = 4;

    public int CurrentSkillPoints   => Mathf.Clamp(jumpSkillPoints, 0, 3)
                                     + Mathf.Clamp(teleportSkillPoints, 0, 3);

    public int CurrentGrenadePoints
    {
        get { int s = 0; foreach (var g in grenadeSlots) s += Mathf.Clamp(g.points,0,4); return s; }
    }

    public int EquippedGrenadeTypes
    {
        get { int s = 0; foreach (var g in grenadeSlots) if (g.points > 0) s++; return s; }
    }

    // ── skill mutation ───────────────────────────────────────────────
    public bool TrySetSkills(int newJump, int newTeleport)
    {
        newJump      = Mathf.Clamp(newJump,      0, MaxSkillPoints);
        newTeleport  = Mathf.Clamp(newTeleport,  0, MaxSkillPoints);
        if (newJump + newTeleport > MaxSkillPoints) return false;
        jumpSkillPoints     = newJump;
        teleportSkillPoints = newTeleport;
        return true;
    }

    // ── grenade mutation ─────────────────────────────────────────────
    public bool TrySetGrenadePoints(int slotIndex, int newPoints)
    {
        if (slotIndex < 0 || slotIndex >= grenadeSlots.Count) return false;
        newPoints         = Mathf.Clamp(newPoints, 0, MaxGrenadePoints);
        int old           = grenadeSlots[slotIndex].points;
        grenadeSlots[slotIndex].points = newPoints;

        bool tooManyPoints = CurrentGrenadePoints > MaxGrenadePoints;
        bool tooManyTypes  = EquippedGrenadeTypes > MaxGrenadeTypes;

        if (tooManyPoints || tooManyTypes)
        {
            grenadeSlots[slotIndex].points = old;  // rollback
            return false;
        }
        return true;
    }

    // ── keys for HUD: Q G U V E map to slots 0-4 ────────────────────
    public static readonly KeyCode[] GrenadeHotkeys =
    {
        KeyCode.G, KeyCode.U, KeyCode.V, KeyCode.E
    };

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
