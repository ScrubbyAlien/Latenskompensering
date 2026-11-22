using System;
using UnityEngine;
using Unity.Netcode;

public class TransformTest : NetworkBehaviour
{
    private void Update() {
        if (IsServer) {
            float theta = Time.frameCount / 10f;
        transform.position = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta));           
        }
    }
}