using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody body;
    private InputAction move;
    private InputAction dash;
    private float linearDampening;

    private bool inDash;
    private bool dashPerformed;
    private float nextDashTime;

    [SerializeField]
    private float acceleration;
    [SerializeField]
    private float dampening;
    [SerializeField]
    private float maxSpeed;

    [Header("Dash")]
    [SerializeField]
    private float dashAcceleration;
    [SerializeField]
    private float dashSpeed;
    [SerializeField]
    private float dashCooldown;

    private void Start() {
        body = GetComponent<Rigidbody>();
        linearDampening = body.linearDamping;
        move = InputSystem.actions.FindAction("Move");
        dash = InputSystem.actions.FindAction("Dash");
    }

    private void Update() {
        if (!inDash) {
            inDash = dash.WasPressedThisFrame();
            if (inDash) {
                nextDashTime = Time.time + dashCooldown;
                dashPerformed = true;
            }
        }
        else {
            inDash = Time.time < nextDashTime;
        }
    }

    private void FixedUpdate() {
        Vector2 moveValue = move.ReadValue<Vector2>().normalized;

        if (moveValue.x != 0f || moveValue.y != 0f) {
            Vector3 force = new Vector3(moveValue.x, 0, moveValue.y) * acceleration;
            body.AddForce(force, ForceMode.Acceleration);
        }
        else if (body.linearVelocity.sqrMagnitude > 0 && !inDash) {
            body.linearVelocity *= dampening;
        }

        float max = maxSpeed;
        if (inDash) {
            max = dashSpeed;
            if (dashPerformed) {
                dashPerformed = false;
                Vector3 dashForce = moveValue * dashAcceleration;
                body.AddForce(dashForce, ForceMode.Impulse);
            }
        }

        float speed = body.linearVelocity.magnitude;
        if (speed > max) {
            Vector3 backwards = -body.linearVelocity / speed;
            Vector3 correction = backwards * (speed - max);
            body.AddForce(correction, ForceMode.VelocityChange);
        }
    }
}