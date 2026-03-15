using UnityEngine;

// ============================================================
//  GRENADE THROW CONTROLLER  — Neural Strike
//  Rename note: class is now GrenadeThrowController to avoid
//  collision with any old GrenadeController in the project.
//  If you had GrenadeController referenced elsewhere, use
//  Find & Replace: GrenadeController → GrenadeThrowController
// ============================================================

public class GrenadeThrowController : MonoBehaviour
{
    [Header("Grenade Prefabs  (assign in Inspector)")]
    public GameObject fragGrenadePrefab;
    public GameObject stickyGrenadePrefab;
    public GameObject vortexGrenadePrefab;

    [Header("Throw Settings")]
    public Transform throwOrigin;        // Camera or hand bone
    public float     throwForce  = 15f;
    public KeyCode   throwKey    = KeyCode.G;
    public KeyCode   cycleKey    = KeyCode.T;

    [Header("Inventory")]
    public int fragCount   = 2;
    public int stickyCount = 1;
    public int vortexCount = 1;

    // 0=frag  1=sticky  2=vortex
    private int _slot;

    // ── Read-only properties for HUD ─────────────────────────
    public int  CurrentSlot  => _slot;
    public int  CurrentCount => _slot == 0 ? fragCount : _slot == 1 ? stickyCount : vortexCount;
    public string SlotName   => _slot == 0 ? "FRAG" : _slot == 1 ? "STICKY" : "VORTEX";

    void Update()
    {
        if (Input.GetKeyDown(cycleKey))
            _slot = (_slot + 1) % 3;

        if (Input.GetKeyDown(throwKey))
            ThrowCurrent();
    }

    public void ThrowCurrent()
    {
        GameObject prefab;
        switch (_slot)
        {
            case 0:
                if (fragCount   <= 0) { Debug.Log("No frags left"); return; }
                prefab = fragGrenadePrefab;
                fragCount--;
                break;
            case 1:
                if (stickyCount <= 0) { Debug.Log("No stickies left"); return; }
                prefab = stickyGrenadePrefab;
                stickyCount--;
                break;
            default:
                if (vortexCount <= 0) { Debug.Log("No vortex grenades left"); return; }
                prefab = vortexGrenadePrefab;
                vortexCount--;
                break;
        }

        if (prefab == null)
        {
            Debug.LogWarning($"[GrenadeThrowController] Slot {_slot} prefab not assigned!");
            return;
        }

        Vector3    origin = throwOrigin ? throwOrigin.position : transform.position + Vector3.up * 1.5f;
        Quaternion rot    = throwOrigin ? throwOrigin.rotation : transform.rotation;
        GameObject go     = Instantiate(prefab, origin, rot);

        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 dir = throwOrigin ? throwOrigin.forward : transform.forward;
            rb.AddForce(dir * throwForce, ForceMode.VelocityChange);
        }
    }

    // ── Called by loot pickups ────────────────────────────────
    public void AddGrenade(int slot, int amount)
    {
        switch (slot)
        {
            case 0: fragCount   += amount; break;
            case 1: stickyCount += amount; break;
            case 2: vortexCount += amount; break;
        }
    }
}
