using UnityEngine;

/// <summary>Defines a timed stat buff that can be picked up or applied by events.</summary>
[CreateAssetMenu(menuName = "Buffs/Stat Buff")]
public class BuffDefinitionSO : ScriptableObject
{
    [Header("ID")]
    public string buffID;

    [Header("Stats")]
    public StatsModifier modifier;

    [Header("Duration")]
    public float duration = 5f;

    [Header("Stacking")]
    public BuffStackType stackType;

    [Header("Optional")]
    public Sprite icon;
}
