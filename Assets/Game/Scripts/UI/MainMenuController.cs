using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// ============================================================
//  MAIN MENU CONTROLLER  — Neural Strike
//
//  HOW TO WIRE IN UNITY
//  1. Create scene "MainMenu".
//  2. Add Canvas → Panel with background image (cyberpunk art).
//  3. Add:
//       Button "Play"      → OnClick → PlayTDM()
//       Button "DM"        → OnClick → PlayDM()
//       Button "Settings"  → OnClick → OpenSettings()
//       Button "Quit"      → OnClick → QuitGame()
//  4. Assign sceneNames in Inspector.
//  5. Add "MainMenu" and all game scenes to Build Settings.
// ============================================================

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    public string tdmScene      = "Game_TDM";
    public string dmScene       = "Game_DM";
    public string baseCapScene  = "Game_BaseCapture";

    [Header("UI Panels")]
    public GameObject mainPanel;
    public GameObject settingsPanel;

    void Start()
    {
        // Ensure cursor is visible on main menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        Time.timeScale   = 1f;

        if (mainPanel)     mainPanel.SetActive(true);
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    // ── Button callbacks ──────────────────────────────────────

    public void PlayTDM()         => SceneManager.LoadScene(tdmScene);
    public void PlayDM()          => SceneManager.LoadScene(dmScene);
    public void PlayBaseCapture() => SceneManager.LoadScene(baseCapScene);

    public void OpenSettings()
    {
        if (mainPanel)     mainPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (mainPanel)     mainPanel.SetActive(true);
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
