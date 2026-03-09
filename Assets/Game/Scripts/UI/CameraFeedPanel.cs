using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CameraFeedPanel : MonoBehaviour
{
    [Header("Feed Settings")]
    public int maxFeeds = 4;
    public Transform feedContainer;
    public GameObject feedPrefab;
    
    private List<CameraFeed> activeFeeds = new List<CameraFeed>();
    
    public void AddFeed(BotController bot)
    {
        if (bot == null || activeFeeds.Count >= maxFeeds) return;
        
        // Check if already has feed
        foreach (CameraFeed feed in activeFeeds)
        {
            if (feed.GetBot() == bot) return;
        }
        
        if (feedPrefab != null && feedContainer != null)
        {
            GameObject feedObj = Instantiate(feedPrefab, feedContainer);
            CameraFeed feed = feedObj.GetComponent<CameraFeed>();
            
            if (feed != null)
            {
                feed.Initialize(bot);
                activeFeeds.Add(feed);
            }
        }
    }
    
    public void RemoveFeed(BotController bot)
    {
        CameraFeed feedToRemove = null;
        foreach (CameraFeed feed in activeFeeds)
        {
            if (feed.GetBot() == bot)
            {
                feedToRemove = feed;
                break;
            }
        }
        
        if (feedToRemove != null)
        {
            activeFeeds.Remove(feedToRemove);
            Destroy(feedToRemove.gameObject);
        }
    }
    
    public void ClearAllFeeds()
    {
        foreach (CameraFeed feed in activeFeeds)
        {
            if (feed != null)
                Destroy(feed.gameObject);
        }
        activeFeeds.Clear();
    }
}