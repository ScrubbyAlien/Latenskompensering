using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Controller))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private PlayerInput playerInput;
    private Controller controller;

    private InputAction move;
    private InputAction dash;
    private InputAction swing;

    private void Awake() {
        controller = GetComponent<Controller>();
    }

    public void Start() {
        move = playerInput.actions.FindAction("Move");
        dash = playerInput.actions.FindAction("Dash");
        swing = playerInput.actions.FindAction("Swing");
    }

    private void Update() {
        controller.Move(move.ReadValue<Vector2>().normalized);
        if (dash.WasPerformedThisFrame()) controller.Dash();
        if (swing.WasPressedThisFrame()) controller.Swing();
    }
}