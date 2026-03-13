using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles throwing grenades via G/U/V/E hotkeys.
/// Reads equipped grenades from PlayerLoadout.
/// Attach to the Player prefab.
/// </summary>
public class GrenadeController : MonoBehaviour
{
    [System.Serializable]
    public class GrenadePrefabEntry
    {
        public GrenadeType type;
        public GameObject  prefab;
    }

    [Header("Prefab Map (one entry per grenade type)")]
    public List<GrenadePrefabEntry> grenadePrefabs = new List<GrenadePrefabEntry>();

    [Header("Throw Settings")]
    public Transform throwOrigin;     // e.g. camera or weapon tip
    public float     throwForce = 12f;

    // Runtime: built from loadout on Start
    private struct GrenadeState
    {
        public GrenadeType type;
        public int         remaining;
        public KeyCode     hotkey;
        public GameObject  prefab;
    }

    private GrenadeState[] _slots;

    private void Start()
    {
        BuildSlotsFromLoadout();
    }

    private void BuildSlotsFromLoadout()
    {
        if (PlayerLoadout.Instance == null) return;

        var equipped = new List<GrenadeSlot>();
        foreach (var slot in PlayerLoadout.Instance.grenadeSlots)
            if (slot.points > 0)
                equipped.Add(slot);

        // max 4 equipped types mapped to G U V E
        int count = Mathf.Min(equipped.Count, 4);
        _slots = new GrenadeState[count];

        for (int i = 0; i < count; i++)
        {
            _slots[i] = new GrenadeState
            {
                type      = equipped[i].type,
                remaining = equipped[i].points,    // 1 point = 1 grenade
                hotkey    = PlayerLoadout.GrenadeHotkeys[i],
                prefab    = FindPrefab(equipped[i].type)
            };
        }
    }

    private void Update()
    {
        if (_slots == null) return;
        for (int i = 0; i < _slots.Length; i++)
        {
            if (Input.GetKeyDown(_slots[i].hotkey) && _slots[i].remaining > 0)
            {
                ThrowGrenade(i);
                break;
            }
        }
    }

    private void ThrowGrenade(int index)
    {
        if (_slots[index].prefab == null)
        {
            Debug.LogWarning($"No prefab assigned for {_slots[index].type}");
            return;
        }

        Transform origin = throwOrigin != null ? throwOrigin : transform;
        var go            = Instantiate(_slots[index].prefab,
                                        origin.position, origin.rotation);

        if (go.TryGetComponent(out Rigidbody rb))
            rb.AddForce(origin.forward * throwForce, ForceMode.VelocityChange);

        _slots[index].remaining--;
    }

    private GameObject FindPrefab(GrenadeType type)
    {
        foreach (var e in grenadePrefabs)
            if (e.type == type) return e.prefab;
        return null;
    }

    // ── Public accessors for HUD ─────────────────────────────────────
    public int GetSlotCount() => _slots?.Length ?? 0;
    public int GetRemaining(int i) => (i >= 0 && i < _slots.Length) ? _slots[i].remaining : 0;
    public GrenadeType GetType(int i) => (i >= 0 && i < _slots.Length) ? _slots[i].type : GrenadeType.Frag;
    public KeyCode GetHotkey(int i)  => (i >= 0 && i < _slots.Length) ? _slots[i].hotkey : KeyCode.None;
}
