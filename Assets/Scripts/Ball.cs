using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour
{
    public event Action<Controller, bool> BallDied;
    
    [SerializeField]
    private Transform shadow;
    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    private float lingerTime;
    private Rigidbody body;
    public bool dead { get; private set; }
    private float despawnTime;

    [HideInInspector]
    public Controller lastPlayerHit;

    public Vector3 velocity => body.linearVelocity; 
    public bool beingServed { get; private set; }
    public Controller servingPlayer { get; private set; }

    private void Awake() {
        body = GetComponent<Rigidbody>();
    }

    private void Update() {
        if (beingServed) transform.position = servingPlayer.ServePosition();
        shadow.position = transform.position.XZ();
        if (dead && Time.time > despawnTime) {
            Destroy(gameObject);            
        }

        if (transform.position.sqrMagnitude > 1000) {
            BallDied?.Invoke(lastPlayerHit, false);
            Die();
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (dead) return;
        if (other.tag == "OutOfBounds") {
            transform.position += new Vector3(0, other.transform.position.y + transform.localScale.y, 0);
            Die();
            // score point for opponent to lastPlayerHit
            BallDied?.Invoke(lastPlayerHit, false);
        }
        if (other.tag == "Court") {
            transform.position += new Vector3(0, other.transform.position.y + transform.localScale.y, 0);
            Die();
            // score point for lastPlayerHit
            bool ownCourt = !(transform.position.z > 0 ^ lastPlayerHit.playerA);
            BallDied?.Invoke(lastPlayerHit, !ownCourt);
        }
    }

    public void EnterServeState(Controller player, int score) {
        beingServed = true;
        servingPlayer = player;
        player.OnGetServe();
        body.isKinematic = true;
    }

    public void ExitServeState() {
        beingServed = false;
        servingPlayer = null;
        body.isKinematic = false;
    }

    public void LaunchToPosition(Vector3 destination, Vector3 netIntersection, float height) {
        if (dead) return;
        Vector3 difference = destination.XZ() - transform.position.XZ();
        Vector3 netDifference = netIntersection.XZ() - transform.position.XZ();
        // max ensures no 2 * time is never zero a couple lines down
        Vector2 fixedPoint = new Vector2(netDifference.magnitude, Mathf.Max(height - transform.position.y, 0.01f));
        
        // messed around on desmos to get these values, too much algebra gd
        float distance = difference.magnitude;
        float a = fixedPoint.y / (fixedPoint.x * fixedPoint.x - distance * fixedPoint.x);
        float b = -a * distance;

        SecondDegreeFunction arc = new SecondDegreeFunction(a, b, 0);
        float maxHeight = arc.Evaluate(distance / 2);
        
        float time = 2 * Mathf.Sqrt(maxHeight / (-Physics.gravity.y * 2));
        float yVel = -Physics.gravity.y * time;
        
        Vector3 velocityHorizontal = difference / (2 * time);
        Vector3 velocityVertical = new Vector3(0, yVel, 0);
        SetVelocity(velocityHorizontal + velocityVertical);
    }
    
    public void SetVelocity(Vector3 velocity) {
        Debug.Log($"{velocity}, {velocity.magnitude}");
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        body.linearVelocity = velocity;
    }

    private void Die() {
        dead = true;
        body.isKinematic = true;
        body.detectCollisions = false;
        despawnTime = Time.time + lingerTime;
    }

    private struct SecondDegreeFunction
    {
        // ax2 + bx + c
        private float a;
        private float b;
        private float c;

        public SecondDegreeFunction(float a, float b, float c) {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public float Evaluate(float x) {
            return a * x * x + b * x + c;
        }

        public float Slope(float x) {
            return 2 * a * x + b;
        }
    }
    
}