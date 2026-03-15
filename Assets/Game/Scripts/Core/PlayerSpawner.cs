using UnityEngine;

// ============================================================
//  PLAYER SPAWNER  — Neural Strike
//
//  Place in every Game scene.  Picks a random spawn point and
//  instantiates the Player prefab at runtime, so the scene
//  itself stays prefab-clean.
//
//  HOW TO WIRE IN UNITY
//  1. Create empty GameObject "PlayerSpawner" in the Game scene.
//  2. Attach this script.
//  3. Assign playerPrefab  = Assets/Prefabs/Player.prefab
//  4. Add empty children "SpawnPoint_0", "SpawnPoint_1" etc.,
//     position them on the ground, drag into spawnPoints array.
// ============================================================

public class PlayerSpawner : MonoBehaviour
{
    [Header("Prefab & Points")]
    public GameObject playerPrefab;
    public Transform[] spawnPoints;

    void Start()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("[PlayerSpawner] playerPrefab is not assigned!");
            return;
        }
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[PlayerSpawner] No spawn points — spawning at origin.");
            Instantiate(playerPrefab, Vector3.zero + Vector3.up * 1f, Quaternion.identity);
            return;
        }

        Transform pt = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(playerPrefab, pt.position, pt.rotation);
        Debug.Log($"[PlayerSpawner] Spawned player at {pt.name}");
    }
}
