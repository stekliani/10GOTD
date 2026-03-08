using UnityEngine;

public class BuffPickup : MonoBehaviour
{
    [SerializeField] private BuffDefinitionSO buff;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerStats player))
        {
            player.ApplyBuff(buff);
            Destroy(gameObject);
        }
    }
}
