using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Thieves.Client.PlayerNetworking;
using Thieves.Share.PlayerNetworking;

namespace Thieves.Client.UI {
    public class PlayerHUD : MonoBehaviour {

        public Text hitpointText;
        public Text statsText;
        NetworkedPlayer player;
        NetworkManager netManager;

        private void Start() {
            player = FindObjectOfType<PlayerPredicted>().GetComponentInParent<NetworkedPlayer>();
            netManager = FindObjectOfType<NetworkManager>();
            hitpointText.text = "HP: " + player.health.hitpoints;

            player.OnHealthUpdated += HandleOnHealthUpdated;
        }

        private void OnDisable() {
            player.OnHealthUpdated -= HandleOnHealthUpdated;
        }

        void Update() {
            UpdateConnectionStats();
        }

        public void HandleOnHealthUpdated(int newHealth) {
            hitpointText.text = "HP: " + newHealth;
        }

        private void UpdateConnectionStats() {
#if UNITY_EDITOR
            foreach (KeyValuePair<short, NetworkConnection.PacketStat> stat in netManager.client.GetConnectionStats()) {
                if (stat.Key == 8) { // Updated stats for changed variables and synced
                    statsText.text = stat.Value.ToString() + "   RTT: " + netManager.client.GetRTT() + "ms.";
                    break;
                }
            }
#else
								statsText.text = "RTT: " + netManager.client.GetRTT() + "ms. ";
#endif
        }

    }
}