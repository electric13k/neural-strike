using System.Collections;
using UnityEngine;

// ============================================================
//  GRENADE CONTROLLER  — Neural Strike
//  Manages grenade slots, throwing, and explosion.
//  Grenades are physics Rigidbody prefabs; explosion uses
//  Physics.OverlapSphere for AoE damage.
// ============================================================

public class GrenadeController : MonoBehaviour
{
    [Header("Slots  (assign prefabs in Inspector)")]
    public GameObject fragGrenadePrefab;
    public GameObject stickyGrenadePrefab;
    public GameObject vortexGrenadePrefab;

    [Header("Throw")]
    public Transform  throwOrigin;       // camera or hand
    public float      throwForce  = 15f;
    public KeyCode    throwKey    = KeyCode.G;
    public KeyCode    cycleKey    = KeyCode.T;

    [Header("Inventory")]
    public int fragCount   = 2;
    public int stickyCount = 1;
    public int vortexCount = 1;

    private int _slot;   // 0=frag 1=sticky 2=vortex

    void Update()
    {
        if (Input.GetKeyDown(cycleKey))
            _slot = (_slot + 1) % 3;

        if (Input.GetKeyDown(throwKey))
            ThrowCurrent();
    }

    void ThrowCurrent()
    {
        GameObject prefab = null;
        switch (_slot)
        {
            case 0:
                if (fragCount   <= 0) return;
                prefab = fragGrenadePrefab;
                fragCount--;
                break;
            case 1:
                if (stickyCount <= 0) return;
                prefab = stickyGrenadePrefab;
                stickyCount--;
                break;
            case 2:
                if (vortexCount <= 0) return;
                prefab = vortexGrenadePrefab;
                vortexCount--;
                break;
        }

        if (prefab == null) return;

        Vector3    pos = throwOrigin ? throwOrigin.position : transform.position + Vector3.up * 1.5f;
        Quaternion rot = throwOrigin ? throwOrigin.rotation : transform.rotation;
        GameObject g   = Instantiate(prefab, pos, rot);

        Rigidbody rb = g.GetComponent<Rigidbody>();
        if (rb != null)
            rb.AddForce(throwOrigin ? throwOrigin.forward * throwForce
                                     : transform.forward * throwForce,
                        ForceMode.VelocityChange);
    }
}
