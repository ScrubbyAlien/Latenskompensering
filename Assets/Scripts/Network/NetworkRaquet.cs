using Unity.Netcode;
using UnityEngine;

public class NetworkRaquet : NetworkBehaviour
{
    [SerializeField]
    private NetworkPlayer heldByPlayer;
    [SerializeField]
    private float variation, width;
    [SerializeField]
    private CourtInfo courtInfo;

    private void OnTriggerEnter(Collider other) {
        if (!IsServer) return;
        if (other.TryGetComponent<NetworkBall>(out NetworkBall ball)) {
            if (ball.dead) return;
            ball.lastPlayerHit = heldByPlayer;

            Vector2 difference = ball.transform.position.XZ2() - transform.position.XZ2();
            float wideFactor = Mathf.Clamp(-Vector2.SignedAngle(Vector2.up, difference) / 90, -1, 1);

            if (ball.beingServed) {
                ball.ExitServeStateRpc();
            }

            Vector3 rand = courtInfo.GetRandomPositionOnSide(!heldByPlayer.controller.playerA);
            float x = ball.transform.position.x + width * wideFactor + Random.Range(-variation, variation);
            Vector3 destination = new (x, rand.y, rand.z);
            Vector3 intersection = courtInfo.NetIntersection(destination, ball.transform.position);
            float height = courtInfo.GetHeightFromAngle(transform.rotation.eulerAngles.x);

            ball.LaunchToPositionRpc(destination, intersection, height);
        }
    }
}