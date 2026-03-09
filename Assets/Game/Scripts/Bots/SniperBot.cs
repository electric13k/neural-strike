using UnityEngine;

public class SniperBot : MonoBehaviour
{
    // Simplified sniper bot - to be implemented
    private BotController controller;
    
    private void Awake()
    {
        controller = GetComponent<BotController>();
    }
}