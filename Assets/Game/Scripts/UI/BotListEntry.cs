using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BotListEntry : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI roleText;
    public TextMeshProUGUI stateText;
    public Slider healthBar;
    public Button selectButton;
    public Image statusIndicator;
    
    private BotController bot;
    private Health botHealth;
    
    public void Initialize(BotController botController)
    {
        bot = botController;
        botHealth = bot.GetComponent<Health>();
        
        if (nameText != null)
            nameText.text = bot.gameObject.name;
        
        if (roleText != null)
            roleText.text = bot.role.ToString();
        
        if (selectButton != null)
            selectButton.onClick.AddListener(OnSelectBot);
    }
    
    public void UpdateDisplay()
    {
        if (bot == null) return;
        
        // Update health bar
        if (healthBar != null && botHealth != null)
        {
            healthBar.value = botHealth.maxHealth > 0 ? (botHealth.maxHealth / botHealth.maxHealth) : 0f;
        }
        
        // Update state text
        if (stateText != null)
        {
            // State is private, show generic status
            stateText.text = bot.enabled ? "Active" : "Disabled";
        }
        
        // Update status indicator color
        if (statusIndicator != null)
        {
            if (botHealth != null)
            {
                float healthPercent = botHealth.maxHealth > 0 ? (botHealth.maxHealth / botHealth.maxHealth) : 0f;
                if (healthPercent > 0.6f)
                    statusIndicator.color = Color.green;
                else if (healthPercent > 0.3f)
                    statusIndicator.color = Color.yellow;
                else
                    statusIndicator.color = Color.red;
            }
        }
    }
    
    private void OnSelectBot()
    {
        // Future: Camera focus, command assignment
        Debug.Log($"Selected bot: {bot.gameObject.name}");
    }
}