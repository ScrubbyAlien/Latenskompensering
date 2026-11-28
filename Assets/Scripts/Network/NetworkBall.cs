using System;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public class NetworkBall : NetworkBehaviour
{
    public event Action<NetworkPlayer, bool> BallDied;
    private Rigidbody body;
    private NetworkReferee referee;

    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    private float lingerTime;

    [SerializeField, Header("Shadow Parameters")]
    private Transform shadow;
    [SerializeField]
    private float minSizeHeight, maxSizeHeight, minSize, maxSize;
    private float shadowThickness;

    public bool dead { get; private set; }
    private float despawnTime;

    [HideInInspector]
    public NetworkPlayer lastPlayerHit;

    public Vector3 velocity => body.linearVelocity;
    public bool beingServed { get; private set; }
    public NetworkPlayer servingPlayer { get; private set; }

    private void Awake() {
        body = GetComponent<Rigidbody>();
        shadowThickness = shadow.transform.localScale.y;
    }

    public override void OnNetworkSpawn() {
        if (IsServer) {
            shadow.gameObject.SetActive(false);
        }
    }

    private void Start() {
        referee = NetworkReferee.instance;
    }

    private void Update() {
        if (IsServer) { // update position on server
            if (beingServed) transform.position = servingPlayer.controller.ServePosition();
            shadow.position = transform.position.XZ();
        }

        if (IsClient) { // update shadow only on client
            float t = Mathf.InverseLerp(minSizeHeight, maxSizeHeight, transform.position.y);
            float size = Mathf.Lerp(minSize, maxSize, t);
            shadow.localScale = new Vector3(size, shadowThickness, size);
        }

        if (IsServer) {
            if (dead && Time.time > despawnTime) {
                DestroyRpc();
            }

            if (transform.position.sqrMagnitude > 1000) {
                BallDied?.Invoke(lastPlayerHit, false);
                Die();
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (!IsServer || dead) return;
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
            bool ownCourt = !((transform.position.z > 0) ^ lastPlayerHit.controller.playerA);
            BallDied?.Invoke(lastPlayerHit, !ownCourt);
        }
    }

    [Rpc(SendTo.Everyone)]
    public void EnterServeStateRpc(ulong playerClientId) {
        if (!referee.GetNetworkPlayerByClientId(playerClientId, out NetworkPlayer player)) return;
        beingServed = true;
        servingPlayer = player;
        player.controller.GetServe();
        if (IsServer) {
            body.isKinematic = true;
        }
    }

    [Rpc(SendTo.Everyone)]
    public void ExitServeStateRpc() {
        beingServed = false;
        servingPlayer = null;
        if (IsServer) {
            body.isKinematic = false;
        }
    }

    [Rpc(SendTo.Server)]
    public void LaunchToPositionRpc(Vector3 destination, Vector3 netIntersection, float height) {
        if (dead) return;
        Vector3 difference = destination.XZ() - transform.position.XZ();
        Vector3 netDifference = netIntersection.XZ() - transform.position.XZ();
        // max ensures no 2 * time is never zero a couple lines down
        Vector2 fixedPoint = new (netDifference.magnitude, Mathf.Max(height - transform.position.y, 0.01f));

        // messed around on desmos to get these values, too much algebra gd
        float distance = difference.magnitude;
        float a = fixedPoint.y / (fixedPoint.x * fixedPoint.x - distance * fixedPoint.x);
        float b = -a * distance;

        SecondDegreeFunction arc = new (a, b, 0);
        float maxHeight = arc.Evaluate(distance / 2);

        float time = 2 * Mathf.Sqrt(maxHeight / (-Physics.gravity.y * 2));
        float yVel = -Physics.gravity.y * time;

        Vector3 velocityHorizontal = difference / (2 * time);
        Vector3 velocityVertical = new (0, yVel, 0);
        SetVelocity(velocityHorizontal + velocityVertical);
    }

    public void SetVelocity(Vector3 velocity) {
        if (!IsServer) return;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        body.linearVelocity = velocity;
    }

    private void Die() {
        dead = true;
        body.isKinematic = true;
        body.detectCollisions = false;
        despawnTime = Time.time + lingerTime;
    }

    [Rpc(SendTo.Everyone)]
    private void DestroyRpc() {
        Destroy(gameObject);
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