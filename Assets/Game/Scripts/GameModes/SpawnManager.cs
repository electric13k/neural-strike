using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform[] playerSpawnPoints;
    public Transform[] botSpawnPoints;
    
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject[] botPrefabs;
    
    [Header("Settings")]
    public int botsToSpawn = 8;
    public float spawnDelay = 5f;
    
    private List<GameObject> spawnedBots = new List<GameObject>();
    
    public void SpawnPlayer()
    {
        if (playerPrefab == null || playerSpawnPoints.Length == 0) return;
        
        Transform spawnPoint = playerSpawnPoints[Random.Range(0, playerSpawnPoints.Length)];
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        player.tag = "Player";
    }
    
    public void SpawnBots()
    {
        if (botPrefabs.Length == 0 || botSpawnPoints.Length == 0) return;
        
        for (int i = 0; i < botsToSpawn; i++)
        {
            SpawnBot();
        }
    }
    
    public void SpawnBot()
    {
        if (botPrefabs.Length == 0 || botSpawnPoints.Length == 0) return;
        
        GameObject botPrefab = botPrefabs[Random.Range(0, botPrefabs.Length)];
        Transform spawnPoint = botSpawnPoints[Random.Range(0, botSpawnPoints.Length)];
        
        GameObject bot = Instantiate(botPrefab, spawnPoint.position, spawnPoint.rotation);
        spawnedBots.Add(bot);
    }
    
    public void RespawnEntity(GameObject prefab, bool isPlayer)
    {
        Transform[] spawnPoints = isPlayer ? playerSpawnPoints : botSpawnPoints;
        if (spawnPoints.Length == 0) return;
        
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject entity = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        
        if (isPlayer)
            entity.tag = "Player";
        else
            spawnedBots.Add(entity);
    }
    
    public void ClearAllBots()
    {
        foreach (GameObject bot in spawnedBots)
        {
            if (bot != null)
                Destroy(bot);
        }
        spawnedBots.Clear();
    }
    
    public List<GameObject> GetSpawnedBots() { return new List<GameObject>(spawnedBots); }
}