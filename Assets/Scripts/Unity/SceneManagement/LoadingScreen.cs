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

    private float _sceneProgress = 0f;
    private float _poolProgress = 0f;

    private void Awake()
    {
        EnhancedTouchSupport.Enable();
        SetSquareAspect();
    }

    private void Start()
    {
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
    {
        // --- Localization init ---
        var locInit = LocalizationSettings.InitializationOperation;
        while (!locInit.IsDone)
            yield return null;

        if (!string.IsNullOrEmpty(SceneLoader.SceneToLoad))
            _sceneToLoad = SceneLoader.SceneToLoad;

        Application.backgroundLoadingPriority = ThreadPriority.High;

        string loadingSceneName = SceneManager.GetActiveScene().name;

        // --- Start loading target scene ---
        var operation = SceneManager.LoadSceneAsync(_sceneToLoad, LoadSceneMode.Additive);
        if (operation == null)
        {
            Debug.LogError($"Failed to load scene {_sceneToLoad}");
            yield break;
        }

        // --- Track UNITY loading progress (REAL) ---
        while (!operation.isDone)
        {
            _sceneProgress = Mathf.Clamp01(operation.progress / 0.9f);
            UpdateUI();
            yield return null;
        }

        // --- Scene fully loaded & instantiated here ---
        _loadedScene = SceneManager.GetSceneByName(_sceneToLoad);

        if (_loadedScene.IsValid())
        {
            SceneManager.SetActiveScene(_loadedScene);
            PauseGame();
            MuteSceneAudio(_loadedScene);
        }

        // Disable loading scene EventSystems
        Scene loadingScene = SceneManager.GetSceneByName(loadingSceneName);
        DisableEventSystemsInScene(loadingScene);

        // --- STEP 2: Find MainPoolManager AFTER load ---
        MainPoolManager pool = null;

        foreach (var root in _loadedScene.GetRootGameObjects())
        {
            pool = root.GetComponentInChildren<MainPoolManager>(true);
            if (pool != null)
                break;
        }

        // --- STEP 3: Run REAL pool initialization ---
        if (pool != null)
        {
            yield return pool.InitializePoolsAsync(p =>
            {
                _poolProgress = p;
                UpdateUI();
            });
        }
        else
        {
            _poolProgress = 1f; // no pool = nothing to load
        }

        // --- Now everything is ACTUALLY ready ---
        UpdateUI(final: true);
        yield return null;

        // --- Wait for input ---
        while (!WasContinuePressed())
            yield return null;

        ResumeGame();
        UnmuteSceneAudio(_loadedScene);

        if (!string.IsNullOrEmpty(loadingSceneName))
            yield return SceneManager.UnloadSceneAsync(loadingSceneName);
    }

    // --- REAL combined progress ---
    private void UpdateUI(bool final = false)
    {
        float totalProgress = (_sceneProgress + _poolProgress) * 0.5f;

        if (final)
            totalProgress = 1f;

        if (progressBar != null)
            progressBar.value = totalProgress;

        if (progressText != null)
        {
            if (final)
                progressText.text = GetPressToContinueLabel();
            else
                progressText.text = Mathf.RoundToInt(totalProgress * 100f) + "%";
        }
    }

    private static void DisableEventSystemsInScene(Scene scene)
    {
        if (!scene.IsValid())
            return;

        foreach (var es in UnityEngine.Object.FindObjectsOfType<EventSystem>(true))
        {
            if (es.gameObject.scene == scene)
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
            Debug.LogWarning($"Localization failed: {e.Message}");
        }

        return "Tap to continue";
    }

    private static bool WasContinuePressed()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            return true;

        if (Mouse.current != null &&
            (Mouse.current.leftButton.wasPressedThisFrame ||
             Mouse.current.rightButton.wasPressedThisFrame ||
             Mouse.current.middleButton.wasPressedThisFrame))
            return true;

        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
                if (touch.press.wasPressedThisFrame)
                    return true;
        }

        foreach (var t in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
            if (t.phase == UnityEngine.InputSystem.TouchPhase.Began)
                return true;

        foreach (var pad in Gamepad.all)
            if (pad.buttonSouth.wasPressedThisFrame || pad.startButton.wasPressedThisFrame)
                return true;

        return false;
    }

    private static void ResumeGame() => Time.timeScale = 1f;
    private void PauseGame() => Time.timeScale = 0f;

    private static void MuteSceneAudio(Scene scene)
    {
        foreach (var audio in UnityEngine.Object.FindObjectsOfType<AudioSource>(true))
        {
            if (audio.gameObject.scene == scene)
                audio.mute = true;
        }
    }

    private static void UnmuteSceneAudio(Scene scene)
    {
        foreach (var audio in UnityEngine.Object.FindObjectsOfType<AudioSource>(true))
        {
            if (audio.gameObject.scene == scene)
                audio.mute = false;
        }
    }


    private void SetSquareAspect()
    {
        Screen.SetResolution(2960, 2960, true);
    }
}