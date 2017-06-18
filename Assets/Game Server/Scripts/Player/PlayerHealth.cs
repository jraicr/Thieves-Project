using UnityEngine;
using Thieves.Share.PlayerNetworking;
using Thieves.Share.PlayerController;

namespace Thieves.GameServer.PlayerNetworking {

    // server side
    public class PlayerHealth : MonoBehaviour {
        int health;
        NetworkedPlayer player;
				PlayerSim sim;

				void Start() {
						player = GetComponent<NetworkedPlayer>();
						sim = GetComponentInChildren<PlayerSim>();
						health = player.startingHealth;
				}

				public void TakeDamage(int amount) {
						if (health <= 0) return;

						DealDamage(amount);

						if (health <= 0) {
								Debug.Log("[Server] Player " + player.name + " is dead.");
								return;
						}
				}

        public void ResetHealth() {
            health = player.startingHealth;
        }

				private void DealDamage(int amount) {
						health -= amount;
						sim.ServerUpdateHealth(health, Time.fixedTime - Time.fixedDeltaTime);
						player.health = sim.state;
				}
    }
}
