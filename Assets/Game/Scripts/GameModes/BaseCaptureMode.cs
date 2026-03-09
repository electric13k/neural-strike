using UnityEngine;
using System.Collections.Generic;
using NeuralStrike.Core;

namespace NeuralStrike.GameModes
{
    /// <summary>
    /// Base Capture mode - attack/defend 4 flags per team.
    /// Two phases: Preparation (10min) and Assault (15-40min).
    /// Only human players can capture flags.
    /// </summary>
    public class BaseCaptureMode : MonoBehaviour, IGameMode
    {
        [Header("Phase Timings")]
        [SerializeField] private float preparationDuration = 600f; // 10 minutes
        [SerializeField] private float assaultDuration = 2400f; // 40 minutes
        
        private GameManager gameManager;
        private BaseCapturePhase currentPhase = BaseCapturePhase.Preparation;
        private float phaseStartTime;
        
        private List<FlagZone> alphaFlags = new List<FlagZone>();
        private List<FlagZone> bravoFlags = new List<FlagZone>();
        
        public BaseCapturePhase CurrentPhase => currentPhase;
        
        public void Initialize(GameManager manager)
        {
            gameManager = manager;
            
            // Find all flag zones
            FlagZone[] allFlags = FindObjectsOfType<FlagZone>();
            foreach (FlagZone flag in allFlags)
            {
                if (flag.OwningTeam == Team.Alpha)
                    alphaFlags.Add(flag);
                else if (flag.OwningTeam == Team.Bravo)
                    bravoFlags.Add(flag);
            }
            
            StartPreparation();
            Debug.Log("Base Capture mode initialized");
        }
        
        public void Update()
        {
            float phaseElapsed = Time.time - phaseStartTime;
            
            if (currentPhase == BaseCapturePhase.Preparation)
            {
                if (phaseElapsed >= preparationDuration)
                {
                    StartAssault();
                }
            }
            else if (currentPhase == BaseCapturePhase.Assault)
            {
                CheckWinConditions();
            }
        }
        
        public void OnKill(GameObject killer, GameObject victim)
        {
            // Same scoring as other modes
        }
        
        private void StartPreparation()
        {
            currentPhase = BaseCapturePhase.Preparation;
            phaseStartTime = Time.time;
            Debug.Log("Preparation phase started - fortify your base!");
            
            // TODO: Disable damage between teams during prep
        }
        
        private void StartAssault()
        {
            currentPhase = BaseCapturePhase.Assault;
            phaseStartTime = Time.time;
            Debug.Log("Assault phase started - attack enemy flags!");
            
            // TODO: Enable combat
        }
        
        private void CheckWinConditions()
        {
            // Total Capture: capture all 4 enemy flags
            int alphaCaptured = CountCapturedFlags(Team.Alpha, bravoFlags);
            int bravoCaptured = CountCapturedFlags(Team.Bravo, alphaFlags);
            
            if (alphaCaptured >= 4)
            {
                Debug.Log("Alpha wins by Total Capture!");
                // TODO: End match with Alpha victory
            }
            else if (bravoCaptured >= 4)
            {
                Debug.Log("Bravo wins by Total Capture!");
                // TODO: End match with Bravo victory
            }
            
            // Majority Control at time limit
            float remainingTime = assaultDuration - (Time.time - phaseStartTime);
            if (remainingTime <= 0)
            {
                CheckMajorityControl();
            }
        }
        
        private int CountCapturedFlags(Team attackingTeam, List<FlagZone> targetFlags)
        {
            int count = 0;
            foreach (FlagZone flag in targetFlags)
            {
                if (flag.OwningTeam == attackingTeam)
                    count++;
            }
            return count;
        }
        
        private void CheckMajorityControl()
        {
            // TODO: Check which team controls more total flags
            Debug.Log("Time expired - checking majority control");
        }
    }
    
    public enum BaseCapturePhase
    {
        Preparation,
        Assault
    }
}