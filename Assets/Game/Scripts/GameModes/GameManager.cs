using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public float matchDuration = 600f;
    public int scoreToWin = 50;
    
    private Dictionary<string, int> teamScores = new Dictionary<string, int>();
    private float matchStartTime;
    private bool matchActive = false;
    
    public float TimeRemaining => Mathf.Max(0, matchDuration - (Time.time - matchStartTime));
    public bool MatchActive => matchActive;
    
    void Start()
    {
        StartMatch();
    }
    
    void Update()
    {
        if (!matchActive) return;
        
        if (TimeRemaining <= 0)
        {
            EndMatch();
        }
    }
    
    public void StartMatch()
    {
        matchStartTime = Time.time;
        matchActive = true;
        teamScores.Clear();
        teamScores["Team1"] = 0;
        teamScores["Team2"] = 0;
    }
    
    public void EndMatch()
    {
        matchActive = false;
        string winner = GetWinningTeam();
        Debug.Log($"Match ended! Winner: {winner}");
    }
    
    public void OnKill(GameObject killer, GameObject victim)
    {
        Health killerHealth = killer.GetComponent<Health>();
        Health victimHealth = victim.GetComponent<Health>();
        
        if (killerHealth == null || victimHealth == null) return;
        
        bool victimWasBot = victim.GetComponent<BotController>() != null;
        
        if (victimWasBot)
        {
            AddScore(killerHealth.Team, 2);
            AddScore(victimHealth.Team, -100);
        }
        else
        {
            AddScore(killerHealth.Team, 1);
            AddScore(victimHealth.Team, -1);
        }
    }
    
    public void AddScore(string team, int points)
    {
        if (!teamScores.ContainsKey(team))
            teamScores[team] = 0;
        
        teamScores[team] += points;
        
        if (scoreToWin > 0 && teamScores[team] >= scoreToWin)
        {
            EndMatch();
        }
    }
    
    public int GetScore(string team)
    {
        return teamScores.ContainsKey(team) ? teamScores[team] : 0;
    }
    
    private string GetWinningTeam()
    {
        string winner = "None";
        int highestScore = int.MinValue;
        
        foreach (var kvp in teamScores)
        {
            if (kvp.Value > highestScore)
            {
                highestScore = kvp.Value;
                winner = kvp.Key;
            }
        }
        
        return winner;
    }
}