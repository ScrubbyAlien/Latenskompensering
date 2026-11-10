using System;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class BallLauncher : MonoBehaviour
{
    [SerializeField]
    private Ball ballPrefab;
    [SerializeField]
    private CourtInfo courtInfo;

    [SerializeField]
    private float timer;
    private float nextSpawnTime;
    
    private void Start() {
        nextSpawnTime = Time.time + timer;
    }

    private void Update() {
        if (Time.time >= nextSpawnTime) {
            nextSpawnTime = Time.time + timer;

            Vector3 positionInBounds = courtInfo.GetRandomPositionOnSide(true);
            Vector3 destination = courtInfo.GetRandomPositionOnSide(false);
            Vector3 intersection = courtInfo.NetIntersection(positionInBounds, destination);
            
            Debug.DrawLine(positionInBounds, destination, Color.red, 10f);

            float height = courtInfo.GetRandomHeight() - positionInBounds.y;
            // Debug.DrawLine(intersection, intersection + Vector3.up * height);
            Ball ball = Instantiate(ballPrefab, positionInBounds, Quaternion.identity);
            ball.LaunchToPosition(destination, intersection, height);
        }
    }

    

    [Button]
    private void Launch() {
        nextSpawnTime = 0;
    }

}
