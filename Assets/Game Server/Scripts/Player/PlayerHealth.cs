using UnityEngine;
using Thieves.Share.PlayerNetworking;

namespace Thieves.GameServer.PlayerNetworking {

    // server side
    public class PlayerHealth : MonoBehaviour {
        int health;
        NetworkedPlayer player;

        void Awake() {
            player = GetComponent<NetworkedPlayer>();
            health = player.startingHealth;
        }

        public void TakeDamage(int amount) {
            if (health <= 0) return;

            health -= amount;
            player.currentHealth = health;

            if (health <= 0) {
                Debug.Log("[Server] Player " + player.name + " is dead.");
                return;
            }
        }

        public void ResetHealth() {
            health = player.startingHealth;
            player.currentHealth = health;
        }
    }
}
