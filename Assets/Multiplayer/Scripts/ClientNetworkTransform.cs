using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

[DisallowMultipleComponent]
public class ClientNetworkTransform : NetworkTransform
{
    //ovverride this function to send the transform from client to the host
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
