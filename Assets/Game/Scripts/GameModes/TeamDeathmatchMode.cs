using UnityEngine;

namespace NeuralStrike.GameModes
{
    /// <summary>
    /// Team Deathmatch (TDM) mode.
    /// 2-4 teams, pure combat, highest score wins.
    /// </summary>
    public class TeamDeathmatchMode : MonoBehaviour, IGameMode
    {
        private GameManager gameManager;
        
        public void Initialize(GameManager manager)
        {
            gameManager = manager;
            Debug.Log("Team Deathmatch mode initialized");
        }
        
        public void Update()
        {
            // TDM has no special per-frame logic beyond base GameManager
        }
        
        public void OnKill(GameObject killer, GameObject victim)
        {
            // Scoring handled by GameManager universal formula
        }
    }
}