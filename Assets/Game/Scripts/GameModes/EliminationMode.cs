using UnityEngine;
using System.Collections.Generic;

public class EliminationMode : GameMode
{
    [Header("Elimination Settings")]
    public int roundsToWin = 3;
    
    private int playerRoundsWon;
    private int botRoundsWon;
    private HashSet<GameObject> aliveEntities = new HashSet<GameObject>();
    
    public override void StartGame()
    {
        modeType = GameModeType.Elimination;
        modeName = "Elimination";
        playerRoundsWon = 0;
        botRoundsWon = 0;
        base.StartGame();
    }
    
    protected override void StartRound()
    {
        base.StartRound();
        RegisterAllEntities();
    }
    
    private void RegisterAllEntities()
    {
        aliveEntities.Clear();
        
        // Register player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            aliveEntities.Add(player);
        
        // Register bots
        BotController[] bots = FindObjectsOfType<BotController>();
        foreach (BotController bot in bots)
        {
            aliveEntities.Add(bot.gameObject);
        }
    }
    
    public override void OnPlayerKill(GameObject killer, GameObject victim)
    {
        aliveEntities.Remove(victim);
        CheckEliminationCondition();
    }
    
    public override void OnBotKill(GameObject killer, GameObject victim)
    {
        aliveEntities.Remove(victim);
        CheckEliminationCondition();
    }
    
    private void CheckEliminationCondition()
    {
        bool playerAlive = false;
        int botsAlive = 0;
        
        foreach (GameObject entity in aliveEntities)
        {
            if (entity.CompareTag("Player"))
                playerAlive = true;
            else
                botsAlive++;
        }
        
        if (!playerAlive)
        {
            botRoundsWon++;
            EndRoundWithWinner("Bots");
        }
        else if (botsAlive == 0)
        {
            playerRoundsWon++;
            EndRoundWithWinner("Player");
        }
    }
    
    private void EndRoundWithWinner(string winner)
    {
        Debug.Log($"Round {currentRound} won by {winner}");
        EndRound();
        
        if (playerRoundsWon >= roundsToWin)
        {
            EndGame($"Player wins {playerRoundsWon}-{botRoundsWon}!");
        }
        else if (botRoundsWon >= roundsToWin)
        {
            EndGame($"Bots win {botRoundsWon}-{playerRoundsWon}!");
        }
        else
        {
            StartRound();
        }
    }
    
    public int GetPlayerRoundsWon() { return playerRoundsWon; }
    public int GetBotRoundsWon() { return botRoundsWon; }
}