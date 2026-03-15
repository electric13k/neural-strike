using UnityEngine;
using UnityEngine.Events;

// ============================================================
//  GAME MODE BASE  — Neural Strike
//  All game modes inherit from this.
//  Manages: match timer, state machine, score formula.
//
//  Score = (playerKills + robotKills/2) - (playerDeaths + robotDeaths) * 100
// ============================================================

public abstract class GameModeBase : MonoBehaviour
{
    [Header("Match Settings")]
    public string modeName          = "Base";
    public float  matchDurationSec  = 600f;   // 10 min default

    [Header("Events")]
    public UnityEvent onMatchStart;
    public UnityEvent onMatchEnd;

    // ── State ──────────────────────────────────────────────────
    public enum MatchState { Warmup, Active, Ended }
    public MatchState State { get; private set; } = MatchState.Warmup;

    protected float _startTime;
    private HUDManager _hud;

    // ── Lifecycle ─────────────────────────────────────────────

    protected virtual void Start()
    {
        _hud = FindObjectOfType<HUDManager>();
        StartMatch();
    }

    protected virtual void Update()
    {
        if (State != MatchState.Active) return;

        float elapsed   = Time.time - _startTime;
        float remaining = Mathf.Max(0f, matchDurationSec - elapsed);

        // Update HUD timer
        int mins = (int)(remaining / 60f);
        int secs = (int)(remaining % 60f);
        _hud?.SetTimerText($"{mins:00}:{secs:00}");

        if (remaining <= 0f)
            EndMatch();
    }

    // ── API ────────────────────────────────────────────────────

    public virtual void StartMatch()
    {
        State      = MatchState.Active;
        _startTime = Time.time;
        onMatchStart?.Invoke();
        Debug.Log($"[{modeName}] Match started.");
    }

    public virtual void EndMatch()
    {
        if (State == MatchState.Ended) return;
        State = MatchState.Ended;
        onMatchEnd?.Invoke();
        Debug.Log($"[{modeName}] Match ended.");
    }

    /// <summary>Calculates Neural Strike score for one player/team.</summary>
    public static int CalcScore(int playerKills, int robotKills,
                                int playerDeaths, int robotDeaths)
    {
        float raw = (playerKills + robotKills * 0.5f)
                  - (playerDeaths + robotDeaths) * 100f;
        return Mathf.RoundToInt(raw);
    }
}
