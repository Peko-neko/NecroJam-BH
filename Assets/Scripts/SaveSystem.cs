using UnityEngine;

public static class SaveSystem
{
    private const string HasSaveKey = "HasSave";
    private const string SavedSceneKey = "SavedScene";

    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(HasSaveKey) && PlayerPrefs.GetInt(HasSaveKey) == 1;
    }

    public static void SaveProgress(string sceneName)
    {
        PlayerPrefs.SetInt(HasSaveKey, 1);
        PlayerPrefs.SetString(SavedSceneKey, sceneName);
        PlayerPrefs.Save();
    }

    public static string GetSavedScene()
    {
        return PlayerPrefs.GetString(SavedSceneKey, "Stage1"); // fallback if missing
    }

    public static void ClearSave()
    {
        PlayerPrefs.DeleteKey(HasSaveKey);
        PlayerPrefs.DeleteKey(SavedSceneKey);
    }
}