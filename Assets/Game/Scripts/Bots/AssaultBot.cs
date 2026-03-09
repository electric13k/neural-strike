using UnityEngine;

public class AssaultBot : MonoBehaviour
{
    private BotController controller;
    
    public void Initialize(BotController botController)
    {
        controller = botController;
    }
    
    private void Awake()
    {
        controller = GetComponent<BotController>();
    }
}