using Unity.Netcode;
using UnityEngine;

public class ManagerTest : MonoBehaviour
{
    private static NetworkManager networkManager;

    private void Awake() {
        networkManager = NetworkManager.Singleton;
    }

    private void OnGUI() {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!networkManager.IsClient && !networkManager.IsServer) {
            StartButtons();
        }
        else {
            StatusLabels();

            SubmitNewPosition();
        }

        GUILayout.EndArea();
    }

    private static void StartButtons() {
        if (GUILayout.Button("Host")) networkManager.StartHost();
        if (GUILayout.Button("Client")) networkManager.StartClient();
        if (GUILayout.Button("Server")) networkManager.StartServer();
    }

    private static void StatusLabels() {
        string mode = networkManager.IsHost ? "Host" : networkManager.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
                        networkManager.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }

    private static void SubmitNewPosition() {
        if (GUILayout.Button(networkManager.IsServer ?  "Move" : "Request Position Change")) {
            if (networkManager.IsServer && !networkManager.IsClient ) {
                foreach (ulong uid in networkManager.ConnectedClientsIds) {
                    networkManager.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<PlayerTest>()
                                  .Move();
                }
            }
            else {
                NetworkObject playerObject = networkManager.SpawnManager.GetLocalPlayerObject();
                PlayerTest player = playerObject.GetComponent<PlayerTest>();
                player.Move();
            }
        }
    }

    // private static void DebugInfo() {
    //     if (networkManager.Is)
    // }
}