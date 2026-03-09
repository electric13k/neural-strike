using UnityEngine;
using NeuralStrike.Core;
using NeuralStrike.UI;

namespace NeuralStrike.Weapons
{
    /// <summary>
    /// Tracker bullet weapon that marks targets for remote viewing.
    /// Tagged targets can be viewed through Battle Pad camera feeds.
    /// </summary>
    public class TrackerBulletWeapon : HitscanWeapon
    {
        [Header("Tracker Settings")]
        [SerializeField] private float trackerDuration = 30f; // How long tracker lasts
        [SerializeField] private GameObject trackerMarkerPrefab; // Visual marker on tagged target
        
        protected override void HandleHit(RaycastHit hit)
        {
            base.HandleHit(hit);
            
            // Check if hit target can be tracked
            TrackerTarget target = hit.collider.GetComponent<TrackerTarget>();
            if (target == null)
            {
                target = hit.collider.GetComponentInParent<TrackerTarget>();
            }
            
            if (target != null)
            {
                target.ApplyTracker(gameObject, trackerDuration);
                
                // Notify Battle Pad that new target is available
                BattlePadManager battlePad = FindObjectOfType<BattlePadManager>();
                if (battlePad != null)
                {
                    battlePad.RegisterTrackedTarget(target);
                }
            }
        }
    }
    
    /// <summary>
    /// Component attached to entities that can be tracked (players, bots).
    /// Stores tracking state and provides camera feed access.
    /// </summary>
    public class TrackerTarget : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera; // First-person camera of this entity
        [SerializeField] private Transform cameraMount; // Where to place spectator camera
        
        private bool isTracked = false;
        private GameObject tracker; // Who applied the tracker
        private float trackerEndTime;
        private GameObject markerInstance;
        
        public bool IsTracked => isTracked && Time.time < trackerEndTime;
        public Camera TargetCamera => targetCamera;
        public Transform CameraMount => cameraMount;
        public GameObject Tracker => tracker;
        
        void Update()
        {
            // Remove tracker when duration expires
            if (isTracked && Time.time >= trackerEndTime)
            {
                RemoveTracker();
            }
        }
        
        /// <summary>
        /// Apply tracker to this target.
        /// </summary>
        public void ApplyTracker(GameObject appliedBy, float duration)
        {
            tracker = appliedBy;
            isTracked = true;
            trackerEndTime = Time.time + duration;
            
            // TODO: Spawn visual marker (glowing outline, icon, etc.)
            Debug.Log($"Tracker applied to {gameObject.name} by {appliedBy.name}");
        }
        
        /// <summary>
        /// Remove tracker.
        /// </summary>
        public void RemoveTracker()
        {
            isTracked = false;
            tracker = null;
            
            if (markerInstance != null)
            {
                Destroy(markerInstance);
            }
            
            Debug.Log($"Tracker removed from {gameObject.name}");
        }
        
        /// <summary>
        /// Get a render texture for Battle Pad camera feed.
        /// </summary>
        public RenderTexture GetCameraFeed()
        {
            if (targetCamera != null)
            {
                return targetCamera.targetTexture;
            }
            return null;
        }
    }
}