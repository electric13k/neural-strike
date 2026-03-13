using UnityEngine;

/// <summary>
/// Local free-for-all test match (no networking): player vs bots.
/// </summary>
public class DeathmatchMode : GameMode
{
    [Header("References")]
    public GameManager  gameManager;
    public SpawnManager spawnManager;

    public override void Initialize()
    {
#if UNITY_2023_1_OR_NEWER
        if (gameManager  == null) gameManager  = Object.FindFirstObjectByType<GameManager>();
        if (spawnManager == null) spawnManager = Object.FindFirstObjectByType<SpawnManager>();
#else
        if (gameManager  == null) gameManager  = FindObjectOfType<GameManager>();
        if (spawnManager == null) spawnManager = FindObjectOfType<SpawnManager>();
#endif
    }

    public override void OnMatchStart()
    {
        Initialize();
        if (gameManager  != null) gameManager.StartMatch();
        if (spawnManager != null) spawnManager.SpawnInitialBots();
    }

    public override void OnMatchEnd()
    {
        if (gameManager != null) gameManager.EndMatch();
    }
}
