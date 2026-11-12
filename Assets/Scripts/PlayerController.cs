using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Controller
{

    private InputAction move;
    private InputAction dash;
    private InputAction swing;
    
    private void Start() {
        move = InputSystem.actions.FindAction("Move");
        dash = InputSystem.actions.FindAction("Dash");
        swing = InputSystem.actions.FindAction("Swing");
    }

    protected override void Update() {
        base.Update();
        Move(move.ReadValue<Vector2>().normalized);
        if (dash.WasPerformedThisFrame()) Dash();
        if (swing.WasPressedThisFrame()) Swing();
    }
}