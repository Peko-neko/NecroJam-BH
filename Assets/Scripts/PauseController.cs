using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject confirmPopup;
    [SerializeField] private TMPro.TMP_Text confirmMessageText;
    
    [SerializeField] private DialogueManager dialogueManager;

    public bool IsPaused => isPaused;
private bool isPaused = false;

    private enum ConfirmAction { None, Quit, MainMenu }
    private ConfirmAction pendingAction = ConfirmAction.None;

    void Update()
    {
        if (isPaused)
        {
            UnlockCursor();
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            if (confirmPopup != null && confirmPopup.activeSelf)
            {
                ConfirmNo();
                return;
            }

            if (isPaused){
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        UnlockCursor();
    }

public void Resume()
    {
        pausePanel.SetActive(false);
        isPaused = false;

        if (dialogueManager == null || !dialogueManager.IsActive)
            Time.timeScale = 1f;
    }

    public void RestartStage()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void RequestMainMenu()
    {
        ShowConfirm(ConfirmAction.MainMenu, "Return to main menu?");
    }

    public void RequestQuit()
    {
        ShowConfirm(ConfirmAction.Quit, "Are you sure you want to exit the game?");
    }

    public void ConfirmYes()
    {
        if (confirmPopup != null) confirmPopup.SetActive(false);

        ConfirmAction action = pendingAction;
        pendingAction = ConfirmAction.None;

        if (action == ConfirmAction.Quit)
        {
            QuitGame();
        }
        else if (action == ConfirmAction.MainMenu)
        {
            GoToMainMenu();
        }
    }

    public void ConfirmNo()
    {
        if (confirmPopup != null) confirmPopup.SetActive(false);
        pendingAction = ConfirmAction.None;
    }

    private void ShowConfirm(ConfirmAction action, string message)
    {
        pendingAction = action;
        if (confirmMessageText != null) confirmMessageText.text = message;
        if (confirmPopup != null) confirmPopup.SetActive(true);
        UnlockCursor();
    }

    private static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    private void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}