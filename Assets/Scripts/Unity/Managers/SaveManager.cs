using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveManager
{
    private static Dictionary<string, string> _data = new();
    private static string SavePath => Application.persistentDataPath + "/save.json";

    // ---------------- SAVE ----------------
    public static void Save<T>(string key, T data)
    {
        var wrapper = new SaveWrapper<T>(data);
        string json = JsonUtility.ToJson(wrapper);
        _data[key] = json;
        WriteToDisk();
    }

    // ---------------- LOAD ---------------- 
    public static T Load<T>(string key, T defaultValue = default)
    {
        if (!_data.TryGetValue(key, out string json))
            return defaultValue;

        try
        {
            var wrapper = JsonUtility.FromJson<SaveWrapper<T>>(json);
            if (wrapper == null)
                return defaultValue;
            
            return wrapper.value;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to load data for key '{key}': {e.Message}. Using default value.");
            return defaultValue;
        }
    }

    // ---------------- DISK ----------------
    private static void WriteToDisk()
    {
        File.WriteAllText(SavePath, JsonUtility.ToJson(new SaveFile(_data)));
    }

    public static void LoadFromDisk()
    {
        if (!File.Exists(SavePath))
        {
            _data = new Dictionary<string, string>();
            return;
        }

        try
        {
            string jsonContent = File.ReadAllText(SavePath);
            if (string.IsNullOrEmpty(jsonContent))
            {
                _data = new Dictionary<string, string>();
                return;
            }

            var file = JsonUtility.FromJson<SaveFile>(jsonContent);
            if (file == null)
            {
                Debug.LogWarning("Save file is corrupted. Starting with empty save data.");
                _data = new Dictionary<string, string>();
                return;
            }

            _data = file.ToDictionary();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to load save file: {e.Message}. Starting with empty save data.");
            _data = new Dictionary<string, string>();
        }
    }

    [Serializable]
    public class SaveWrapper<T>
    {
        public T value;
        public SaveWrapper(T value) => this.value = value;
    }

    [Serializable]
    public class SaveFile
    {
        public List<string> keys = new();
        public List<string> values = new();

        public SaveFile(Dictionary<string, string> dict)
        {
            foreach (var kv in dict)
            {
                keys.Add(kv.Key);
                values.Add(kv.Value);
            }
        }

        public Dictionary<string, string> ToDictionary()
        {
            var dict = new Dictionary<string, string>();

            // Handle null or mismatched arrays
            if (keys == null || values == null)
                return dict;

            int count = Mathf.Min(keys.Count, values.Count);
            for (int i = 0; i < count; i++)
            {
                if (keys[i] != null && values[i] != null)
                    dict[keys[i]] = values[i];
            }

            return dict;
        }
    }

    public static void SaveAll()
    {
        foreach (var mono in GameObject.FindObjectsOfType<MonoBehaviour>(true))
        {
            if (mono is ISaveable saveable)
            {
                var state = saveable.CaptureState();

                var method = typeof(SaveManager)
                    .GetMethod(nameof(Save))
                    .MakeGenericMethod(state.GetType());

                method.Invoke(null, new object[] { saveable.SaveKey, state });
            }
        }
    }

    public static void LoadAll()
    {
        LoadFromDisk();
        foreach (var mono in GameObject.FindObjectsOfType<MonoBehaviour>(true))
        {
            if (mono is ISaveable saveable)
            {
                if (!_data.TryGetValue(saveable.SaveKey, out string json))
                {
                    // No save data for this key - call RestoreState with null to apply defaults
                    try
                    {
                        saveable.RestoreState(null);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Failed to restore default state for '{saveable.SaveKey}': {e.Message}.");
                    }
                    continue;
                }

                try
                {
                    // Try to deserialize using the generic SaveWrapper approach
                    // We need to extract the actual type from the JSON
                    var wrapper = JsonUtility.FromJson<SaveWrapper<object>>(json);
                    if (wrapper == null || wrapper.value == null)
                    {
                        Debug.LogWarning($"Save data for '{saveable.SaveKey}' is null. Using default values.");
                        saveable.RestoreState(null);
                        continue;
                    }

                    saveable.RestoreState(wrapper.value);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Failed to restore state for '{saveable.SaveKey}': {e.Message}. Using default values.");
                    try
                    {
                        saveable.RestoreState(null);
                    }
                    catch (System.Exception e2)
                    {
                        Debug.LogWarning($"Failed to restore default state for '{saveable.SaveKey}': {e2.Message}.");
                    }
                }
            }
        }
    }
}