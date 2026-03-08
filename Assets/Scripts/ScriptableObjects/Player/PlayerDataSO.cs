using UnityEngine;

[CreateAssetMenu(menuName = "Player/PlayerDataSO")]
public class PlayerDataSO : ScriptableObject
{
    public PlayerStatEntry maxHealth;
    public PlayerStatEntry mana;
    public PlayerStatEntry manaRegen;
    public PlayerStatEntry damageBoost;
    public PlayerStatEntry xpBonus;
    public PlayerStatEntry recovery;
    public PlayerStatEntry cooldownReduction;
    public PlayerStatEntry armor;
    public PlayerStatEntry piercing;
    public PlayerStatEntry amount;
    public PlayerStatEntry projectileSpeed;
    public PlayerStatEntry area;
}
