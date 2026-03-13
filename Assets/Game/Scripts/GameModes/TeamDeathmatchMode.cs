using UnityEngine;

/// <summary>
/// Team Deathmatch game mode — plugs into MatchManager and wraps GameManager.
/// </summary>
public class TeamDeathmatchMode : GameMode
{
    [Header("References")]
    public GameManager gameManager;

    public override void Initialize()
    {
        if (gameManager == null)
#if UNITY_2023_1_OR_NEWER
            gameManager = Object.FindFirstObjectByType<GameManager>();
#else
            gameManager = FindObjectOfType<GameManager>();
#endif
    }

    public override void OnMatchStart()
    {
        if (gameManager == null) Initialize();
        if (gameManager != null) gameManager.StartMatch();
    }

    public override void OnMatchEnd()
    {
        if (gameManager != null) gameManager.EndMatch();
    }
}
