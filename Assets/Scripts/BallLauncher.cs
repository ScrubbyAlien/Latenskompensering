using System;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class BallLauncher : MonoBehaviour
{
    [SerializeField]
    private Ball ballPrefab;

    [SerializeField]
    private Bounds startingArea, targetArea;
    [SerializeField]
    private float timer, netHeight, ceilingHeight;
    private float nextSpawnTime;

    // [BoxGroup("Travel time")]
    // [SerializeField, Min(0.01f)]
    // private float min, max;

    private void OnValidate() {
        if (ceilingHeight < netHeight) ceilingHeight = netHeight;
        // if (max < min) max = min;
    }

    private void Start() {
        nextSpawnTime = Time.time + timer;
    }

    private void Update()
    {
        if (Time.time >= nextSpawnTime) {
            nextSpawnTime = Time.time + timer;
            
            Vector3 positionInBounds = RandomPositionInBounds(startingArea);
            Vector3 destination = RandomPositionInBounds(targetArea);
            Vector3 intersection = NetIntersection(positionInBounds, destination);
            
            
            Debug.DrawLine(positionInBounds, destination, Color.red, 10f);
            
            float height = Random.Range(netHeight, ceilingHeight) - positionInBounds.y;
            // float time = Random.Range(min, max);
            Debug.DrawLine(intersection, intersection + Vector3.up * height);
            
            Ball ball = Instantiate(ballPrefab, positionInBounds, Quaternion.identity);
            ball.LaunchToPosition(destination, intersection, height);
        }
    }

    private Vector3 NetIntersection(Vector3 a, Vector3 b) {
        float distanceToIntersection = a.z * Vector3.Distance(a, b) / a.z + b.z;
        return a + distanceToIntersection * (b - a).normalized;
    }

    [Button]
    private void Launch() {
        nextSpawnTime = 0;
    }

    private static Vector3 RandomPositionInBounds(Bounds bounds) {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.darkRed;
        Gizmos.DrawWireCube(startingArea.center, startingArea.size);
        Gizmos.color = Color.cornflowerBlue;
        Gizmos.DrawWireCube(targetArea.center, targetArea.size);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(new Vector3(-7.5f, netHeight, 0), new Vector3(7.5f, netHeight, 0));
        Gizmos.DrawLine(new Vector3(-7.5f, ceilingHeight, 0), new Vector3(7.5f, ceilingHeight, 0));
    }
}
