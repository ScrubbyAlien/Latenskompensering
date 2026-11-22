using Unity.Netcode;
using UnityEngine;

public class RpcTest : NetworkBehaviour
{
    public override void OnNetworkSpawn() {
        LogNetworkObjectInfo();

        // Only send an RPC to the server from the client that owns the NetworkObject of this NetworkBehaviour instance
        if (!IsServer && IsOwner) {
            // ServerOnlyRpc(0, NetworkObjectId);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ClientAndHostRpc(int value, ulong sourceNetworkObjectId) {
        Debug.Log($"Client Received the RPC #{value} on NetworkObject #{sourceNetworkObjectId}");

        // Only send an RPC to the owner of the NetworkObject
        // More than one client might exist, only rpc the server if this client is the owner of the network object
        // a network object will only have one owner, who will be a client or host
        // this method would be executed for all combinations of clients and objects,
        // but only one combination will have the client be the owner of the object
        if (IsOwner) {
            ServerOnlyRpc(value + 1, sourceNetworkObjectId);
        }
    }

    [Rpc(SendTo.Server)]
    private void ServerOnlyRpc(int value, ulong sourceNetworkObjectId) {
        Debug.Log($"Server Received the RPC #{value} on NetworkObject #{sourceNetworkObjectId}");
        ClientAndHostRpc(value, sourceNetworkObjectId);
    }

    private void LogNetworkObjectInfo() {
        Debug.Log(
            $"Is server: {IsServer} | " +
            $"Is client: {IsClient} | " +
            $"Is owner: {IsOwner} | " +
            $"Is host: {IsHost} | " +
            $"id: {NetworkObjectId}"
        );
    }
}