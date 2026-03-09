using UnityEngine;
using NeuralStrike.Core;

namespace NeuralStrike.GameModes
{
    /// <summary>
    /// Flag capture zone for Base Capture mode.
    /// Can only be captured by human players (not bots).
    /// </summary>
    public class FlagZone : MonoBehaviour
    {
        [Header("Flag Settings")]
        [SerializeField] private Team owningTeam = Team.Alpha;
        [SerializeField] private float captureTime = 3f;
        [SerializeField] private GameObject flagModel;
        [SerializeField] private Material alphaFlagMaterial;
        [SerializeField] private Material bravoFlagMaterial;
        
        [Header("Capture Indicator")]
        [SerializeField] private GameObject captureUI;
        
        private GameObject capturingPlayer;
        private float captureProgress = 0f;
        private bool isBeingCaptured = false;
        
        public Team OwningTeam => owningTeam;
        public bool IsBeingCaptured => isBeingCaptured;
        public float CaptureProgress => captureProgress;
        
        void Update()
        {
            if (isBeingCaptured && capturingPlayer != null)
            {
                // Check if player still in zone
                float distance = Vector3.Distance(transform.position, capturingPlayer.transform.position);
                if (distance > 5f) // Zone radius
                {
                    CancelCapture();
                    return;
                }
                
                // Increment capture progress
                captureProgress += Time.deltaTime / captureTime;
                
                if (captureProgress >= 1f)
                {
                    CompleteCapture();
                }
            }
        }
        
        void OnTriggerEnter(Collider other)
        {
            // Check if human player (not bot)
            if (other.CompareTag("Player"))
            {
                Health playerHealth = other.GetComponent<Health>();
                if (playerHealth != null && playerHealth.Team != owningTeam)
                {
                    // Enemy player entered zone - start capture
                    StartCapture(other.gameObject);
                }
            }
        }
        
        void OnTriggerExit(Collider other)
        {
            if (other.gameObject == capturingPlayer)
            {
                CancelCapture();
            }
        }
        
        private void StartCapture(GameObject player)
        {
            capturingPlayer = player;
            isBeingCaptured = true;
            captureProgress = 0f;
            
            if (captureUI != null)
                captureUI.SetActive(true);
            
            Debug.Log($"Player {player.name} started capturing flag");
        }
        
        private void CancelCapture()
        {
            capturingPlayer = null;
            isBeingCaptured = false;
            captureProgress = 0f;
            
            if (captureUI != null)
                captureUI.SetActive(false);
            
            Debug.Log("Flag capture cancelled");
        }
        
        private void CompleteCapture()
        {
            if (capturingPlayer != null)
            {
                Health playerHealth = capturingPlayer.GetComponent<Health>();
                if (playerHealth != null)
                {
                    // Transfer flag ownership
                    owningTeam = playerHealth.Team;
                    UpdateFlagVisual();
                    
                    Debug.Log($"Flag captured by {owningTeam}!");
                }
            }
            
            CancelCapture();
        }
        
        private void UpdateFlagVisual()
        {
            if (flagModel != null)
            {
                Renderer renderer = flagModel.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = owningTeam == Team.Alpha ? alphaFlagMaterial : bravoFlagMaterial;
                }
            }
        }
    }
}