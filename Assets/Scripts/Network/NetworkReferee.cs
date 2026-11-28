using System;
using System.Collections;
using NaughtyAttributes;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

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

    [SerializeField]
    private NetworkBall ballPrefab;
    private NetworkBall ballInPlay;
    private bool ballDied;

    [SerializeField]
    private int winScore, maxScore;
    private bool playerAServes;

    private WaitUntil allPlayersConnected;
    private WaitUntil untilBallDies;
    private WaitUntil untilBallDespawns;

    private void Awake() {
        instance = this;

        playerAClientId = new NetworkVariable<ulong>();
        playerBClientId = new NetworkVariable<ulong>();

        allPlayersConnected = new WaitUntil(() => playerA && playerB);
        untilBallDies = new WaitUntil(() => ballDied);
        untilBallDespawns = new WaitUntil(() => !ballInPlay);
    }

    public override void OnNetworkSpawn() {
        nm = NetworkManager.Singleton;
        playerAClientId.OnValueChanged += UpdatePlayerA;
        playerBClientId.OnValueChanged += UpdatePlayerB;

        if (IsServer) {
            StartCoroutine(GameLoop());
        }
    }

    private IEnumerator GameLoop() {
        yield return allPlayersConnected;
        playerAServes = Random.value < 0.5f;
        while (true) {
            Debug.Log($"{(playerAServes ? "Player A" : "Player B")} serves.");
            PutBallInPlay();
            NetworkPlayer startingPlayer = playerAServes ? playerA : playerB;
            ballInPlay.EnterServeStateRpc(startingPlayer.OwnerClientId);
        }
    }

    private void PutBallInPlay() {
        if (!IsServer) return;
        ballInPlay = Instantiate(ballPrefab, Vector3.up * 3f, Quaternion.identity);
        ballInPlay.NetworkObject.Spawn();
        ballDied = false;
        ballInPlay.BallDied += HandleBallDeath;
    }

    private void HandleBallDeath(NetworkPlayer lastPlayerHit, bool lastPlayerScored) {
        ballDied = true;
        ballInPlay.BallDied -= HandleBallDeath;

        if (lastPlayerHit == playerA && lastPlayerScored) {
            scoreA++;
        }
        else if (lastPlayerHit == playerB && !lastPlayerScored) {
            scoreA++;
        }
        else {
            scoreB++;
            playerAServes = false;
            Debug.Log("Player B scored");
            return;
        }

        playerAServes = true;
        Debug.Log("Player A scored");
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

    public bool GetNetworkPlayerByClientId(ulong clientId, out NetworkPlayer player) {
        player = null;
        if (clientId == playerAClientId.Value) player = playerA;
        if (clientId == playerBClientId.Value) player = playerB;
        return player;
    }

    #region Register Clients

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

    #endregion
}