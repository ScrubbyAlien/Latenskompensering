using System;
using UnityEngine;

public class Raquet : MonoBehaviour
{
    [SerializeField]
    private bool heldByASidePlayer;
    [SerializeField]
    private CourtInfo courtInfo;
    
    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent<Ball>(out Ball ball)) {
            if (ball.dead) return;
            Vector3 destination = courtInfo.GetRandomPositionOnSide(!heldByASidePlayer);
            Vector3 intersection = courtInfo.NetIntersection(destination, ball.transform.position);
            float height = courtInfo.GetRandomHeight();
            
            ball.LaunchToPosition(destination, intersection, height);
        }
    }
}
