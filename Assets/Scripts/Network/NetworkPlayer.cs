using System;
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
        if (!IsOwner) return;
        playerInput.enabled = true;
        move = playerInput.actions.FindAction("Move");
        dash = playerInput.actions.FindAction("Dash");
        swing = playerInput.actions.FindAction("Swing");

        if (NetworkObjectId == 1) {
            playerInput.SwitchCurrentControlScheme(Keyboard.current, Mouse.current);
        }
        else {
            playerInput.SwitchCurrentControlScheme(Gamepad.current);
        }
    }

    private void Update() {
        if (!IsOwner) return;
        Move(move.ReadValue<Vector2>());
    }

    private void Move(Vector2 input) {
        MoveServerRpc(input, OwnerClientId);
    }

    [Rpc(SendTo.Server)]
    public void MoveServerRpc(Vector2 moveInput, ulong clientId) {
        Controller clientController = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId)
                                                    .GetComponent<Controller>();
        clientController.Move(moveInput);
    }

    // [Rpc(SendTo.ClientsAndHost)]
    // public void MoveClientRpc(Vector2 moveInput) {
    //     if (IsOwner) {
    //         controller.Move(moveInput);
    //     }
    // }
}