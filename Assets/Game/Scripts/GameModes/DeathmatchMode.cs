using UnityEngine;

namespace NeuralStrike.GameModes
{
    /// <summary>
    /// Deathmatch (DM) - free-for-all mode.
    /// Every player is an enemy, individual scoring.
    /// </summary>
    public class DeathmatchMode : MonoBehaviour, IGameMode
    {
        private GameManager gameManager;
        
        public void Initialize(GameManager manager)
        {
            gameManager = manager;
            Debug.Log("Deathmatch mode initialized");
            
            // In DM, each player is their own team
        }
        
        public void Update()
        {
            // DM has no special logic
        }
        
        public void OnKill(GameObject killer, GameObject victim)
        {
            // Individual scoring - same formula but per-player
        }
    }
}