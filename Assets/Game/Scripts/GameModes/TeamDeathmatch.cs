using UnityEngine;
using UnityEngine.UI;

// ============================================================
//  TEAM DEATHMATCH  — Neural Strike
//  2-team (Alpha / Bravo) or 4-team (Alpha / Bravo / Tango / Charlie)
//  Match length: 10-25 minutes.
//  Score formula: inherited from GameModeBase.
//
//  HOW TO WIRE IN UNITY
//  1. Create empty "GameMode" in the TDM scene.
//  2. Attach TeamDeathmatch script.
//  3. Set matchDurationSec (600–1500).
//  4. Assign scoreTexts in Inspector (one per team).
// ============================================================

public class TeamDeathmatch : GameModeBase
{
    [Header("Teams")]
    public bool fourTeamMode = false;

    [Header("Score UI (one Text per team)")]
    public Text alphaScoreText;
    public Text bravoScoreText;
    public Text tangoScoreText;   // only used in 4-team mode
    public Text charlieScoreText;

    // ── Per-team kill/death trackers ──────────────────────────
    private int _alphaKills,   _alphaBotKills,   _alphaDeaths,   _alphaBotDeaths;
    private int _bravoKills,   _bravoBotKills,   _bravoDeaths,   _bravoBotDeaths;
    private int _tangoKills,   _tangoBotKills,   _tangoDeaths,   _tangoBotDeaths;
    private int _charlieKills, _charlieBotKills, _charlieDeaths, _charlieBotDeaths;

    protected override void Start()
    {
        modeName         = fourTeamMode ? "TDM-4" : "TDM-2";
        matchDurationSec = 600f;   // default 10 min; Inspector can override
        base.Start();
    }

    // ── Public API called by kill system ──────────────────────

    public void RegisterKill(string killerTeam, bool isRobot)
    {
        switch (killerTeam)
        {
            case "Alpha":   if (isRobot) _alphaBotKills++;   else _alphaKills++;   break;
            case "Bravo":   if (isRobot) _bravoBotKills++;   else _bravoKills++;   break;
            case "Tango":   if (isRobot) _tangoBotKills++;   else _tangoKills++;   break;
            case "Charlie": if (isRobot) _charlieBotKills++; else _charlieKills++; break;
        }
        RefreshScoreUI();
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
        RefreshScoreUI();
    }

    // ── Score display ─────────────────────────────────────────

    void RefreshScoreUI()
    {
        if (alphaScoreText)
            alphaScoreText.text   = $"Alpha: {CalcScore(_alphaKills,   _alphaBotKills,   _alphaDeaths,   _alphaBotDeaths)}";
        if (bravoScoreText)
            bravoScoreText.text   = $"Bravo: {CalcScore(_bravoKills,   _bravoBotKills,   _bravoDeaths,   _bravoBotDeaths)}";
        if (fourTeamMode)
        {
            if (tangoScoreText)
                tangoScoreText.text   = $"Tango: {CalcScore(_tangoKills,   _tangoBotKills,   _tangoDeaths,   _tangoBotDeaths)}";
            if (charlieScoreText)
                charlieScoreText.text = $"Charlie: {CalcScore(_charlieKills, _charlieBotKills, _charlieDeaths, _charlieBotDeaths)}";
        }
    }

    protected override void EndMatch()
    {
        base.EndMatch();
        string winner = DetermineWinner();
        Debug.Log($"[TDM] WINNER: {winner}");
        // TODO: show end-screen panel with winner
    }

    string DetermineWinner()
    {
        int aScore = CalcScore(_alphaKills, _alphaBotKills, _alphaDeaths, _alphaBotDeaths);
        int bScore = CalcScore(_bravoKills, _bravoBotKills, _bravoDeaths, _bravoBotDeaths);
        if (!fourTeamMode)
            return aScore == bScore ? "Draw" : (aScore > bScore ? "Alpha" : "Bravo");

        int tScore = CalcScore(_tangoKills, _tangoBotKills, _tangoDeaths, _tangoBotDeaths);
        int cScore = CalcScore(_charlieKills, _charlieBotKills, _charlieDeaths, _charlieBotDeaths);
        int best   = Mathf.Max(aScore, bScore, tScore, cScore);
        if (aScore == best) return "Alpha";
        if (bScore == best) return "Bravo";
        if (tScore == best) return "Tango";
        return "Charlie";
    }
}
