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
        ImpactEffect manager = FindObjectOfType<ImpactEffect>();
        if (manager == null) return;
        
        GameObject prefab = manager.GetImpactPrefab(surfaceType);
        if (prefab != null)
        {
            Quaternion rotation = Quaternion.LookRotation(normal);
            GameObject impact = Instantiate(prefab, position, rotation);
            Destroy(impact, 2f);
        }
    }
    
    private GameObject GetImpactPrefab(string surfaceType)
    {
        switch (surfaceType.ToLower())
        {
            case "metal": return metalImpactPrefab ?? defaultImpactPrefab;
            case "concrete": return concreteImpactPrefab ?? defaultImpactPrefab;
            case "flesh": return fleshImpactPrefab ?? defaultImpactPrefab;
            default: return defaultImpactPrefab;
        }
    }
}