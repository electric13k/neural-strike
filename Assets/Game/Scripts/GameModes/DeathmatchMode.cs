using UnityEngine;

/// <summary>
/// Local free-for-all test match (no networking): player vs bots.
/// Assign scoreToWin = 0 on GameManager for a timed-only match.
/// </summary>
public class DeathmatchMode : GameMode
{
    [Header("References")]
    public GameManager  gameManager;
    public SpawnManager spawnManager;

    public override void Initialize()
    {
        if (gameManager  == null) gameManager  = FindObjectOfType<GameManager>();
        if (spawnManager == null) spawnManager = FindObjectOfType<SpawnManager>();
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
