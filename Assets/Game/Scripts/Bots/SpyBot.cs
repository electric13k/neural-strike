using UnityEngine;

public class SpyBot : MonoBehaviour
{
    [Header("Spy Settings")]
    public float scoutRange = 30f;
    
    private BotController controller;
    
    private void Awake()
    {
        controller = GetComponent<BotController>();
    }
}