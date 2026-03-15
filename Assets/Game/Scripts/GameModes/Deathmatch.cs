using UnityEngine;
using System.Collections.Generic;

// ============================================================
//  FREE-FOR-ALL DEATHMATCH  — Neural Strike
//  Match length: 7–12 minutes.
//  Score per individual player tracked in a Dictionary.
// ============================================================

public class Deathmatch : GameModeBase
{
    [Header("Match Length (7–12 min)")]
    public float minDuration = 420f;   // 7 min
    public float maxDuration = 720f;   // 12 min

    // playerID → score
    private Dictionary<string, int> _scores = new Dictionary<string, int>();

    protected override void Start()
    {
        modeName         = "Deathmatch";
        matchDurationSec = Random.Range(minDuration, maxDuration);
        base.Start();
    }

    public void RegisterKill(string playerID, bool isRobotKill)
    {
        if (!_scores.ContainsKey(playerID)) _scores[playerID] = 0;
        _scores[playerID] += isRobotKill ? 0 : 1;   // robots worth 0.5; use float if needed
    }

    public void RegisterDeath(string playerID)
    {
        if (!_scores.ContainsKey(playerID)) _scores[playerID] = 0;
        _scores[playerID] -= 100;
    }

    protected override void EndMatch()
    {
        base.EndMatch();
        string winner   = "";
        int    topScore = int.MinValue;
        foreach (var kv in _scores)
            if (kv.Value > topScore) { topScore = kv.Value; winner = kv.Key; }

        Debug.Log($"[DM] Winner: {winner} with score {topScore}");
    }
}
