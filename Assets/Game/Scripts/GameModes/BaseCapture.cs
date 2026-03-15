using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// ============================================================
//  BASE CAPTURE  — Neural Strike
//  Two teams: Alpha & Bravo, each with 4 flags.
//  Prep phase (10 min) → Main phase (15–40 min).
//  Win by capturing all 4 enemy flags while keeping ≥3 own.
// ============================================================

public class BaseCapture : GameModeBase
{
    [Header("Phase Durations (seconds)")]
    public float prepDuration  = 600f;    // 10 min
    public float minMainPhase  = 900f;    // 15 min
    public float maxMainPhase  = 2400f;   // 40 min

    [Header("Flag References — 4 Alpha, 4 Bravo")]
    public FlagZone[] alphaFlags = new FlagZone[4];
    public FlagZone[] bravoFlags = new FlagZone[4];

    [Header("UI")]
    public Text phaseText;
    public Text alphaFlagText;
    public Text bravoFlagText;

    // ── State ──────────────────────────────────────────────────
    private enum Phase { Prep, Main }
    private Phase _phase = Phase.Prep;
    private float _prepEndTime;

    protected override void Start()
    {
        modeName         = "Base Capture";
        matchDurationSec = Random.Range(minMainPhase, maxMainPhase);
        _prepEndTime     = Time.time + prepDuration;
        if (phaseText) phaseText.text = "PREP PHASE";
        // Don't call base.Start() yet — wait until main phase
        StartCoroutine(PrepCountdown());
    }

    IEnumerator PrepCountdown()
    {
        yield return new WaitForSeconds(prepDuration);
        _phase = Phase.Main;
        if (phaseText) phaseText.text = "BATTLE PHASE";
        base.Start();
    }

    protected override void Update()
    {
        if (_phase == Phase.Prep) return;
        base.Update();      // handles timer & EndMatch
        RefreshFlagUI();
        CheckVictory();
    }

    // ── Flag tracking ─────────────────────────────────────────

    void RefreshFlagUI()
    {
        int alphaOwned = 0, bravoOwned = 0;
        foreach (var f in alphaFlags) if (f != null && f.OwnedBy == "Alpha") alphaOwned++;
        foreach (var f in bravoFlags) if (f != null && f.OwnedBy == "Bravo") bravoOwned++;

        if (alphaFlagText) alphaFlagText.text = $"Alpha flags: {alphaOwned}/4";
        if (bravoFlagText) bravoFlagText.text  = $"Bravo flags: {bravoOwned}/4";
    }

    void CheckVictory()
    {
        int alphaCaptured = 0, bravoCaptured = 0;
        int alphaLost    = 0, bravoLost    = 0;

        foreach (var f in alphaFlags)
        {
            if (f == null) continue;
            if (f.OwnedBy == "Bravo") alphaCaptured++;
            if (f.OwnedBy == "Alpha") continue;
        }
        foreach (var f in bravoFlags)
        {
            if (f == null) continue;
            if (f.OwnedBy == "Alpha") bravoCaptured++;
        }

        // Alpha wins: captured all 4 bravo flags & lost <= 1
        int alphaRemaining = 4 - alphaCaptured;
        int bravoRemaining = 4 - bravoCaptured;

        if (bravoCaptured == 4 && alphaRemaining >= 3)
        {
            Debug.Log("[BaseCapture] ALPHA WINS!");
            EndMatch();
        }
        else if (alphaCaptured == 4 && bravoRemaining >= 3)
        {
            Debug.Log("[BaseCapture] BRAVO WINS!");
            EndMatch();
        }
    }
}
