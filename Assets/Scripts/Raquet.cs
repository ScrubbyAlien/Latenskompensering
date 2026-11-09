using System;
using UnityEngine;

public class Raquet : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent<Ball>(out Ball ball)) {
            Destroy(ball.gameObject);
        }
    }
}
