using System;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class NetworkPlayer : NetworkBehaviour
{
    private PlayerController playerController;

    private void Awake() {
        playerController = GetComponent<PlayerController>();
    }
    
    
    
}
