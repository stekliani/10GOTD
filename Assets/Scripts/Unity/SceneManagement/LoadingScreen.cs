using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressText;
    private string _sceneToLoad = SceneLoader.Scene.Menu.ToString();
    private Scene _loadedScene;

    private void Awake()
    {
        EnhancedTouchSupport.Enable();
    }

    private void Start()
    {
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
    {
        var locInit = LocalizationSettings.InitializationOperation;
        while (!locInit.IsDone)
            yield return null;

        if (SceneLoader.SceneToLoad != "")
            _sceneToLoad = SceneLoader.SceneToLoad;

        Application.backgroundLoadingPriority = UnityEngine.ThreadPriority.High;
        yield return null;

        string loadingSceneName = SceneManager.GetActiveScene().name;

        // Additive keeps Loading visible until unload. Do NOT use allowSceneActivation = false — on many
        // Android builds progress stalls around 0.82–0.89 and never reaches 0.9.
        var operation = SceneManager.LoadSceneAsync(_sceneToLoad, LoadSceneMode.Additive);
        if (operation == null)
        {
            Debug.LogError($"LoadingScreen: LoadSceneAsync failed for '{_sceneToLoad}'. Check Build Settings and scene name.", this);
            yield break;
        }

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            if (progressBar != null)
                progressBar.value = progress;
            if (progressText != null)
                progressText.text = Mathf.RoundToInt(progress * 100f) + "%";

            yield return null;
        }

        Scene loaded = SceneManager.GetSceneByName(_sceneToLoad);
        if (loaded.IsValid())
        {
            _loadedScene = loaded;

            SceneManager.SetActiveScene(loaded);
            PauseGame();

            MuteSceneAudio(_loadedScene);
        }

        // Two EventSystems (Loading + Menu) while additive — disable Loading's; continue uses Input System.
        Scene loadingScene = SceneManager.GetSceneByName(loadingSceneName);
        DisableEventSystemsInScene(loadingScene);

        if (progressBar != null)
            progressBar.value = 1f;
        if (progressText != null)
            progressText.text = GetPressToContinueLabel();
        yield return null;

        while (!WasContinuePressed())
            yield return null;

        // Paused after Menu became active — resume here so gameplay / UI timers run again before we unload Loading.
        ResumeGame();
        UnmuteSceneAudio(_loadedScene);

        if (!string.IsNullOrEmpty(loadingSceneName))
            yield return SceneManager.UnloadSceneAsync(loadingSceneName);
    }

    private static void DisableEventSystemsInScene(Scene scene)
    {
        if (!scene.IsValid())
            return;

        foreach (var es in UnityEngine.Object.FindObjectsOfType<EventSystem>(true))
        {
            if (es != null && es.gameObject.scene == scene)
                es.gameObject.SetActive(false);
        }
    }

    private static string GetPressToContinueLabel()
    {
        try
        {
            if (LocalizationSettings.InitializationOperation.IsDone)
                return L.Get("Loading", "Loading.pressAnyKey");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"LoadingScreen: could not localize continue prompt. {e.Message}");
        }

        return "Tap to continue";
    }

    private static bool WasContinuePressed()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            return true;
        }

        var pointer = Pointer.current;
        if (pointer != null && pointer.press.wasPressedThisFrame)
        {
            return true;
        }
        if (Mouse.current != null &&
            (Mouse.current.leftButton.wasPressedThisFrame ||
             Mouse.current.rightButton.wasPressedThisFrame ||
             Mouse.current.middleButton.wasPressedThisFrame))
        {
            return true;
        }
        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.press.wasPressedThisFrame)
                {
                    return true;
                }
            }
        }

        foreach (var t in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
        {
            if (t.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                return true;
            }
        }

        foreach (var pad in Gamepad.all)
        {
            if (pad == null)
                continue;
            if (pad.buttonSouth.wasPressedThisFrame || pad.startButton.wasPressedThisFrame)
            {
                return true;
            }
        }

        return false;
    }

    private static void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
    }

    private static void MuteSceneAudio(Scene scene)
    {
        foreach (var audio in UnityEngine.Object.FindObjectsOfType<AudioSource>(true))
        {
            if (audio.gameObject.scene == scene)
            {
                audio.mute = true;
            }
        }
    }

    private static void UnmuteSceneAudio(Scene scene)
    {
        foreach (var audio in UnityEngine.Object.FindObjectsOfType<AudioSource>(true))
        {
            if (audio.gameObject.scene == scene)
            {
                audio.mute = false;
            }
        }
    }
}
