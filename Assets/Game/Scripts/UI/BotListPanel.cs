using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BotListPanel : MonoBehaviour
{
    [Header("References")]
    public Transform botListContent;
    public GameObject botEntryPrefab;
    
    private Dictionary<BotController, BotListEntry> botEntries = new Dictionary<BotController, BotListEntry>();
    
    public void AddBot(BotController bot)
    {
        if (bot == null || botEntries.ContainsKey(bot)) return;
        
        if (botEntryPrefab != null && botListContent != null)
        {
            GameObject entryObj = Instantiate(botEntryPrefab, botListContent);
            BotListEntry entry = entryObj.GetComponent<BotListEntry>();
            
            if (entry != null)
            {
                entry.Initialize(bot);
                botEntries[bot] = entry;
            }
        }
    }
    
    public void RemoveBot(BotController bot)
    {
        if (botEntries.ContainsKey(bot))
        {
            BotListEntry entry = botEntries[bot];
            if (entry != null)
                Destroy(entry.gameObject);
            
            botEntries.Remove(bot);
        }
    }
    
    private void Update()
    {
        foreach (var kvp in botEntries)
        {
            if (kvp.Value != null)
                kvp.Value.UpdateDisplay();
        }
    }
}