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
    [HideInInspector]
    public Controller controller;

    private InputAction move;
    private InputAction dash;
    private InputAction swing;

    private void Awake() {
        // if (!IsOwner) return;
        controller = GetComponent<Controller>();
    }

    public override void OnNetworkSpawn() {
        playerCamera.gameObject.SetActive(false);
        GetComponent<MeshRenderer>().sharedMaterial = courtInfo.GetSideColor(OwnerClientId - 1);
        controller.SetASide(OwnerClientId == 1);

        if (IsOwner) {
            SetInitialPosition();
            playerCamera.transform.parent = null;
            courtInfo.SetCameraPosition(OwnerClientId, playerCamera);
            // ReSharper disable once Unity.PerformanceCriticalCodeCameraMain
            Camera.main.gameObject.SetActive(false);
            playerCamera.gameObject.SetActive(true);

            if (NetworkReferee.instance && IsSpawned) {
                if (OwnerClientId == 1) NetworkReferee.instance.RegisterPlayerARpc(OwnerClientId);
                if (OwnerClientId == 2) NetworkReferee.instance.RegisterPlayerBRpc(OwnerClientId);
            }
        }

        if (IsClient) {
            playerInput.enabled = true;
            move = playerInput.actions.FindAction("Move");
            dash = playerInput.actions.FindAction("Dash");
            swing = playerInput.actions.FindAction("Swing");

            move.performed += MoveCallHandle;
            move.canceled += MoveCallHandle;
            dash.performed += DashCallHandle;
            swing.performed += SwingCallHandle;

            if (OwnerClientId == 1) {
                playerInput.SwitchCurrentControlScheme(Keyboard.current, Mouse.current);
            }
            else if (OwnerClientId == 2) {
                playerInput.SwitchCurrentControlScheme(Gamepad.current);
            }
        }
    }

    private void Start() {
        if (IsOwner) {
            GetComponent<Rigidbody>().constraints =
                RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;

            if (NetworkReferee.instance && IsSpawned) {
                if (OwnerClientId == 1) NetworkReferee.instance.RegisterPlayerARpc(OwnerClientId);
                if (OwnerClientId == 2) NetworkReferee.instance.RegisterPlayerBRpc(OwnerClientId);
            }
        }
    }

    private void MoveCallHandle(InputAction.CallbackContext context) {
        Vector2 input = context.ReadValue<Vector2>();
        if (OwnerClientId == 1) input *= -1; // adjust for rotation of camera
        Move(input);
    }

    private void Move(Vector2 input) {
        if (IsClient) {
            MoveControllerClientRpc(input, /* NetworkManager.Singleton.LocalClientId,*/ NetworkObjectId);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void MoveControllerClientRpc(Vector2 moveInput, ulong noid) {
        if (NetworkObjectId == noid) {
            controller.Move(moveInput);
        }
    }

    private void DashCallHandle(InputAction.CallbackContext context) {
        Dash();
    }

    private void Dash() {
        if (IsClient) {
            DashClientRpc(NetworkObjectId);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void DashClientRpc(ulong noid) {
        if (NetworkObjectId == noid) {
            controller.Dash();
        }
    }

    private void SwingCallHandle(InputAction.CallbackContext context) {
        Swing();
    }

    private void Swing() {
        if (IsClient) {
            SwingClientRpc(NetworkObjectId);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SwingClientRpc(ulong noid) {
        if (NetworkObjectId == noid) {
            controller.Swing();
        }
    }

    private void SetInitialPosition() {
        if (OwnerClientId == 1) transform.eulerAngles = new Vector3(0f, 180f, 0f);
        transform.position = courtInfo.GetSideStartPosition(OwnerClientId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetPositionRpc(ulong noid, Vector3 position) {
        if (NetworkObjectId == noid) {
            transform.position = position;
        }
    }
}