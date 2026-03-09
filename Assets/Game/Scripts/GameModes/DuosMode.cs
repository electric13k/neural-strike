using UnityEngine;
using NeuralStrike.Core;

namespace NeuralStrike.GameModes
{
    /// <summary>
    /// Duos mode - teams of 2 players with color-coded teams.
    /// Each duo has combined score.
    /// </summary>
    public class DuosMode : MonoBehaviour, IGameMode
    {
        private GameManager gameManager;
        
        public void Initialize(GameManager manager)
        {
            gameManager = manager;
            Debug.Log("Duos mode initialized");
        }
        
        public void Update()
        {
            // Duos uses same scoring as TDM, just with smaller teams
        }
        
        public void OnKill(GameObject killer, GameObject victim)
        {
            // Scoring handled by GameManager
        }
    }
}