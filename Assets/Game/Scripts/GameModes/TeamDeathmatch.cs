using UnityEngine;
using UnityEngine.UI;

// ============================================================
//  TEAM DEATHMATCH  — Neural Strike
//  2-team (Alpha / Bravo) or 4-team (Alpha / Bravo / Tango / Charlie)
//  Match length: 10-25 minutes.
//  Score formula: inherited from GameModeBase.
// ============================================================

public class TeamDeathmatch : GameModeBase
{
    [Header("Teams")]
    public bool fourTeamMode = false;

    [Header("Score UI  (one Text per team)")]
    public Text alphaScoreText;
    public Text bravoScoreText;
    public Text tangoScoreText;
    public Text charlieScoreText;

    // ── Per-team stats ────────────────────────────────────────
    private int _alphaKills,   _alphaBotKills,   _alphaDeaths,   _alphaBotDeaths;
    private int _bravoKills,   _bravoBotKills,   _bravoDeaths,   _bravoBotDeaths;
    private int _tangoKills,   _tangoBotKills,   _tangoDeaths,   _tangoBotDeaths;
    private int _charlieKills, _charlieBotKills, _charlieDeaths, _charlieBotDeaths;

    protected override void Start()
    {
        modeName         = fourTeamMode ? "TDM-4" : "TDM-2";
        matchDurationSec = matchDurationSec > 0f ? matchDurationSec : 600f;
        base.Start();
    }

    // ── Public API (called by kill/death events) ──────────────

    public void RegisterKill(string killerTeam, bool isRobot)
    {
        switch (killerTeam)
        {
            case "Alpha":   if (isRobot) _alphaBotKills++;   else _alphaKills++;   break;
            case "Bravo":   if (isRobot) _bravoBotKills++;   else _bravoKills++;   break;
            case "Tango":   if (isRobot) _tangoBotKills++;   else _tangoKills++;   break;
            case "Charlie": if (isRobot) _charlieBotKills++; else _charlieKills++; break;
        }
        RefreshUI();
    }

    public void RegisterDeath(string victimTeam, bool isRobot)
    {
        switch (victimTeam)
        {
            case "Alpha":   if (isRobot) _alphaBotDeaths++;   else _alphaDeaths++;   break;
            case "Bravo":   if (isRobot) _bravoBotDeaths++;   else _bravoDeaths++;   break;
            case "Tango":   if (isRobot) _tangoBotDeaths++;   else _tangoDeaths++;   break;
            case "Charlie": if (isRobot) _charlieBotDeaths++; else _charlieDeaths++; break;
        }
        RefreshUI();
    }

    // ── Score UI ──────────────────────────────────────────────

    void RefreshUI()
    {
        if (alphaScoreText)
            alphaScoreText.text   = $"Alpha: {CalcScore(_alphaKills,   _alphaBotKills,   _alphaDeaths,   _alphaBotDeaths)}";
        if (bravoScoreText)
            bravoScoreText.text   = $"Bravo: {CalcScore(_bravoKills,   _bravoBotKills,   _bravoDeaths,   _bravoBotDeaths)}";

        if (!fourTeamMode) return;

        if (tangoScoreText)
            tangoScoreText.text   = $"Tango: {CalcScore(_tangoKills,   _tangoBotKills,   _tangoDeaths,   _tangoBotDeaths)}";
        if (charlieScoreText)
            charlieScoreText.text = $"Charlie: {CalcScore(_charlieKills, _charlieBotKills, _charlieDeaths, _charlieBotDeaths)}";
    }

    // fix CS0507: must be public override (same access as base)
    public override void EndMatch()
    {
        base.EndMatch();
        Debug.Log($"[TDM] WINNER: {DetermineWinner()}");
    }

    string DetermineWinner()
    {
        int a = CalcScore(_alphaKills, _alphaBotKills, _alphaDeaths, _alphaBotDeaths);
        int b = CalcScore(_bravoKills, _bravoBotKills, _bravoDeaths, _bravoBotDeaths);

        if (!fourTeamMode)
            return a == b ? "Draw" : (a > b ? "Alpha" : "Bravo");

        int t = CalcScore(_tangoKills,   _tangoBotKills,   _tangoDeaths,   _tangoBotDeaths);
        int c = CalcScore(_charlieKills, _charlieBotKills, _charlieDeaths, _charlieBotDeaths);
        int best = Mathf.Max(a, b, t, c);
        if (a == best) return "Alpha";
        if (b == best) return "Bravo";
        if (t == best) return "Tango";
        return "Charlie";
    }
}
