using UnityEngine;
using System.Collections.Generic;
using NeuralStrike.Core;

namespace NeuralStrike.GameModes
{
    /// <summary>
    /// Central game manager controlling match flow, scoring, spawning, and win conditions.
    /// Handles all 4 game modes: TDM, Duos, DM, Base Capture.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Game Mode")]
        [SerializeField] private GameMode currentMode = GameMode.TeamDeathmatch;
        [SerializeField] private float matchDuration = 600f; // 10 minutes default
        [SerializeField] private int scoreToWin = 50; // Optional score threshold
        
        [Header("Teams")]
        [SerializeField] private int numberOfTeams = 2;
        
        [Header("Spawning")]
        [SerializeField] private float respawnDelay = 0f; // 0 for TDM/Duos/DM, 10 for Base Capture
        [SerializeField] private float spawnProtectionDuration = 3f;
        
        private Dictionary<Team, int> teamScores = new Dictionary<Team, int>();
        private Dictionary<Team, List<GameObject>> teamPlayers = new Dictionary<Team, List<GameObject>>();
        private Dictionary<Team, List<GameObject>> teamBots = new Dictionary<Team, List<GameObject>>();
        
        private float matchStartTime;
        private bool matchActive = false;
        private Team winningTeam = Team.Neutral;
        
        public GameMode CurrentMode => currentMode;
        public float TimeRemaining => Mathf.Max(0, matchDuration - (Time.time - matchStartTime));
        public bool MatchActive => matchActive;
        
        private IGameMode gameModeController;
        
        void Start()
        {
            InitializeGameMode();
            StartMatch();
        }
        
        void Update()
        {
            if (!matchActive) return;
            
            // Check time limit
            if (TimeRemaining <= 0)
            {
                EndMatch();
            }
            
            // Let game mode update
            gameModeController?.Update();
        }
        
        #region Match Flow
        
        private void InitializeGameMode()
        {
            // Create appropriate game mode controller
            switch (currentMode)
            {
                case GameMode.TeamDeathmatch:
                    gameModeController = gameObject.AddComponent<TeamDeathmatchMode>();
                    break;
                case GameMode.Duos:
                    gameModeController = gameObject.AddComponent<DuosMode>();
                    break;
                case GameMode.Deathmatch:
                    gameModeController = gameObject.AddComponent<DeathmatchMode>();
                    break;
                case GameMode.BaseCapture:
                    gameModeController = gameObject.AddComponent<BaseCaptureMode>();
                    break;
            }
            
            gameModeController?.Initialize(this);
        }
        
        public void StartMatch()
        {
            matchStartTime = Time.time;
            matchActive = true;
            
            // Initialize team scores
            teamScores.Clear();
            for (int i = 1; i <= numberOfTeams; i++)
            {
                teamScores[(Team)i] = 0;
            }
            
            Debug.Log($"Match started: {currentMode}, Duration: {matchDuration}s");
        }
        
        public void EndMatch()
        {
            matchActive = false;
            
            // Determine winner
            winningTeam = GetWinningTeam();
            
            Debug.Log($"Match ended! Winner: {winningTeam}, Score: {teamScores[winningTeam]}");
            
            // TODO: Show scoreboard, transition to next match
        }
        
        #endregion
        
        #region Scoring
        
        /// <summary>
        /// Universal scoring formula: (player_kills) + (robot_kills × 2) - (player_deaths) - (robot_deaths × 100)
        /// </summary>
        public void OnKill(GameObject killer, GameObject victim)
        {
            Health killerHealth = killer.GetComponent<Health>();
            Health victimHealth = victim.GetComponent<Health>();
            
            if (killerHealth == null || victimHealth == null) return;
            
            Team killerTeam = killerHealth.Team;
            
            // Determine if victim was player or bot
            bool victimWasBot = victim.GetComponent<BotController>() != null;
            
            if (victimWasBot)
            {
                // Robot kill: +2 points to killer's team
                AddScore(killerTeam, 2);
                
                // Robot death: -100 points to victim's team
                AddScore(victimHealth.Team, -100);
            }
            else
            {
                // Player kill: +1 point to killer's team
                AddScore(killerTeam, 1);
                
                // Player death: -1 point to victim's team
                AddScore(victimHealth.Team, -1);
            }
            
            gameModeController?.OnKill(killer, victim);
        }
        
        public void AddScore(Team team, int points)
        {
            if (!teamScores.ContainsKey(team))
                teamScores[team] = 0;
            
            teamScores[team] += points;
            
            // Check for score-based win condition
            if (scoreToWin > 0 && teamScores[team] >= scoreToWin)
            {
                EndMatch();
            }
        }
        
        public int GetScore(Team team)
        {
            return teamScores.ContainsKey(team) ? teamScores[team] : 0;
        }
        
        private Team GetWinningTeam()
        {
            Team winner = Team.Neutral;
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
        
        #endregion
        
        #region Spawning
        
        public void SpawnPlayer(GameObject player, Team team)
        {
            Transform spawnPoint = GetSpawnPoint(team);
            if (spawnPoint != null)
            {
                player.transform.position = spawnPoint.position;
                player.transform.rotation = spawnPoint.rotation;
                
                // Apply spawn protection
                Health health = player.GetComponent<Health>();
                if (health != null)
                {
                    health.SetInvulnerable(true);
                    StartCoroutine(RemoveSpawnProtection(health));
                }
            }
        }
        
        private Transform GetSpawnPoint(Team team)
        {
            // Find spawn points tagged for this team
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag($"SpawnPoint_{team}");
            if (spawnPoints.Length == 0)
            {
                spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
            }
            
            if (spawnPoints.Length > 0)
            {
                return spawnPoints[Random.Range(0, spawnPoints.Length)].transform;
            }
            
            return null;
        }
        
        private System.Collections.IEnumerator RemoveSpawnProtection(Health health)
        {
            yield return new WaitForSeconds(spawnProtectionDuration);
            health.SetInvulnerable(false);
        }
        
        #endregion
    }
    
    public enum GameMode
    {
        TeamDeathmatch,
        Duos,
        Deathmatch,
        BaseCapture
    }
    
    /// <summary>
    /// Interface for all game mode implementations.
    /// </summary>
    public interface IGameMode
    {
        void Initialize(GameManager manager);
        void Update();
        void OnKill(GameObject killer, GameObject victim);
    }
}