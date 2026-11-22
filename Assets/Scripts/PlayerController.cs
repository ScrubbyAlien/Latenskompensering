using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Controller
{
    [SerializeField]
    private PlayerInput playerInput;

    private InputAction move;
    private InputAction dash;
    private InputAction swing;

    private void Start() {
        foreach (InputDevice inputDevice in InputSystem.devices) {
            Debug.Log(inputDevice.description);
        }
        AssignActions();
    }

    // public void AssignDevice(InputDevice newDevice) {
    //     AssignActions();
    // }

    private void AssignActions() {
        move = playerInput.actions.FindAction("Move");
        dash = playerInput.actions.FindAction("Dash");
        swing = playerInput.actions.FindAction("Swing");
    }

    protected override void Update() {
        base.Update();
        Move(move.ReadValue<Vector2>().normalized);
        if (dash.WasPerformedThisFrame()) Dash();
        if (swing.WasPressedThisFrame()) Swing();
    }

    public void Test(string testString) {
        Debug.Log(testString);
    }
}