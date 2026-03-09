using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CommandPanel : MonoBehaviour
{
    [Header("UI References")]
    public Button attackButton;
    public Button defendButton;
    public Button followButton;
    public Button holdButton;
    public TextMeshProUGUI selectedBotText;
    
    private BotController selectedBot;
    
    private void Start()
    {
        if (attackButton != null)
            attackButton.onClick.AddListener(() => IssueCommand("Attack"));
        
        if (defendButton != null)
            defendButton.onClick.AddListener(() => IssueCommand("Defend"));
        
        if (followButton != null)
            followButton.onClick.AddListener(() => IssueCommand("Follow"));
        
        if (holdButton != null)
            holdButton.onClick.AddListener(() => IssueCommand("Hold"));
    }
    
    public void SelectBot(BotController bot)
    {
        selectedBot = bot;
        
        if (selectedBotText != null)
        {
            if (bot != null)
                selectedBotText.text = $"Selected: {bot.gameObject.name}";
            else
                selectedBotText.text = "No bot selected";
        }
    }
    
    private void IssueCommand(string command)
    {
        if (selectedBot == null)
        {
            Debug.Log("No bot selected for command");
            return;
        }
        
        Debug.Log($"Issuing {command} command to {selectedBot.gameObject.name}");
        
        // Future: Implement actual command system
        // For now, just log
    }
}