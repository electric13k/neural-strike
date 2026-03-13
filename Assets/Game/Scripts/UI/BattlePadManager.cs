using UnityEngine;
using System.Collections.Generic;

public class BattlePadManager : MonoBehaviour
{
    [Header("References")]
    public GameObject   battlePadUI;
    public BotListPanel botListPanel;
    public CameraFeedPanel cameraFeedPanel;
    public CommandPanel    commandPanel;

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.Tab;

    private bool isOpen;
    private List<BotController> registeredBots = new List<BotController>();

    private void Start()
    {
        if (battlePadUI != null) battlePadUI.SetActive(false);
        RegisterAllBots();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey)) ToggleBattlePad();
    }

    private void RegisterAllBots()
    {
#if UNITY_2023_1_OR_NEWER
        foreach (var bot in Object.FindObjectsByType<BotController>(FindObjectsSortMode.None))
            RegisterBot(bot);
#else
        foreach (var bot in FindObjectsOfType<BotController>())
            RegisterBot(bot);
#endif
    }

    public void RegisterBot(BotController bot)
    {
        if (!registeredBots.Contains(bot))
        {
            registeredBots.Add(bot);
            if (botListPanel != null) botListPanel.AddBot(bot);
        }
    }

    public void UnregisterBot(BotController bot)
    {
        if (registeredBots.Contains(bot))
        {
            registeredBots.Remove(bot);
            if (botListPanel != null) botListPanel.RemoveBot(bot);
        }
    }

    private void ToggleBattlePad()
    {
        isOpen = !isOpen;
        if (battlePadUI != null) battlePadUI.SetActive(isOpen);
        Cursor.lockState  = isOpen ? CursorLockMode.None   : CursorLockMode.Locked;
        Cursor.visible    = isOpen;
        Time.timeScale    = isOpen ? 0f : 1f;
    }

    public List<BotController> GetRegisteredBots() => new List<BotController>(registeredBots);
    public bool IsOpen() => isOpen;
}
