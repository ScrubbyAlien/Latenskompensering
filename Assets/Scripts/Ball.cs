using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour
{
    [SerializeField]
    private Transform shadow;
    [SerializeField]
    private float lingerTime;
    private Rigidbody body;
    public bool dead { get; private set; }
    private float despawnTime;

    [HideInInspector]
    public PlayerController lastPlayerHit;

    private void Awake() {
        body = GetComponent<Rigidbody>();
    }

    private void Update() {
        shadow.position = transform.position.XZ();
        if (dead && Time.time > despawnTime) {
            Destroy(gameObject);            
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (dead) return;
        if (other.tag == "OutOfBounds") {
            transform.position += new Vector3(0, other.transform.position.y + transform.localScale.y, 0);
            Die();
            // score point for opponent to lastPlayerHit
        }
        if (other.tag == "Court") {
            transform.position += new Vector3(0, other.transform.position.y + transform.localScale.y, 0);
            Die();
            // score point for lastPlayerHit
        }
    }

    public void LaunchToPosition(Vector3 destination, Vector3 netIntersection, float height) {
        if (dead) return;
        Vector3 difference = destination.XZ() - transform.position.XZ();
        Vector3 netDifference = netIntersection.XZ() - transform.position.XZ();
        Vector2 fixedPoint = new Vector2(netDifference.magnitude, height);
        
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
        body.linearVelocity = Vector3.zero;
        body.AddForce(velocity, ForceMode.VelocityChange);
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