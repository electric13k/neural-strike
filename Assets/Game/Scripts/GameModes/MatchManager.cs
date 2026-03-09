using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public GameMode currentGameMode;
    public SpawnManager spawnManager;
    public bool autoStartGame = true;
    public float gameStartDelay = 3f;
    
    private void Start()
    {
        if (autoStartGame)
        {
            Invoke(nameof(StartGame), gameStartDelay);
        }
    }
    
    public void StartGame()
    {
        if (currentGameMode != null)
        {
            currentGameMode.OnMatchStart();
        }
        
        if (spawnManager != null)
        {
            spawnManager.SpawnInitialBots();
        }
    }
}