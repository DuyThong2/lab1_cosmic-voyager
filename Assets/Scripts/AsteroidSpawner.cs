using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class AsteroidEntry
{
    public string id = "asteroid";
    public GameObject prefab;                   // prefab có Collider2D (+ Health, DamageDealer …)
    [Min(0f)] public float weight = 1f;         // tỉ lệ xuất hiện tương đối

    [Header("Rơi & xoay")]
    public Vector2 speedRange = new Vector2(3f, 6f);       // tốc độ rơi xuống (y)
    public Vector2 driftXRange = new Vector2(-1.0f, 1.0f); // lệch ngang
    public Vector2 angularRange = new Vector2(-180f, 180f); // độ/giây
    public Vector2 scaleRange = new Vector2(0.8f, 1.6f);   // phóng to/thu nhỏ ngẫu nhiên
}

public class AsteroidSpawner : MonoBehaviour
{
    [Header("Danh sách thiên thạch")]
    public List<AsteroidEntry> asteroids = new();

    [Header("Nhịp spawn")]
    public Vector2 spawnIntervalRange = new Vector2(0.4f, 1.0f);

    [Header("Theo camera")]
    public bool followCameraTop = true;
    public float topOffset = 1.0f;
    public float xPadding = 0.5f;
    public float destroyBelowY = -6f;

    float minX, maxX, topY;

    void Start()
    {
        RecalcBounds();
        StartCoroutine(SpawnLoop());
    }

    void LateUpdate()
    {
        if (followCameraTop) RecalcBounds();
    }

    void RecalcBounds()
    {
        var cam = Camera.main;
        var min = cam.ViewportToWorldPoint(new Vector2(0, 0));
        var max = cam.ViewportToWorldPoint(new Vector2(1, 1));
        minX = min.x + xPadding;
        maxX = max.x - xPadding;
        topY = max.y + topOffset;
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            float wait = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
            yield return new WaitForSeconds(wait);

            var cfg = PickByWeight(asteroids);
            if (cfg?.prefab == null) continue;

            SpawnOne(cfg);
        }
    }

    void SpawnOne(AsteroidEntry cfg)
    {
        float x = Random.Range(minX, maxX);
        Vector3 pos = new Vector3(x, topY, 0f);
        var go = Instantiate(cfg.prefab, pos, Quaternion.identity);

        // random size
        float s = Random.Range(cfg.scaleRange.x, cfg.scaleRange.y);
        go.transform.localScale = new Vector3(s, s, 1f);

        float speedY = Random.Range(cfg.speedRange.x, cfg.speedRange.y);
        float driftX = Random.Range(cfg.driftXRange.x, cfg.driftXRange.y);
        float angular = Random.Range(cfg.angularRange.x, cfg.angularRange.y);

        // Ưu tiên dùng Rigidbody2D nếu có
        if (go.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.bodyType = RigidbodyType2D.Kinematic;  // arcade control
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(driftX, -speedY);
            rb.angularVelocity = angular;             // deg/s (Z)
        }
        else
        {
            // fallback: dịch chuyển thủ công
            var fall = go.AddComponent<AsteroidFall>();
            fall.velocity = new Vector2(driftX, -speedY);
            fall.angularSpeed = angular;
            fall.destroyBelowY = destroyBelowY;
        }
    }

    AsteroidEntry PickByWeight(List<AsteroidEntry> list)
    {
        if (list == null || list.Count == 0) return null;
        float total = 0f;
        foreach (var e in list) total += Mathf.Max(0f, e.weight);
        if (total <= 0f) return list[Random.Range(0, list.Count)];
        float r = Random.value * total, acc = 0f;
        foreach (var e in list)
        {
            acc += Mathf.Max(0f, e.weight);
            if (r <= acc) return e;
        }
        return list[^1];
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        RecalcBounds();
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(minX, topY, 0), new Vector3(maxX, topY, 0));
    }
#endif
}
