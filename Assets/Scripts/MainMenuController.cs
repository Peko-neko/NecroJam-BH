using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject continueButton;

    void Start()
    {
        continueButton.SetActive(SaveSystem.HasSave());
    }

    public void ContinueGame()
    {
        string savedScene = SaveSystem.GetSavedScene();
        SceneManager.LoadScene(savedScene);
    }

    public void NewGame()
    {
        SaveSystem.ClearSave(); // wipe old progress so a fresh run doesn't inherit it
        SceneManager.LoadScene("Stage1");
    }

    public void QuitGame()
    {
        Debug.Log("Quit requested");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}