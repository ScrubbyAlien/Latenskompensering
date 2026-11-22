using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeviceManager : NetworkBehaviour
{
    public static DeviceManager instance;
    private HashSet<InputDevice> availableDevices;

    private void Start() {
        if (instance) {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // public bool RequestPlayerInput(out PlayerInput playerInput) {
    // }
}