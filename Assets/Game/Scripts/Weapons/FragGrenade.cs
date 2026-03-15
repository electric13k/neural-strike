using System.Collections;
using UnityEngine;

// ============================================================
//  FRAG GRENADE  — Neural Strike
//  Attach to a Rigidbody prefab.
//  Explodes after fuseTime seconds, applies AoE damage.
// ============================================================

[RequireComponent(typeof(Rigidbody))]
public class FragGrenade : MonoBehaviour
{
    public float fuseTime       = 3f;
    public float blastRadius    = 6f;
    public float maxDamage      = 80f;
    public float knockbackForce = 8f;
    public LayerMask damageMask = ~0;

    public GameObject explosionFXPrefab;

    void Start() => StartCoroutine(Fuse());

    IEnumerator Fuse()
    {
        yield return new WaitForSeconds(fuseTime);
        Explode();
    }

    void Explode()
    {
        if (explosionFXPrefab)
            Destroy(Instantiate(explosionFXPrefab, transform.position, Quaternion.identity), 3f);

        Collider[] cols = Physics.OverlapSphere(transform.position, blastRadius, damageMask);
        foreach (Collider col in cols)
        {
            float dist    = Vector3.Distance(transform.position, col.transform.position);
            float falloff = 1f - Mathf.Clamp01(dist / blastRadius);
            float dmg     = maxDamage * falloff;

            HealthSystem hs = col.GetComponentInParent<HealthSystem>();
            if (hs != null) hs.TakeDamage(dmg, gameObject);

            Rigidbody rb = col.GetComponentInParent<Rigidbody>();
            if (rb != null)
            {
                Vector3 dir = (col.transform.position - transform.position).normalized;
                rb.AddForce(dir * knockbackForce * falloff, ForceMode.VelocityChange);
            }
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, blastRadius);
    }
}
