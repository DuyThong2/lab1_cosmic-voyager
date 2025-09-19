using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    Vector2 rawInput;

    public InputActionReference move;
    public Rigidbody2D rb;

    [SerializeField] float paddingLeft;
    [SerializeField] float paddingRight;
    [SerializeField] float paddingTop;
    [SerializeField] float paddingBottom;

    Vector2 minBounds;
    Vector2 maxBounds;

    Shooter shooter;

    float speedMultiplier = 1f;
    Coroutine speedCo;

    void Awake()
    {
        shooter = GetComponent<Shooter>();
    }

    void Start()
    {
        InitBounds();
    }

    void Update()
    {
        Move();
    }

    void InitBounds()
    {
        Camera mainCamera = Camera.main;
        minBounds = mainCamera.ViewportToWorldPoint(new Vector2(0, 0));
        maxBounds = mainCamera.ViewportToWorldPoint(new Vector2(1, 1));
    }

    //void Move()
    //{
    //    Vector2 input = move.action.ReadValue<Vector2>();
    //    rb.linearVelocity = input * moveSpeed ;
    //}

    void Move()
    {
        Vector2 delta = rawInput * (moveSpeed * speedMultiplier) * Time.deltaTime; // ← thêm speedMultiplier
        Vector2 newPos = new Vector2();
        newPos.x = Mathf.Clamp(transform.position.x + delta.x, minBounds.x + paddingLeft, maxBounds.x - paddingRight);
        newPos.y = Mathf.Clamp(transform.position.y + delta.y, minBounds.y + paddingBottom, maxBounds.y - paddingTop);
        transform.position = newPos;
    }


    void OnMove(InputValue value)
    {
        rawInput = value.Get<Vector2>();
        Debug.Log(rawInput);
    }

    void OnFire(InputValue value)
    {
        if (shooter != null)
        {
            shooter.isFiring = value.isPressed;
        }
    }

    public void BoostSpeed(float factor, float duration)
    {
        if (speedCo != null) StopCoroutine(speedCo);
        speedCo = StartCoroutine(BoostRoutine(factor, duration));
    }
    IEnumerator BoostRoutine(float factor, float duration)
    {
        speedMultiplier = factor;
        yield return new WaitForSeconds(duration);
        speedMultiplier = 1f;
        speedCo = null;
    }
}
