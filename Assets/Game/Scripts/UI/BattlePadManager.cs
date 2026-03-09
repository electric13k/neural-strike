using UnityEngine;
using System.Collections.Generic;

public class BattlePadManager : MonoBehaviour
{
    [Header("References")]
    public GameObject battlePadUI;
    public BotListPanel botListPanel;
    public CameraFeedPanel cameraFeedPanel;
    public CommandPanel commandPanel;
    
    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.Tab;
    
    private bool isOpen;
    private List<BotController> registeredBots = new List<BotController>();
    
    private void Start()
    {
        if (battlePadUI != null)
            battlePadUI.SetActive(false);
        
        RegisterAllBots();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleBattlePad();
        }
    }
    
    private void RegisterAllBots()
    {
        BotController[] bots = FindObjectsOfType<BotController>();
        foreach (BotController bot in bots)
        {
            RegisterBot(bot);
        }
    }
    
    public void RegisterBot(BotController bot)
    {
        if (!registeredBots.Contains(bot))
        {
            registeredBots.Add(bot);
            if (botListPanel != null)
                botListPanel.AddBot(bot);
        }
    }
    
    public void UnregisterBot(BotController bot)
    {
        if (registeredBots.Contains(bot))
        {
            registeredBots.Remove(bot);
            if (botListPanel != null)
                botListPanel.RemoveBot(bot);
        }
    }
    
    private void ToggleBattlePad()
    {
        isOpen = !isOpen;
        
        if (battlePadUI != null)
            battlePadUI.SetActive(isOpen);
        
        if (isOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
        }
    }
    
    public List<BotController> GetRegisteredBots() { return new List<BotController>(registeredBots); }
    public bool IsOpen() { return isOpen; }
}