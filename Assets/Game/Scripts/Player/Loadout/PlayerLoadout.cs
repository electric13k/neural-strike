using System;
using System.Collections.Generic;
using UnityEngine;

public enum GrenadeType { Frag, Sticky, Impulse, Proximity, Smoke, TripMine }

[Serializable]
public class GrenadeSelection
{
    public GrenadeType type;
    [Range(0, 4)] public int points; // points = how many you carry
}

/// <summary>
/// Singleton loadout container. Persists from MainMenu into FFA scene.
/// Rules:
///   - 3 skill points shared between jump (0-3) and teleport (0-3).
///   - 4 grenade points across up to 4 equipped grenade types.
///   - Max 1 weapon prefab + max 1 melee prefab.
/// </summary>
public class PlayerLoadout : MonoBehaviour
{
    public static PlayerLoadout Instance { get; private set; }

    [Header("Weapon (max 1)")]
    public GameObject primaryWeaponPrefab;
    [Header("Melee (max 1)")]
    public GameObject meleePrefab;
    [Header("Bot skin")]
    public GameObject botPrefab;

    [Header("Skills – 3 points total")]
    [Range(0, 3)] public int jumpSkillPoints     = 1;
    [Range(0, 3)] public int teleportSkillPoints = 0;

    [Header("Grenades – 4 points, max 4 types equipped")]
    public List<GrenadeSelection> grenades = new List<GrenadeSelection>
    {
        new GrenadeSelection { type = GrenadeType.Frag,      points = 2 },
        new GrenadeSelection { type = GrenadeType.Sticky,    points = 1 },
        new GrenadeSelection { type = GrenadeType.Impulse,   points = 1 },
        new GrenadeSelection { type = GrenadeType.Proximity, points = 0 },
    };

    public int MaxSkillPoints   => 3;
    public int MaxGrenadePoints => 4;
    public int MaxGrenadeSlots  => 4;

    public int CurrentSkillPoints   => Mathf.Clamp(jumpSkillPoints, 0, 3)
                                     + Mathf.Clamp(teleportSkillPoints, 0, 3);
    public int CurrentGrenadePoints
    {
        get { int s = 0; foreach (var g in grenades) s += Mathf.Clamp(g.points, 0, 4); return s; }
    }
    public int EquippedGrenadeTypes
    {
        get { int n = 0; foreach (var g in grenades) if (g.points > 0) n++; return n; }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>Try to set jump+teleport. Returns false if total exceeds 3.</summary>
    public bool TrySetSkills(int newJump, int newTeleport)
    {
        newJump     = Mathf.Clamp(newJump,     0, 3);
        newTeleport = Mathf.Clamp(newTeleport, 0, 3);
        if (newJump + newTeleport > MaxSkillPoints) return false;
        jumpSkillPoints     = newJump;
        teleportSkillPoints = newTeleport;
        return true;
    }

    /// <summary>Try to set grenade point allocation. Enforces budget + slot cap.</summary>
    public bool TrySetGrenadePoints(int index, int newPoints)
    {
        if (index < 0 || index >= grenades.Count) return false;
        newPoints = Mathf.Clamp(newPoints, 0, 4);

        int old = grenades[index].points;
        grenades[index].points = newPoints;

        bool tooManyPoints = CurrentGrenadePoints > MaxGrenadePoints;
        bool tooManySlots  = EquippedGrenadeTypes > MaxGrenadeSlots;

        if (tooManyPoints || tooManySlots)
        {
            grenades[index].points = old;
            return false;
        }
        return true;
    }
}
