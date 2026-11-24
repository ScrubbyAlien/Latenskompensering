using System;
using UnityEngine;

[RequireComponent(typeof(Controller))]
public class AIController : MonoBehaviour
{
    [SerializeField]
    private Referee referee;
    [SerializeField]
    private CourtInfo courtInfo;
    private Controller controller;

    [Header("AI Parameters"), SerializeField]
    private float reactionSpeed;
    private float nextReactionTime;
    [SerializeField]
    private float swingWithinHeight;
    [SerializeField]
    private float swingCooldown;
    private float nextSwingTime;
    [SerializeField]
    private float dashLimit;
    [SerializeField]
    private float serveWaitTime;
    private float serveTime;
    private bool serving;

    private void Awake() {
        controller = GetComponent<Controller>();
        controller.OnGetServe += OnGetServe;
    }

    private void Update() {
        Vector3 target = Vector3.zero;
        Ball ball = referee.ballInPlay;

        bool noBall = !ball && ball.dead;
        bool returnBall = !noBall && ball.lastPlayerHit != this && !ball.servingPlayer;
        if (returnBall) {
            target = CalculateDestination(ball.velocity, ball.transform.position)
                     + controller.playerRaquet.transform.localPosition.XZ();
        }
        else {
            // when not moving to ball, move toward center
            nextReactionTime = Time.time + reactionSpeed;
            target = (controller.playerA ? courtInfo.ASide : courtInfo.BSide).center;
        }

        Vector2 difference = target.XZ2() - transform.position.XZ2();
        float distance = difference.magnitude;
        if (distance > 0.2f && (Time.time > nextReactionTime || !returnBall)) {
            controller.Move(difference.normalized);
        }
        else {
            controller.Move(Vector2.zero);
        }
        if (noBall) return;

        if (distance > dashLimit) controller.Dash();

        bool ballOnMyCourt = !((ball.transform.position.z > 0) ^ controller.playerA); // both false or both true
        bool serve = serving && Time.time >= serveTime;
        bool attemptReturn = returnBall && Time.time > nextSwingTime && ball.transform.position.y < swingWithinHeight;
        if (ballOnMyCourt && (serve || attemptReturn)) {
            controller.Swing();
            serving = false;
            nextSwingTime = Time.time + swingCooldown;
        }
    }

    private void OnGetServe() {
        serving = true;
        serveTime = Time.time + serveWaitTime;
    }

    private static Vector3 CalculateDestination(Vector3 initialVelocity, Vector3 initialPosition) {
        float vy = initialVelocity.y;
        float py = initialPosition.y;
        float g = -Physics.gravity.y;
        float remainingTime = (vy + Mathf.Sqrt(vy * vy + 2 * g * py)) / g;
        return initialPosition.XZ() + initialVelocity.XZ() * remainingTime;
    }
}