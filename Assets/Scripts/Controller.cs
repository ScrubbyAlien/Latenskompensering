using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Controller : MonoBehaviour
{
    private Rigidbody body;
    
    [SerializeField]
    private float acceleration;
    [SerializeField]
    private float dampening;
    [SerializeField]
    private float maxSpeed;

    [Header("Dash")]
    [SerializeField]
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
    
    void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    protected virtual void Update() {
        inDash = Time.time < nextDashTime;
    }

    private void FixedUpdate() {
        if (moveVector.x != 0f || moveVector.y != 0f) {
            Vector3 force = new Vector3(moveVector.x, 0, moveVector.y) * acceleration;
            body.AddForce(force, ForceMode.Acceleration);
        }
        else if (!inDash) {
            body.linearVelocity *= dampening;
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
        
        float speed = body.linearVelocity.magnitude;
        if (speed > max) {
            Vector3 backwards = -body.linearVelocity / speed;
            Vector3 correction = backwards * (speed - max);
            body.AddForce(correction, ForceMode.VelocityChange);
        }
    }

    protected void Move(Vector2 inputMoveVector) {
        moveVector = inputMoveVector;
    }

    protected void Dash() {
        if (inDash) return;
        nextDashTime = Time.time + dashCooldown;
        dashPerformed = true;
    }

    protected void Swing() {
        animator?.SetTrigger("swing");
    }
    
    public Vector3 ServePosition() {
        return serveHandle.position;
    }

    public virtual void OnGetServe() { }
}
