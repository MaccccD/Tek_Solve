using System.Collections.Generic;
using Mirror;
using UnityEngine;


public class CustomNetworkManagerScript : NetworkManager
{

    [Header("Auto‑Start Settings")]
    [Tooltip("How many seconds to wait for a host before starting one.")]
    public float clientConnectTimeout = 1.0f;

    public override void OnClientDisconnect()
    {
        if (!NetworkClient.isConnected)
            Debug.Log("[AutoStart] Disconnected as client.");
    }
}
