using UnityEngine;
using UnityEngine.SceneManagement;

// ============================================================
//  SCENE BOOTSTRAP  — Neural Strike
//
//  Handles:
//   - Flooded ground kill-zone
//   - Escape → Pause toggle
//   - Scene reload / return to main menu
//
//  HOW TO WIRE IN UNITY
//  Place on an empty "GameManager" object in every Game scene.
// ============================================================

public class SceneBootstrap : MonoBehaviour
{
    [Header("Flooded Ground Kill Plane")]
    [Tooltip("Any player below this Y is killed & respawned.")]
    public float     killPlaneY     = -8f;
    public Transform respawnPoint;      // if null, uses spawn origin (0,2,0)

    [Header("Scene Names")]
    public string mainMenuScene = "MainMenu";
    public string gameScene     = "Game";

    private bool _paused;

    void Update()
    {
        HandlePause();
        EnforceKillPlane();
    }

    // ── Pause ──────────────────────────────────────────────────
    void HandlePause()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        _paused = !_paused;
        Time.timeScale = _paused ? 0f : 1f;

        // Unlock cursor so player can click resume button
        Cursor.lockState = _paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible   = _paused;
    }

    // ── Kill plane (flooded ground) ────────────────────────────
    void EnforceKillPlane()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;
        if (player.transform.position.y >= killPlaneY) return;

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        Vector3 safePos = respawnPoint != null
            ? respawnPoint.position
            : new Vector3(0f, 2f, 0f);

        player.transform.position = safePos;
        if (cc) cc.enabled = true;

        // Kill the player health-wise too
        HealthSystem hs = player.GetComponent<HealthSystem>();
        if (hs != null) hs.TakeDamage(9999f);
    }

    // ── Public buttons (called by UI) ─────────────────────────
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void ReloadScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameScene);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
