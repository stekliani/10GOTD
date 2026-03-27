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
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) continue;
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
        var weightProp = property.FindPropertyRelative("weight");
        var affectedStatsProp = property.FindPropertyRelative("affectedStats");

        float y = position.y;

        // ----- Prefab Popup -----
        Rect prefabRect = new Rect(position.x, y, position.width, lineHeight);
        y += lineHeight + padding;

        int currentIndex = -1;
        if (prefabProp.objectReferenceValue != null)
            currentIndex = _cachedPrefabs.IndexOf(prefabProp.objectReferenceValue as EnemyStats);

        int selectedIndex = EditorGUI.Popup(prefabRect, "Prefab", currentIndex, _cachedOptions);

        if (selectedIndex >= 0 && selectedIndex < _cachedPrefabs.Count)
            prefabProp.objectReferenceValue = _cachedPrefabs[selectedIndex];
        else
            prefabProp.objectReferenceValue = null;

        // ----- Balance Header -----
        Rect balanceHeaderRect = new Rect(position.x, y, position.width, lineHeight);
        y += lineHeight + padding;
        EditorGUI.LabelField(balanceHeaderRect, "Balance", EditorStyles.boldLabel);

        // ----- Weight -----
        Rect weightRect = new Rect(position.x, y, position.width, lineHeight);
        y += lineHeight + padding;
        weightProp.floatValue = EditorGUI.FloatField(weightRect, "Weight", weightProp.floatValue);

        // ----- Wave Stat Scaling Header -----
        Rect scalingHeaderRect = new Rect(position.x, y, position.width, lineHeight);
        y += lineHeight + padding;
        EditorGUI.LabelField(scalingHeaderRect, "Wave Stat Scaling", EditorStyles.boldLabel);

        // ----- Affected Stats (Flags Enum) -----
        Rect affectedStatsRect = new Rect(position.x, y, position.width, lineHeight);
        affectedStatsProp.intValue = EditorGUI.MaskField(
            affectedStatsRect,
            "Affected Stats",
            affectedStatsProp.intValue,
            System.Enum.GetNames(typeof(WaveAffectedEnemyStats))
        );

        EditorGUI.EndProperty();
    }

    // ---------- HEIGHT ----------
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float padding = 2f;

        // prefab, balance header, weight, scaling header, affected stats
        return lineHeight * 5 + padding * 5;
    }
}