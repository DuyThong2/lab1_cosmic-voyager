using UnityEngine;

public class AsteroidFall : MonoBehaviour
{
    public Vector2 velocity;       // (vx, -speed)
    public float angularSpeed;     // deg/s
    public float destroyBelowY = -6f;

    void Update()
    {
        transform.position += (Vector3)(velocity * Time.deltaTime);
        transform.Rotate(0f, 0f, angularSpeed * Time.deltaTime);
        if (transform.position.y < destroyBelowY) Destroy(gameObject);
    }
}
