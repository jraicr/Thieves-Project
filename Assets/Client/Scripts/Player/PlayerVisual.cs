using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Thieves.Share.PlayerNetworking;

public class PlayerVisual : MonoBehaviour {

    NetworkedPlayer networkedPlayer;
    Color orange, blue;
    Renderer playerRender;
    TextMesh playerTextMesh;
    MeshRenderer labelNameRender;

    void Awake() {
        networkedPlayer = GetComponentInParent<NetworkedPlayer>();
        orange = new Color(1, 0.53f, 0.26f, 1);
        blue = new Color(0f, 0.56f, 1f, 1);

        playerRender = GetComponent<Renderer>();

    }
}
