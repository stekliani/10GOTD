using System;
using UnityEngine;

public class PlayerInventory : Subject
{
    [SerializeField] private int startingCoins;
    private int _coins;

    public string SaveKey => "PlayerInventory";

    private void Awake()
    {
        _coins = startingCoins;
    }

    public void AddCoins(int amount)
    {
        _coins += amount;
        NotifyObservers(InputActions.UpgradeRuntimeStats);
    }

    public void RemoveCoins(int amount)
    {
        _coins -= amount;
        NotifyObservers(InputActions.UpgradeRuntimeStats);
        if (_coins < 0) { Debug.LogError("Coins went negative!"); _coins = 0; }
    }

    public int GetCoinAmount() => _coins;

    public object CaptureState()
    {
        return new PlayerInventoryData
        {
            coins = _coins,
        };
    }

    public void RestoreState(object state)
    {
        // Handle null or missing save data - use default starting coins
        if (state == null)
        {
            Debug.Log("No save data found for PlayerInventory. Using starting coins.");
            _coins = startingCoins;
            return;
        }

        try
        {
            var data = (PlayerInventoryData)state;
            if (data == null)
            {
                _coins = startingCoins;
                return;
            }

            _coins = data.coins;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to restore PlayerInventory state: {e.Message}. Using starting coins.");
            _coins = startingCoins;
        }
    }

    [Serializable]
    public class PlayerInventoryData
    {
        public int coins;
    }
}
