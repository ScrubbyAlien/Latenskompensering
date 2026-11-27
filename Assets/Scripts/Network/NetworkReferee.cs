using System;
using NaughtyAttributes;
using Unity.Netcode;
using UnityEngine;

public class NetworkReferee : NetworkBehaviour
{
    public static NetworkReferee instance;

    private NetworkManager nm;

    private NetworkVariable<ulong> playerAClientId, playerBClientId;
    private NetworkPlayer playerA, playerB;
    private ulong playerAnoid => playerA.NetworkObjectId;
    private ulong playerBnoid => playerB.NetworkObjectId;
    private int scoreA, scoreB;

    [SerializeField]
    private CourtInfo courtInfo;

    private void Awake() {
        playerAClientId = new NetworkVariable<ulong>();
        playerBClientId = new NetworkVariable<ulong>();

        instance = this;
    }

    public override void OnNetworkSpawn() {
        if (IsServer) {
            nm = NetworkManager.Singleton;

            playerAClientId.OnValueChanged += UpdatePlayerA;
            playerBClientId.OnValueChanged += UpdatePlayerB;
        }
    }

    public void ResetPlayers() {
        if (IsServer) {
            ResetPlayer(playerA);
            ResetPlayer(playerB);
        }
    }

    private void ResetPlayer(NetworkPlayer player) {
        if (!IsServer || !player) return;
        player.SetPositionRpc(player.NetworkObjectId, courtInfo.GetSideStartPosition(player.OwnerClientId));
    }

    private void UpdatePlayerA(ulong oldId, ulong newId) {
        playerA = nm.SpawnManager.GetPlayerNetworkObject(newId).GetComponent<NetworkPlayer>();
    }

    private void UpdatePlayerB(ulong oldId, ulong newId) {
        playerB = nm.SpawnManager.GetPlayerNetworkObject(newId).GetComponent<NetworkPlayer>();
    }

    [Rpc(SendTo.Server)]
    public void RegisterPlayerARpc(ulong clientId) {
        playerAClientId.Value = clientId;
        AssertCorrectClientIds(clientId, playerA.OwnerClientId);
        Debug.Log($"Registered Player A from client {playerAClientId.Value}");
    }

    [Rpc(SendTo.Server)]
    public void RegisterPlayerBRpc(ulong clientId) {
        playerBClientId.Value = clientId;
        AssertCorrectClientIds(clientId, playerB.OwnerClientId);
        Debug.Log($"Registered Player B from client {playerBClientId.Value}");
    }

    private void AssertCorrectClientIds(ulong id1, ulong id2) {
        Debug.Assert(id1 == id2, "non matching client ids");
    }
}