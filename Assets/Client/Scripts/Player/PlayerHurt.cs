using UnityEngine;
using UnityEngine.UI;
using Thieves.Share.PlayerNetworking;
using Thieves.Client.Manager;

/// <summary>
///  Client-side perspective when a player suffers damage
///  this component just have the visual effects for the client
///  when a player deals damage to others or when suffers damage from others.
/// </summary>
namespace Thieves.Client.PlayerController {
    public class PlayerHurt : MonoBehaviour {

        private NetworkedPlayer networkedClient;
        private CameraManager camManagement;

        private void Awake() {
            networkedClient = GetComponentInParent<NetworkedPlayer>();

            if (networkedClient.isLocalPlayer) {
                camManagement = FindObjectOfType<CameraManager>();
            }
        }

        public void TakeDamage(Vector3 hitPoint) {
            if (networkedClient.isLocalPlayer) {
                camManagement.HurtShake();
            }
        }
    }
}
