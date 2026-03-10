using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static string SceneToLoad = "";
    public enum Scene
    {
        Menu,
        Level1
    }
    public static void Load(string sceneName)
    {
        SceneToLoad = sceneName;
        SceneManager.LoadScene("Loading");
    }
}