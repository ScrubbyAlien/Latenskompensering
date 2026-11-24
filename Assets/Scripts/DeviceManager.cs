using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

[RequireComponent(typeof(NetworkObject))]
public class DeviceManager : MonoBehaviour
{
    public static DeviceManager instance;
    private HashSet<InputDevice> availableDevices;
    private HashSet<InputDevice> usedDevices;

    private void Start() {
        if (instance) {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}