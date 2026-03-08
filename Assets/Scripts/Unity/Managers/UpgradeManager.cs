using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : Subject
{
    private readonly List<Weapon> _activeWeapons = new();
    private readonly System.Random _rng          = new();

    private PlayerStats     _playerStats;
    private PlayerInventory _playerInventory;

    private void Start()
    {
        _playerStats     = FindObjectOfType<PlayerStats>();
        _playerInventory = FindObjectOfType<PlayerInventory>();
    }

    public void RegisterWeaponInstance(Weapon weapon)
    {
        if (weapon != null && !_activeWeapons.Contains(weapon))
            _activeWeapons.Add(weapon);
    }

    public void UnregisterWeaponInstance(Weapon weapon) => _activeWeapons.Remove(weapon);

    public List<Weapon> GetRandomUpgrades(int amount)
    {
        List<IWeapon> eligible = UpgradeSystem.FilterEligible(
            _activeWeapons.Cast<IWeapon>().ToList());

        List<IWeapon> chosen = UpgradeSystem.GetRandomUpgrades(eligible, amount, _rng);

        return chosen.Cast<Weapon>().ToList();
    }

    public List<Weapon> GetActivatedWeaponsList() =>
        _activeWeapons.Where(w => w != null && w.IsActive).ToList();

    public void UpgradePlayerStat(PlayerStatEntry stat, Button upgradeButton)
    {
        int cost = stat.GetUpgradeCost();
        if (_playerInventory.GetCoinAmount() < cost) return;

        _playerInventory.RemoveCoins(cost);
        stat.currentUpgradeLevel++;
        _playerStats.ApplyRuntimeModifier(stat.runtimeUpgradeModifier);
        NotifyObservers(InputActions.UpgradeRuntimeStats);
    }
}
