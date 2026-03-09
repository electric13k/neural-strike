using UnityEngine;
using System.Collections.Generic;

public class DeathmatchMode : GameMode
{
    [Header("Deathmatch Settings")]
    public int killsToWin = 30;
    
    private Dictionary<GameObject, int> scores = new Dictionary<GameObject, int>();
    
    public override void StartGame()
    {
        modeType = GameModeType.Deathmatch;
        modeName = "Deathmatch";
        scores.Clear();
        base.StartGame();
    }
    
    public override void OnPlayerKill(GameObject killer, GameObject victim)
    {
        AddScore(killer, 1);
        CheckWinCondition();
    }
    
    public override void OnBotKill(GameObject killer, GameObject victim)
    {
        AddScore(killer, 1);
        CheckWinCondition();
    }
    
    private void AddScore(GameObject entity, int points)
    {
        if (!scores.ContainsKey(entity))
            scores[entity] = 0;
        
        scores[entity] += points;
    }
    
    private void CheckWinCondition()
    {
        foreach (var kvp in scores)
        {
            if (kvp.Value >= killsToWin)
            {
                EndGame($"{kvp.Key.name} wins with {kvp.Value} kills!");
                break;
            }
        }
    }
    
    public int GetScore(GameObject entity)
    {
        return scores.ContainsKey(entity) ? scores[entity] : 0;
    }
    
    public Dictionary<GameObject, int> GetScores() { return new Dictionary<GameObject, int>(scores); }
}