using System;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public CollectibleSO data;

    void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<Player>();
        if (!player) return;

        switch (data.type)
        {
            case CollectibleType.Coin:
                FindObjectOfType<ScoreKeeper>()?.ModifyScore(data.value);
                break;

            case CollectibleType.Fuel:
                player.GetComponent<Health>()?.AddHealth(data.value);
                break;

            case CollectibleType.Shield:
                player.GetComponent<ShieldController>()?.AddCharge(data.bypassCooldown);
                break;

            case CollectibleType.Thruster:
                player.BoostSpeed(data.factor, data.duration);
                break;
        }

        Debug.Log(data.type);

        Destroy(gameObject);
    }
}
