using UnityEngine;
using UnityEngine.UI;

// ============================================================
//  FLAG ZONE  — Neural Strike
//  One per flag post.  Player stands in it for captureTime
//  seconds to claim it for their team.
//
//  HOW TO WIRE IN UNITY
//  1. Create a cylinder / post as visual prop.
//  2. Add Box/Sphere Collider → Is Trigger = true.
//  3. Attach FlagZone script.
//  4. Set ownerTeam ("Alpha" or "Bravo") — the team that starts owning it.
//  5. Assign flagRenderer so the material colour changes on capture.
// ============================================================

public class FlagZone : MonoBehaviour
{
    [Header("Settings")]
    public string ownerTeam   = "Alpha";   // starting owner
    public float  captureTime = 5f;

    [Header("Visual")]
    public Renderer flagRenderer;           // changes colour on capture
    public Color    alphaColor = Color.blue;
    public Color    bravoColor = Color.red;
    public Color    neutralColor = Color.grey;

    [Header("UI")]
    public Slider captureProgressSlider;    // optional world-space UI slider

    // ── State ──────────────────────────────────────────────────
    public string OwnedBy { get; private set; }
    private float _progress;
    private string _capturingTeam = "";
    private bool   _contested;

    void Start()
    {
        OwnedBy = ownerTeam;
        SetFlagColour(OwnedBy);
    }

    void OnTriggerStay(Collider other)
    {
        TeamMember tm = other.GetComponentInParent<TeamMember>();
        if (tm == null) return;

        if (tm.teamName == OwnedBy) return;   // already owns it

        if (_capturingTeam == "" || _capturingTeam == tm.teamName)
        {
            _capturingTeam = tm.teamName;
            _progress += Time.deltaTime;

            if (captureProgressSlider)
                captureProgressSlider.value = _progress / captureTime;

            if (_progress >= captureTime)
                Capture(_capturingTeam);
        }
    }

    void OnTriggerExit(Collider other)
    {
        TeamMember tm = other.GetComponentInParent<TeamMember>();
        if (tm == null || tm.teamName != _capturingTeam) return;
        _progress      = 0f;
        _capturingTeam = "";
        if (captureProgressSlider) captureProgressSlider.value = 0f;
    }

    void Capture(string newTeam)
    {
        OwnedBy        = newTeam;
        _progress      = 0f;
        _capturingTeam = "";
        SetFlagColour(newTeam);
        Debug.Log($"[FlagZone] {name} captured by {newTeam}!");
    }

    void SetFlagColour(string team)
    {
        if (flagRenderer == null) return;
        flagRenderer.material.color = team == "Alpha" ? alphaColor
                                    : team == "Bravo" ? bravoColor
                                    : neutralColor;
    }
}
