using UnityEngine;
using System.Collections.Generic;

// ============================================================
//  FREE-FOR-ALL DEATHMATCH  — Neural Strike
//  Match length: 7–12 minutes.
//  Score per individual player tracked in a Dictionary.
// ============================================================

public class Deathmatch : GameModeBase
{
    [Header("Match Length (seconds)")]
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

    // ── Public API ────────────────────────────────────────────

    public void RegisterKill(string playerID, bool isRobotKill)
    {
        if (!_scores.ContainsKey(playerID)) _scores[playerID] = 0;
        // robots count as half; we use integer rounding here
        _scores[playerID] += isRobotKill ? 0 : 1;
    }

    public void RegisterDeath(string playerID)
    {
        if (!_scores.ContainsKey(playerID)) _scores[playerID] = 0;
        _scores[playerID] -= 100;
    }

    // fix CS0507: must be public override (same access as base)
    public override void EndMatch()
    {
        base.EndMatch();

        string winner   = "nobody";
        int    topScore = int.MinValue;
        foreach (KeyValuePair<string, int> kv in _scores)
        {
            if (kv.Value > topScore)
            {
                topScore = kv.Value;
                winner   = kv.Key;
            }
        }
        Debug.Log($"[DM] Winner: {winner}  Score: {topScore}");
    }
}
