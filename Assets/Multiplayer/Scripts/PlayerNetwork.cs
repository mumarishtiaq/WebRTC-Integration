using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{
    public float moveSpeed = 5f; // Movement speed

    void Update()
    {
        if (!IsOwner) return;
        float moveX = Input.GetAxis("Horizontal"); // A/D or Left/Right arrow keys
        float moveZ = Input.GetAxis("Vertical");   // W/S or Up/Down arrow keys

        Vector3 move = new Vector3(moveX, 0, moveZ);
        transform.Translate(-move * moveSpeed * Time.deltaTime);
    }
}
