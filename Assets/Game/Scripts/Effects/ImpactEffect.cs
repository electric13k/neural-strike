using UnityEngine;

public class ImpactEffect : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject defaultImpactPrefab;
    public GameObject metalImpactPrefab;
    public GameObject concreteImpactPrefab;
    public GameObject fleshImpactPrefab;

    public static void SpawnImpact(Vector3 position, Vector3 normal, string surfaceType = "default")
    {
#if UNITY_2023_1_OR_NEWER
        ImpactEffect manager = Object.FindFirstObjectByType<ImpactEffect>();
#else
        ImpactEffect manager = FindObjectOfType<ImpactEffect>();
#endif
        if (manager == null) return;

        GameObject prefab = manager.GetImpactPrefab(surfaceType);
        if (prefab != null)
        {
            var impact = Instantiate(prefab, position, Quaternion.LookRotation(normal));
            Destroy(impact, 2f);
        }
    }

    private GameObject GetImpactPrefab(string surfaceType)
    {
        return surfaceType.ToLower() switch
        {
            "metal"    => metalImpactPrefab    ?? defaultImpactPrefab,
            "concrete" => concreteImpactPrefab ?? defaultImpactPrefab,
            "flesh"    => fleshImpactPrefab    ?? defaultImpactPrefab,
            _          => defaultImpactPrefab
        };
    }
}
