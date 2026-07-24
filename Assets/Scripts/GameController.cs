using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] private float clearDelay = 5f;
    [SerializeField] private string nextSceneName = "Stage2";
    [SerializeField] private float transitionDelay = 2f;

    private float noEnemyTimer = 0f;
    private bool stageCleared = false;

    void Update()
    {
        if (stageCleared) return;

        bool enemiesRemain = GameObject.FindGameObjectWithTag("Enemy") != null;

        if (enemiesRemain)
        {
            noEnemyTimer = 0f;
        }
        else
        {
            noEnemyTimer += Time.deltaTime;
            if (noEnemyTimer >= clearDelay)
            {
                StageClear();
            }
        }
    }

    private void StageClear()
    {
        stageCleared = true;
        Debug.Log("Stage complete. 0 enemies left");

        Invoke(nameof(LoadNextScene), transitionDelay);
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}