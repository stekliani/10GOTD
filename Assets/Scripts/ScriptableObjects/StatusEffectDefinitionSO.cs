using UnityEngine;

/// <summary>Defines a status effect (Poison / Burn) that can be applied to the player.</summary>
[CreateAssetMenu(menuName = "Buffs/Status Effect")]
public class StatusEffectDefinitionSO : ScriptableObject
{
    public enum StatusEffectType { Poison, Burn }

    public StatusEffectType type;
    public float duration;
    public float dps;
}
