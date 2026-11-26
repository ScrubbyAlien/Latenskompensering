using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
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

        if (IsOwner) {
            SetPosition();
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

    private void Move(Vector2 input) {
        if (IsClient) {
            MoveClientControllerRpc(input, /* NetworkManager.Singleton.LocalClientId,*/ NetworkObjectId);
        }
    }
    //
    // [Rpc(SendTo.Server)]
    // public void MoveServerRpc(Vector2 moveInput, /*ulong clientId,*/ ulong noid) {
    //     MoveClientControllerRpc( moveInput, noid);
    // }

    [Rpc(SendTo.ClientsAndHost)]
    public void MoveClientControllerRpc( Vector2 moveInput, ulong noid) {
        if (NetworkObjectId == noid) {
            controller.Move(moveInput);
        }
    }

    private void SetPosition() {
        transform.position = courtInfo.GetSideStartPosition(NetworkObjectId);
    }
}