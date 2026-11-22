using Unity.Netcode;
using UnityEngine;

public class PlayerTest : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new ();

    public override void OnNetworkSpawn() {
        if (IsOwner) {
            Move();
        }
    }

    public void Move() {
        SubmitPositionRequestRpc();
    }

    [Rpc(SendTo.Server)]
    private void SubmitPositionRequestRpc(RpcParams rpcParams = default) {
        Vector3 randomPosition = GetRandomPositionOnPlane();
        transform.position = randomPosition;
        Position.Value = randomPosition;
    }

    private static Vector3 GetRandomPositionOnPlane() {
        return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
    }

    private void Update() {
        transform.position = Position.Value;
    }
}