using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Controller))]
public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField]
    private CourtInfo courtInfo;
    [SerializeField]
    private Camera playerCamera;

    // https://discussions.unity.com/t/how-to-use-input-system-together-with-netcode-for-gameobjects/260546
    [SerializeField]
    private PlayerInput playerInput;
    private Controller controller;

    private InputAction move;
    private InputAction dash;
    private InputAction swing;

    private void Awake() {
        // if (!IsOwner) return;
        controller = GetComponent<Controller>();
    }

    public override void OnNetworkSpawn() {
        playerCamera.gameObject.SetActive(false);
        GetComponent<MeshRenderer>().sharedMaterial = courtInfo.GetSideColor(NetworkObjectId - 1);

        if (IsServer) {
            SetPositionRpc();
        }

        if (IsOwner) {
            playerCamera.transform.parent = null;
            courtInfo.SetCameraPosition(NetworkObjectId, playerCamera);
            // ReSharper disable once Unity.PerformanceCriticalCodeCameraMain
            Camera.main.gameObject.SetActive(false);
            playerCamera.gameObject.SetActive(true);
        }

        if (IsClient) {
            playerInput.enabled = true;
            move = playerInput.actions.FindAction("Move");
            // dash = playerInput.actions.FindAction("Dash");
            // swing = playerInput.actions.FindAction("Swing");

            move.performed += MoveCallHandle;
            move.canceled += MoveCallHandle;

            if (NetworkObjectId == 1) {
                playerInput.SwitchCurrentControlScheme(Keyboard.current, Mouse.current);
            }
            else if (NetworkObjectId == 2) {
                playerInput.SwitchCurrentControlScheme(Gamepad.current);
            }
        }
    }

    private void MoveCallHandle(InputAction.CallbackContext context) {
        Vector2 input = context.ReadValue<Vector2>();
        if (NetworkObjectId == 1) input *= -1; // adjust for rotation of camera
        Move(input);
    }

    private void Update() {
        Debug.Log(transform.position.y);
    }

    private void Move(Vector2 input) {
        if (IsClient) {
            MoveServerRpc(input, NetworkManager.Singleton.LocalClientId, NetworkObjectId);
        }
    }

    [Rpc(SendTo.Server)]
    public void MoveServerRpc(Vector2 moveInput, ulong clientId, ulong noid) {
        Controller clientController = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(noid)
                                                    .GetComponent<Controller>();
        clientController.Move(moveInput);
    }

    // [Rpc(SendTo.ClientsAndHost)]
    private void SetPositionRpc() {
        transform.position = courtInfo.GetSideStartPosition(NetworkObjectId);
        SetClientPositionRpc(transform.position);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetClientPositionRpc(Vector3 position) {
        transform.position = position;
    }
}