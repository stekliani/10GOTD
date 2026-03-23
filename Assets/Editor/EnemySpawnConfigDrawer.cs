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
        float sectionGap = 6f;

        var prefabProp = property.FindPropertyRelative("prefab");
        var countProp = property.FindPropertyRelative("spawnCount");
        var intervalProp = property.FindPropertyRelative("spawnInterval");
        var affectedStatsProp = property.FindPropertyRelative("affectedStats");

        float y = position.y;
        Rect prefabRect = new Rect(position.x, y, position.width, lineHeight);
        y += lineHeight + padding;
        Rect countRect = new Rect(position.x, y, position.width, lineHeight);
        y += lineHeight + padding;
        Rect intervalRect = new Rect(position.x, y, position.width, lineHeight);
        y += lineHeight + sectionGap;
        Rect scalingHeaderRect = new Rect(position.x, y, position.width, lineHeight);
        y += lineHeight + padding;
        Rect affectedStatsRect = new Rect(position.x, y, position.width, lineHeight);

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
        EditorGUI.LabelField(scalingHeaderRect, "Wave Stat Scaling", EditorStyles.boldLabel);
        EditorGUI.PropertyField(affectedStatsRect, affectedStatsProp);

        EditorGUI.EndProperty();
    }

    // ---------- HEIGHT ----------
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float padding = 2f;
        float sectionGap = 6f;

        // 5 lines: prefab, count, interval, scaling header, affected stats
        // plus regular paddings and one larger section gap.
        return lineHeight * 5 + padding * 4 + sectionGap;
    }
}