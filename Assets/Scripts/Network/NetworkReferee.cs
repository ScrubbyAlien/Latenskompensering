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

        if (IsServer) {
            playerAClientId.OnValueChanged += UpdatePlayerA;
            playerBClientId.OnValueChanged += UpdatePlayerB;
        }

        instance = this;
        nm = NetworkManager.Singleton;
    }

    [Button]
    public void ResetPlayers() {
        if (IsServer) {
            ResetPlayer(playerA);
            ResetPlayer(playerB);
        }
    }

    private void ResetPlayer(NetworkPlayer player) {
        if (!IsServer) return;
        ulong noid = player.NetworkObjectId;
        player.SetPositionRpc(noid, courtInfo.GetSideStartPosition(noid));
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
        Debug.LogAssertion($"Correct id: {clientId == playerA.OwnerClientId}");
        Debug.Log($"Registered Player A from client {playerAClientId.Value}");
    }

    [Rpc(SendTo.Server)]
    public void RegisterPlayerBRpc(ulong clientId) {
        playerBClientId.Value = clientId;
        Debug.LogAssertion($"Correct id: {clientId == playerB.OwnerClientId}");
        Debug.Log($"Registered Player B from client {playerBClientId.Value}");
    }
}