using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
public class NetworkPlayer : NetworkBehaviour
{
    private PlayerInput playerInput;
    // https://discussions.unity.com/t/how-to-use-input-system-together-with-netcode-for-gameobjects/260546
    private Controller controller;

    private void Awake() {
        controller = GetComponent<PlayerController>();
    }

    private void Move(InputAction.CallbackContext context) {
        MoveServerRpc(context.ReadValue<Vector2>(), NetworkObjectId);
    }

    [ServerRpc]
    public void MoveServerRpc(Vector2 moveInput, ulong networkObjectId) {
        MoveClientRpc(moveInput, networkObjectId);
    }

    [ClientRpc]
    public void MoveClientRpc(Vector2 moveInput, ulong id) {
        if (IsOwner && id == NetworkObjectId) {
            Debug.Log(moveInput);
            controller.Move(moveInput);
        }
    }
}