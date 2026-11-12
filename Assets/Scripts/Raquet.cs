using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Raquet : MonoBehaviour
{
    [SerializeField]
    private Controller heldByPlayer;
    [SerializeField]
    private float variation, width;
    [SerializeField]
    private CourtInfo courtInfo;

    
    
    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent<Ball>(out Ball ball)) {
            if (ball.dead) return;
            ball.lastPlayerHit = heldByPlayer;

            Vector2 difference = ball.transform.position.XZ2() - transform.position.XZ2();
            float wideFactor = Mathf.Clamp(Vector2.SignedAngle(Vector2.up, difference) / 90, -1, 1);
            
            
            if (ball.beingServed) {
                ball.ExitServeState();
            }
            
            
            Vector3 rand = courtInfo.GetRandomPositionOnSide(!heldByPlayer.playerA);
            float x = ball.transform.position.x + width * wideFactor + Random.Range(-variation, variation);
            Vector3 destination = new Vector3(x, rand.y, rand.z);            
            Vector3 intersection = courtInfo.NetIntersection(destination, ball.transform.position);
            float height = courtInfo.GetHeightFromAngle(transform.rotation.eulerAngles.x);
            
            ball.LaunchToPosition(destination, intersection, height);
        }
    }

    
}
