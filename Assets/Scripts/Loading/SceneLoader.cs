using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static string NextSceneName { get; private set; }

    public static void Load(string sceneName)
    {
        if (GameManager.Instance != null && sceneName == "MainScene")
        {
            GameManager.Instance.EnsureValidCurrentStage();
        }

        NextSceneName = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }
}
