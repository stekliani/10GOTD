using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnemySpawnConfig))]
public class EnemySpawnConfigDrawer : PropertyDrawer
{
    // ---------- CACHE ----------
    private static List<EnemyStats> _cachedPrefabs;
    private static string[] _cachedOptions;

    private static void BuildCache()
    {
        _cachedPrefabs = new List<EnemyStats>();

        // Search ONLY prefabs
        string[] guids = AssetDatabase.FindAssets("t:Prefab");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (go == null) continue;

            // Only prefabs that contain EnemyStats
            EnemyStats enemy = go.GetComponent<EnemyStats>();
            if (enemy != null)
                _cachedPrefabs.Add(enemy);
        }

        _cachedOptions = _cachedPrefabs
            .Select(e => e.gameObject.name)
            .ToArray();
    }

    // ---------- DRAW ----------
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (_cachedPrefabs == null)
            BuildCache();

        EditorGUI.BeginProperty(position, label, property);

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float padding = 2f;

        var prefabProp = property.FindPropertyRelative("prefab");
        var countProp = property.FindPropertyRelative("spawnCount");
        var intervalProp = property.FindPropertyRelative("spawnInterval");

        Rect prefabRect = new Rect(position.x, position.y, position.width, lineHeight);
        Rect countRect = new Rect(position.x, position.y + lineHeight + padding, position.width, lineHeight);
        Rect intervalRect = new Rect(position.x, position.y + 2 * (lineHeight + padding), position.width, lineHeight);

        // ----- Current selection -----
        int currentIndex = -1;

        if (prefabProp.objectReferenceValue != null)
        {
            currentIndex = _cachedPrefabs.IndexOf(
                prefabProp.objectReferenceValue as EnemyStats
            );
        }

        // ----- Popup -----
        int selectedIndex = EditorGUI.Popup(
            prefabRect,
            "Prefab",
            currentIndex,
            _cachedOptions
        );

        // ----- Assign selection -----
        if (selectedIndex >= 0 && selectedIndex < _cachedPrefabs.Count)
            prefabProp.objectReferenceValue = _cachedPrefabs[selectedIndex];
        else
            prefabProp.objectReferenceValue = null;

        // ----- Other fields -----
        EditorGUI.PropertyField(countRect, countProp);
        EditorGUI.PropertyField(intervalRect, intervalProp);

        EditorGUI.EndProperty();
    }

    // ---------- HEIGHT ----------
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float padding = 2f;
        return lineHeight * 3 + padding * 4;
    }
}