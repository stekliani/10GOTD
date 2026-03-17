using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressText;
    private string _sceneToLoad = SceneLoader.Scene.Menu.ToString();
    private void Start()
    {
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
    {
        if (SceneLoader.SceneToLoad != "")
        {
            _sceneToLoad = SceneLoader.SceneToLoad;
        }
        AsyncOperation operation = SceneManager.LoadSceneAsync(_sceneToLoad);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            progressBar.value = progress;
            progressText.text = Mathf.RoundToInt(progress * 100f) + "%";

            if (operation.progress >= 0.9f && MainPoolManager.Instance.isInitialized)
            {
                // loading finished
                progressBar.value = 1f;
                progressText.text = L.Get("Loading", progressText.name);

                if (Input.anyKeyDown)
                {
                    operation.allowSceneActivation = true;
                }
            }

            yield return null;
        }
    }
}