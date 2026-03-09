using UnityEngine;
using UnityEngine.Events;

public enum GameModeType { Deathmatch, TeamDeathmatch, Elimination, CapturePoint, Custom }

[System.Serializable]
public class GameStartEvent : UnityEvent { }
[System.Serializable]
public class GameEndEvent : UnityEvent<string> { }
[System.Serializable]
public class RoundStartEvent : UnityEvent<int> { }
[System.Serializable]
public class RoundEndEvent : UnityEvent<int> { }

public abstract class GameMode : MonoBehaviour
{
    [Header("Mode Settings")]
    public GameModeType modeType;
    public string modeName;
    public float roundDuration = 300f;
    public int scoreLimit = 50;
    
    [Header("Events")]
    public GameStartEvent onGameStart;
    public GameEndEvent onGameEnd;
    public RoundStartEvent onRoundStart;
    public RoundEndEvent onRoundEnd;
    
    protected bool isActive;
    protected int currentRound;
    protected float roundTimeRemaining;
    
    public virtual void StartGame()
    {
        isActive = true;
        currentRound = 1;
        onGameStart.Invoke();
        StartRound();
    }
    
    public virtual void EndGame(string reason)
    {
        isActive = false;
        onGameEnd.Invoke(reason);
    }
    
    protected virtual void StartRound()
    {
        roundTimeRemaining = roundDuration;
        onRoundStart.Invoke(currentRound);
    }
    
    protected virtual void EndRound()
    {
        onRoundEnd.Invoke(currentRound);
        currentRound++;
    }
    
    protected virtual void Update()
    {
        if (!isActive) return;
        
        roundTimeRemaining -= Time.deltaTime;
        if (roundTimeRemaining <= 0f)
        {
            OnTimeExpired();
        }
    }
    
    protected virtual void OnTimeExpired() { EndRound(); }
    public virtual void OnPlayerKill(GameObject killer, GameObject victim) { }
    public virtual void OnBotKill(GameObject killer, GameObject victim) { }
    
    public bool IsActive() { return isActive; }
    public float GetRoundTimeRemaining() { return roundTimeRemaining; }
    public int GetCurrentRound() { return currentRound; }
}