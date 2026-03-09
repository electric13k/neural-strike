using UnityEngine;
using System.Collections;
using NeuralStrike.Core;
using NeuralStrike.Bots;

namespace NeuralStrike.Weapons
{
    /// <summary>
    /// Data Knife melee weapon for hacking enemy bots.
    /// Hold on a bot to transfer ownership after hack duration.
    /// </summary>
    public class DataKnife : Weapon
    {
        [Header("Data Knife Settings")]
        [SerializeField] private float hackDuration = 3f; // Time to complete hack
        [SerializeField] private float hackRange = 2f;
        [SerializeField] private LayerMask botLayer;
        [SerializeField] private KeyCode hackKey = KeyCode.Mouse0;
        
        [Header("Visual Feedback")]
        [SerializeField] private GameObject hackEffectPrefab;
        [SerializeField] private AudioClip hackingSound;
        [SerializeField] private AudioClip hackCompleteSound;
        
        private BotController targetBot;
        private bool isHacking = false;
        private float hackProgress = 0f;
        private Coroutine hackCoroutine;
        
        public bool IsHacking => isHacking;
        public float HackProgress => hackProgress;
        public BotController TargetBot => targetBot;
        
        protected override void Update()
        {
            base.Update();
            
            // Hold to hack
            if (Input.GetKey(hackKey))
            {
                TryStartHack();
            }
            else if (Input.GetKeyUp(hackKey))
            {
                CancelHack();
            }
        }
        
        protected override void Fire()
        {
            // Data knife uses hack system, not fire
        }
        
        /// <summary>
        /// Attempt to start hacking a bot in range.
        /// </summary>
        private void TryStartHack()
        {
            if (isHacking) return;
            
            // Check for bot in front of player
            if (Physics.Raycast(muzzlePoint.position, muzzlePoint.forward, out RaycastHit hit, hackRange, botLayer))
            {
                BotController bot = hit.collider.GetComponent<BotController>();
                if (bot == null)
                    bot = hit.collider.GetComponentInParent<BotController>();
                
                if (bot != null && CanHackBot(bot))
                {
                    StartHack(bot);
                }
            }
        }
        
        /// <summary>
        /// Check if bot can be hacked (must be enemy bot).
        /// </summary>
        private bool CanHackBot(BotController bot)
        {
            Health playerHealth = GetComponentInParent<Health>();
            Health botHealth = bot.GetComponent<Health>();
            
            if (playerHealth == null || botHealth == null) return false;
            
            // Can only hack bots from enemy teams
            return playerHealth.Team != botHealth.Team;
        }
        
        /// <summary>
        /// Start hacking sequence.
        /// </summary>
        private void StartHack(BotController bot)
        {
            targetBot = bot;
            isHacking = true;
            hackProgress = 0f;
            
            PlaySound(hackingSound);
            hackCoroutine = StartCoroutine(HackCoroutine());
        }
        
        /// <summary>
        /// Cancel ongoing hack.
        /// </summary>
        private void CancelHack()
        {
            if (hackCoroutine != null)
            {
                StopCoroutine(hackCoroutine);
            }
            
            isHacking = false;
            hackProgress = 0f;
            targetBot = null;
            audioSource.Stop();
        }
        
        /// <summary>
        /// Hack progress coroutine.
        /// </summary>
        private IEnumerator HackCoroutine()
        {
            float elapsed = 0f;
            
            while (elapsed < hackDuration)
            {
                elapsed += Time.deltaTime;
                hackProgress = elapsed / hackDuration;
                
                // Check if still in range
                if (targetBot == null || Vector3.Distance(transform.position, targetBot.transform.position) > hackRange)
                {
                    CancelHack();
                    yield break;
                }
                
                yield return null;
            }
            
            // Hack complete!
            CompleteHack();
        }
        
        /// <summary>
        /// Complete hack and transfer bot ownership.
        /// </summary>
        private void CompleteHack()
        {
            if (targetBot != null)
            {
                PlaySound(hackCompleteSound);
                
                // Transfer bot to player's team
                Health playerHealth = GetComponentInParent<Health>();
                if (playerHealth != null)
                {
                    targetBot.TransferOwnership(playerHealth.Team, gameObject);
                }
                
                Debug.Log($"Successfully hacked {targetBot.name}!");
            }
            
            isHacking = false;
            hackProgress = 0f;
            targetBot = null;
        }
    }
}