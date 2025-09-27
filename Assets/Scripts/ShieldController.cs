using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class ShieldController : MonoBehaviour
{
    [Header("Shield")]
    [SerializeField] public float duration = 5f;
    [SerializeField] public float cooldown = 3f;
    [SerializeField] public GameObject shieldVFX;
    [SerializeField] public ParticleSystem shieldChargeVFX;

    public int charges { get; private set; }
    public float activeRemaining { get; private set; }
    public float cooldownRemaining { get; private set; }
    public bool IsActive => activeRemaining > 0f;
    public bool CanActivate => charges > 0 && !IsActive && cooldownRemaining <= 0f;

    Health health;
    public event Action OnChanged;

    // === CHEAT ===
    [Header("Cheat / Debug")]
    [SerializeField] bool cheatGodMode = false;  // trạng thái toggle

    private float blinkTimer = 0f;
    [SerializeField] private float blinkInterval = 0.2f;
    private bool isShieldVisible = true;

    void Awake()
    {
        health = GetComponent<Health>();
        cooldownRemaining = cooldown;
    }
    void Start()
    {
        shieldVFX.SetActive(false);
    }


    void Update()
    {
        float dt = Time.deltaTime;

 
        

            if (IsActive)
        {
            activeRemaining -= dt;
                if (activeRemaining <= 3f)
                {
                    blinkTimer += dt;
                    if (blinkTimer >= blinkInterval)
                    {
                        blinkTimer = 0f;
                        isShieldVisible = !isShieldVisible;
                        if (shieldVFX) shieldVFX.SetActive(isShieldVisible);
                    }
                }
                if (activeRemaining <= 0f)
            {
                activeRemaining = 0f;
                cooldownRemaining = cooldown;
                    if (shieldVFX) shieldVFX.SetActive(false);
                    Debug.Log("Shield expired → start cooldown");
                OnChanged?.Invoke();
            }
            else OnChanged?.Invoke();
        }
        else if (cooldownRemaining > 0f)
        {
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - dt);
            // Debug.Log($"Shield cooldown remaining: {cooldownRemaining:F1} s");
            if (cooldownRemaining <= 0f)
            {
                if (shieldChargeVFX) shieldChargeVFX.Play();
                AddCharge(false);
            }
            OnChanged?.Invoke();
        }

        // === CHEAT: đảm bảo bất tử luôn bật khi cheatGodMode = true,
        // kể cả khi coroutine trong Health đã cố tắt.
        if (cheatGodMode && health && !health.invulnerable)
        {
            health.invulnerable = true;
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

    // === CHEAT: Input System gọi hàm này để toggle
    public void OnCheat(InputValue value)
    {
        if (!value.isPressed) return;

        cheatGodMode = !cheatGodMode;

        if (health)
        {
            health.invulnerable = cheatGodMode;
        }

        Debug.Log($"[CHEAT] God Mode {(cheatGodMode ? "ON" : "OFF")}");
        OnChanged?.Invoke();

        // (tuỳ chọn) hiệu ứng nhỏ khi bật cheat
        if (cheatGodMode)
        {
            if (shieldVFX) shieldVFX.SetActive(true);
        }
        else
        {
            if (shieldVFX) shieldVFX.SetActive(false);
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
        if (health)
        {
            health.SetInvulnerable(duration);
            Debug.Log(health.invulnerable);
        }
        if (shieldChargeVFX) shieldChargeVFX.Stop();
        if (shieldVFX) shieldVFX.SetActive(true);

        Debug.Log($"Shield ACTIVATED! duration={duration}s, remaining charges={charges}");
        OnChanged?.Invoke();
        return true;
    }
}
