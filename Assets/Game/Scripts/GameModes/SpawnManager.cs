using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform[] playerSpawnPoints;
    public Transform[] botSpawnPoints;
    
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject[] botPrefabs;
    
    [Header("Settings")]
    public int botsToSpawn = 4;
    public float spawnDelay = 5f;
    
    public void SpawnInitialBots()
    {
        for (int i = 0; i < botsToSpawn && i < botSpawnPoints.Length; i++)
        {
            SpawnBot(i);
        }
    }
    
    private void SpawnBot(int index)
    {
        if (botPrefabs.Length == 0 || botSpawnPoints.Length == 0) return;
        
        GameObject botPrefab = botPrefabs[Random.Range(0, botPrefabs.Length)];
        Transform spawnPoint = botSpawnPoints[index % botSpawnPoints.Length];
        
        GameObject bot = Instantiate(botPrefab, spawnPoint.position, spawnPoint.rotation);
        bot.name = $"Bot_{index}";
    }
}