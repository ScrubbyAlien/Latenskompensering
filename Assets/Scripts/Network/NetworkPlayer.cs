using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Controller))]
public class NetworkPlayer : NetworkBehaviour
{
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
        if (!IsClient) return;
        playerInput.enabled = true;
        // move = playerInput.actions.FindAction("Move");
        // dash = playerInput.actions.FindAction("Dash");
        // swing = playerInput.actions.FindAction("Swing");

        playerInput.actions.FindAction("Move").performed += MoveCallHandle;
        playerInput.actions.FindAction("Move").canceled += MoveCallHandle;

        if (NetworkObjectId == 1) {
            playerInput.SwitchCurrentControlScheme(Keyboard.current, Mouse.current);
        }
        else if (NetworkObjectId == 2) {
            playerInput.SwitchCurrentControlScheme(Gamepad.current);
        }
    }

    private void MoveCallHandle(InputAction.CallbackContext context) {
        Move(context.ReadValue<Vector2>());
    }

    // private void Update() {
    //     
    //     if (IsClient && OwnerClientId != 2) {
    //         Move(move.ReadValue<Vector2>());
    //     }
    // }

    private void Move(Vector2 input) {
        MoveServerRpc(input, NetworkManager.Singleton.LocalClientId, NetworkObjectId);
    }

    [Rpc(SendTo.Server)]
    public void MoveServerRpc(Vector2 moveInput, ulong clientId, ulong noid) {
        Controller clientController = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(noid)
                                                    .GetComponent<Controller>();
        clientController.Move(moveInput);
    }
}