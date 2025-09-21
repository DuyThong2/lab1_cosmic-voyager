using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class ShieldController : MonoBehaviour
{
    [Header("Shield")]
    [SerializeField] public float duration = 5f;
    [SerializeField]public float cooldown = 3f;
    [SerializeField] public ParticleSystem shieldVFX;

    public int charges { get; private set; }
    public float activeRemaining { get; private set; }
    public float cooldownRemaining { get; private set; }
    public bool IsActive => activeRemaining > 0f;
    public bool CanActivate => charges > 0 && !IsActive && cooldownRemaining <= 0f;

    Health health;
    public event Action OnChanged;

    void Awake() {
        health = GetComponent<Health>();
        cooldownRemaining = cooldown;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        if (IsActive)
        {
            activeRemaining -= dt;
            if (activeRemaining <= 0f)
            {
                activeRemaining = 0f;
                cooldownRemaining = cooldown;
                if(shieldVFX) shieldVFX.Stop(); // Tắt shield VFX
                Debug.Log("Shield expired → start cooldown");
                OnChanged?.Invoke();
            }
            else OnChanged?.Invoke();
        }
        else if (cooldownRemaining > 0f)
        {
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - dt);
            Debug.Log($"Shield cooldown remaining: {cooldownRemaining:F1} s");
            // Khi cooldown vừa chạm 0 -> gọi AddCharge
            if (cooldownRemaining <= 0f)
            {
                AddCharge(false);
            }
            OnChanged?.Invoke();
        }
    }


    // Input System sẽ gọi hàm này
    public void OnShield(InputValue value)
    {
        if (value.isPressed)
        {
            Debug.Log("OnShield action pressed!");
            TryActivate();
        }
    }

    public void AddCharge(bool bypassCooldown = false)
    {
        charges++;
        if (bypassCooldown) cooldownRemaining = 0f;
        Debug.Log($"Shield charges added → now: {charges}, bypassCooldown={bypassCooldown}");
        OnChanged?.Invoke();
    }

    public bool TryActivate()
    {
        if (!CanActivate)
        {
            Debug.Log("Shield TryActivate → FAILED (no charges or still cooldown)");
            return false;
        }

        charges = Mathf.Max(0, charges - 1);
        activeRemaining = duration;
        if (health) {
            health.SetInvulnerable(duration);
            Debug.Log(health.invulnerable);
                
        }
        
        if (shieldVFX) shieldVFX.Play(); // Bật shield VFX

        Debug.Log($"Shield ACTIVATED! duration={duration}s, remaining charges={charges}");
        OnChanged?.Invoke();
        return true;
    }
}
