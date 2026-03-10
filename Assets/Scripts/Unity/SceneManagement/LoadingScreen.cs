using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressText;
    private string _defaultSceneName = "Menu";
    private void Start()
    {
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
    {
        if (SceneLoader.SceneToLoad != "")
        {
            _defaultSceneName = SceneLoader.SceneToLoad;
        }
        AsyncOperation operation = SceneManager.LoadSceneAsync(_defaultSceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            progressBar.value = progress;
            progressText.text = Mathf.RoundToInt(progress * 100f) + "%";

            if (operation.progress >= 0.9f)
            {
                // loading finished
                progressBar.value = 1f;
                progressText.text = "Press Space Bar To Continue...";

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    operation.allowSceneActivation = true;
                }
            }

            yield return null;
        }
    }
}