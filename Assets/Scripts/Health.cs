using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] bool isPlayer;
    [SerializeField] int health = 50;
    [SerializeField] int score = 50;
    [SerializeField] ParticleSystem hitEffect;

    [SerializeField] bool applyCameraShake;
    //CameraShake cameraShake;

    AudioPlayer audioPlayer;
    ScoreKeeper scoreKeeper;
    LevelManager levelManager;

    public bool invulnerable;
    [SerializeField] int maxHealth = 50;


    void Start()
    {
        if (isPlayer)
        {
            StartCoroutine(DrainHealthOverTime());
        }
    }
    void Awake()
    {
        //cameraShake = Camera.main.GetComponent<CameraShake>();
        audioPlayer = FindObjectOfType<AudioPlayer>();
        scoreKeeper = FindObjectOfType<ScoreKeeper>();
        levelManager = FindObjectOfType<LevelManager>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        DamageDealer damageDealer = other.GetComponent<DamageDealer>();

        if (damageDealer != null)
        {
            TakeDamage(damageDealer.GetDamage());
            PlayHitEffect();
            audioPlayer.PlayDamageClip();
            ShakeCamera();
            damageDealer.Hit();
        }
    }

    public int GetHealth()
    {
        return health;
    }

    void TakeDamage(int damage)
    {
        if (invulnerable)
        {
            return;
        }
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (!isPlayer)
        {
            scoreKeeper.ModifyScore(score);
        }
        else
        {
            levelManager.LoadGameOver();
        }
        Destroy(gameObject);
    }

    void PlayHitEffect()
    {
        if (hitEffect != null)
        {
            ParticleSystem instance = Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(instance.gameObject, instance.main.duration + instance.main.startLifetime.constantMax);
        }
    }

    void ShakeCamera()
    {
        //if (cameraShake != null && applyCameraShake)
        //{
        //    cameraShake.Play();
        //}
    }

    public void AddHealth(int amount)
    {
        health = Mathf.Clamp(health + amount, 0, maxHealth);
        Debug.Log(health);
        // TODO: phát event cho UI nếu cần
    }

    public void SetInvulnerable(float seconds)
    {
        if (gameObject.activeInHierarchy)
            StartCoroutine(InvulRoutine(seconds));
        Debug.Log(gameObject.activeInHierarchy);
    }

    IEnumerator InvulRoutine(float seconds)
    {
        invulnerable = true;
        yield return new WaitForSeconds(seconds);
        invulnerable = false;
    }

    IEnumerator DrainHealthOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // mỗi giây
            Debug.Log($"đang bất tử :" + invulnerable);
            if (!invulnerable) // không trừ máu nếu đang bất tử
            {
                int damage = Mathf.CeilToInt(maxHealth * 0.02f); // 2% máu
                health -= damage;
                Debug.Log($"Player mất {damage} máu theo thời gian, còn lại: {health}");

                if (health <= 0)
                {
                    Die();
                    yield break; // dừng coroutine nếu chết
                }
            }
        }
    }
}
