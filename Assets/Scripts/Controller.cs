using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Controller : MonoBehaviour
{
    public event Action OnGetServe;

    private Rigidbody body;

    [SerializeField]
    private float acceleration;
    [SerializeField]
    private float dampening;
    [SerializeField]
    private float maxSpeed;

    [Header("Dash"), SerializeField]
    private float dashAcceleration;
    [SerializeField]
    private float dashSpeed;
    [SerializeField]
    private float dashCooldown;

    [SerializeField]
    private Animator animator;

    private Vector2 moveVector;

    private bool inDash;
    private bool dashPerformed;
    private float nextDashTime;

    protected bool activePlayer;
    [SerializeField]
    private bool isASidePlayer;
    public bool playerA => isASidePlayer;
    [SerializeField]
    private Raquet raquet;
    public Raquet playerRaquet => raquet;
    [SerializeField]
    private Transform serveHandle;

    private void Awake() {
        body = GetComponent<Rigidbody>();
    }

    protected virtual void Update() {
        inDash = Time.time < nextDashTime;
    }

    private void FixedUpdate() {
        if (moveVector.x != 0f || moveVector.y != 0f) {
            Vector3 force = new Vector3(moveVector.x, 0, moveVector.y) * acceleration;
            body.AddForce(force, ForceMode.Acceleration);
            body.linearDamping = 0;
        }
        else if (!inDash) {
            body.linearDamping = dampening;
        }

        float max = maxSpeed;
        if (inDash) {
            max = dashSpeed;
            if (dashPerformed) {
                dashPerformed = false;
                Vector3 dashForce = moveVector * dashAcceleration;
                body.AddForce(dashForce, ForceMode.Impulse);
            }
        }

        // float min = max / 5f;
        // max = Mathf.Max(max * moveVector.magnitude, min);

        float speed = body.linearVelocity.magnitude;
        if (speed > max) {
            Vector3 backwards = -body.linearVelocity / speed;
            Vector3 correction = backwards * (speed - max);
            body.AddForce(correction, ForceMode.VelocityChange);
        }
    }

    public void Move(Vector2 inputMoveVector) {
        moveVector = inputMoveVector;
    }

    public void Dash() {
        if (inDash) return;
        nextDashTime = Time.time + dashCooldown;
        dashPerformed = true;
    }

    public void Swing() {
        animator?.SetTrigger("swing");
    }

    public Vector3 ServePosition() {
        return serveHandle.position;
    }

    public void GetServe() {
        OnGetServe?.Invoke();
    }

    public void SetASide(bool isPlayerA) {
        isASidePlayer = isPlayerA;
    }
}