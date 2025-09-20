using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class SpawnEntry
{
    public string id;                     // tên để phân biệt (Coin, Fuel, Thruster)
    public GameObject prefab;             // prefab có CollectibleFall + Pickup
    [Min(0f)] public float weight = 1f;   // tỉ lệ rớt tương đối
    public Vector2 speedRange = new Vector2(2f, 4f); // random speed
}

public class CollectibleSpawner : MonoBehaviour
{
    [Header("Ngẫu nhiên (Coin/Fuel/Thruster...)")]
    public List<SpawnEntry> items = new List<SpawnEntry>();

    [Header("Shield định kỳ")]
    public GameObject shieldPrefab;
    public Vector2 shieldSpeedRange = new Vector2(2f, 3f);
    public float shieldInterval = 30f;

    [Header("Spawn timing")]
    public Vector2 spawnIntervalRange = new Vector2(0.8f, 1.6f);

    [Header("Theo camera")]
    public bool followCameraTop = true;
    public float topOffset = 1.0f;
    public float xPadding = 0.5f;
    public float destroyBelowY = -6f; // điểm hủy mặc định

    float minX, maxX, topY;

    void Start()
    {
        RecalcBounds();
        StartCoroutine(SpawnLoop());
        if (shieldPrefab != null) StartCoroutine(ShieldLoop());
    }

    void LateUpdate()
    {
        if (followCameraTop) RecalcBounds();
    }

    void RecalcBounds()
    {
        var cam = Camera.main;
        Vector2 min = cam.ViewportToWorldPoint(new Vector2(0, 0));
        Vector2 max = cam.ViewportToWorldPoint(new Vector2(1, 1));
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

            var cfg = PickByWeight(items);
            if (cfg == null || cfg.prefab == null) continue;

            float speed = Random.Range(cfg.speedRange.x, cfg.speedRange.y);
            SpawnOne(cfg.prefab, speed);
        }
    }

    IEnumerator ShieldLoop()
    {
        yield return new WaitForSeconds(2f);
        while (true)
        {
            float speed = Random.Range(shieldSpeedRange.x, shieldSpeedRange.y);
            SpawnOne(shieldPrefab, speed);
            yield return new WaitForSeconds(Mathf.Max(0.1f, shieldInterval));
        }
    }

    void SpawnOne(GameObject prefab, float speed)
    {
        float x = Random.Range(minX, maxX);
        Vector3 pos = new Vector3(x, topY, 0f);
        var go = Instantiate(prefab, pos, Quaternion.identity);

        var fall = go.GetComponent<CollectibleFall>();
        if (fall != null)
        {
            fall.speed = speed;
            fall.destroyBelowY = destroyBelowY;
        }
        else
        {
            Debug.LogWarning($"Prefab '{prefab.name}' chưa gắn CollectibleFall!");
        }
    }

    SpawnEntry PickByWeight(List<SpawnEntry> list)
    {
        if (list == null || list.Count == 0) return null;
        float total = 0f;
        foreach (var e in list) total += Mathf.Max(0f, e.weight);
        if (total <= 0f) return list[Random.Range(0, list.Count)];

        float r = Random.value * total;
        float acc = 0f;
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
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(minX, topY, 0), new Vector3(maxX, topY, 0));
    }
#endif
}
