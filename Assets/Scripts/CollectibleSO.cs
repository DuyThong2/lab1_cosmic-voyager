using UnityEngine;

public enum CollectibleType { Coin, Fuel, Shield, Thruster }

[CreateAssetMenu(menuName = "Cosmic/Collectible")]
public class CollectibleSO : ScriptableObject
{
    public CollectibleType type;
    public Sprite icon;          // dùng cho UI sau này
    public int value = 150;        // coin (+score), fuel (+máu)
    public float duration = 5f;  // shield/thruster thời gian chạy
    public float factor = 1f;    // thruster: hệ số speed
    public bool bypassCooldown;  // shield: bỏ cooldown khi nhặt
}
