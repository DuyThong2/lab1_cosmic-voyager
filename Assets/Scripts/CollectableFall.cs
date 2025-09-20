using UnityEngine;

public class CollectibleFall : MonoBehaviour
{
    public float speed = 3f;
    public float destroyBelowY;  // ?i?m ?áy ?? hu?

    void Update()
    {
        transform.position += Vector3.down * speed * Time.deltaTime;
        if (transform.position.y < destroyBelowY) Destroy(gameObject);
    }
}