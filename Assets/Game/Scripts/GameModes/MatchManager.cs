using UnityEngine;
using TMPro;

public class MatchManager : MonoBehaviour
{
    [Header("References")]
    public GameMode currentGameMode;
    public SpawnManager spawnManager;
    public TextMeshProUGUI gameStatusText;
    
    [Header("Settings")]
    public bool autoStartGame = true;
    public float gameStartDelay = 3f;
    
    private void Start()
    {
        if (autoStartGame)
        {
            Invoke(nameof(StartMatch), gameStartDelay);
        }
    }
    
    public void StartMatch()
    {
        if (currentGameMode == null)
        {
            Debug.LogError("No game mode assigned!");
            return;
        }
        
        // Spawn entities
        if (spawnManager != null)
        {
            spawnManager.SpawnPlayer();
            spawnManager.SpawnBots();
        }
        
        // Start game mode
        currentGameMode.StartGame();
        
        // Subscribe to events
        currentGameMode.onGameStart.AddListener(OnGameStart);
        currentGameMode.onGameEnd.AddListener(OnGameEnd);
        currentGameMode.onRoundStart.AddListener(OnRoundStart);
        currentGameMode.onRoundEnd.AddListener(OnRoundEnd);
        
        UpdateStatusText($"{currentGameMode.modeName} started!");
    }
    
    public void EndMatch()
    {
        if (currentGameMode != null)
        {
            currentGameMode.EndGame("Match ended by manager");
        }
    }
    
    private void OnGameStart()
    {
        Debug.Log("Game started!");
    }
    
    private void OnGameEnd(string reason)
    {
        Debug.Log($"Game ended: {reason}");
        UpdateStatusText(reason);
    }
    
    private void OnRoundStart(int round)
    {
        Debug.Log($"Round {round} started");
        UpdateStatusText($"Round {round}");
    }
    
    private void OnRoundEnd(int round)
    {
        Debug.Log($"Round {round} ended");
    }
    
    private void UpdateStatusText(string text)
    {
        if (gameStatusText != null)
            gameStatusText.text = text;
    }
    
    private void Update()
    {
        if (currentGameMode != null && currentGameMode.IsActive())
        {
            float timeRemaining = currentGameMode.GetRoundTimeRemaining();
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            UpdateStatusText($"{currentGameMode.modeName} - {minutes:00}:{seconds:00}");
        }
    }
}