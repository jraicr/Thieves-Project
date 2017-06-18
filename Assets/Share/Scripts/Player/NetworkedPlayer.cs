using UnityEngine.Networking;
using UnityEngine;
using Thieves.Client.PlayerNetworking;
using Thieves.Client.PlayerController;
using Thieves.Client.UI;
using Thieves.Share.PlayerController;
using Thieves.GameServer.PlayerNetworking;

namespace Thieves.Share.PlayerNetworking {
		[NetworkSettings(channel = 2)]
		public class NetworkedPlayer : NetworkBehaviour {
				/// <summary>
				/// Prefab for remote player.
				/// Instantiated by clients.
				/// </summary>
				public PlayerClient prefabInterpolated;

				/// <summary>
				/// Prefab for local player object which input would be predicted by client.
				/// Instantiated only by the local client
				/// </summary>
				public PlayerClient prefabPredicted;

				/// <summary>
				/// Prefab for the server player object. 
				/// Instantiated only by the server.
				/// </summary>
				public PlayerServer prefabServer;

				/// <summary>
				/// Input buffer size needed to start sending input data to the server
				/// </summary>
				public int inputBufferSize = 3;

				/// <summary>
				/// The time that must pass before player can shoot again.
				/// </summary>
				public float timeBetweenBullets = 0.15f;

				/// <summary>
				/// Initial value for health
				/// </summary>
				public int startingHealth = 100;

				/// <summary>
				/// The nickname of this player
				/// </summary>
				[SyncVar]
				[HideInInspector]
				public string playerName;

				[SyncVar(hook = "OnChangeMove")]
				public PlayerState move;

				[SyncVar(hook = "OnChangeShoot")]
				public PlayerState shoot;

				[SyncVar(hook = "OnChangeHealth")]
				public PlayerState health;
				public delegate void HealthUpdate(int hitpoints);
				public event HealthUpdate OnHealthUpdated;

				private PlayerServer server;
				private PlayerClient client;

				// This Awake() function only is called on game server.
				[ServerCallback]
				void Awake() {
						gameObject.AddComponent<PlayerHistory>();
						gameObject.AddComponent<PlayerHealth>();
						server = Instantiate(prefabServer, transform);
				}

				// This Start() function only is called on clients.
				[ClientCallback]
				void Start() {
						client = Instantiate(isLocalPlayer ? prefabPredicted : prefabInterpolated, transform);

						if (isLocalPlayer) {
								FindObjectOfType<ThievesMainMenuUI>().GetComponentInChildren<PlayerHUD>().enabled = true;
						}
				}

				/// <summary>
				/// Send input data to the server.
				/// This is called from client and invoked on server.
				/// </summary>
				/// <param name="inputs">Array with player data input</param>
				[Command(channel = 0)]
				public void CmdMove(PlayerInput[] inputs) {
						server.Move(inputs);
				}

				void OnChangeMove(PlayerState move) {
						this.move = move;
						if (client == null) return;
						client.OnSnapshot(move, false);
				}

				void OnChangeShoot(PlayerState shoot) {
						this.shoot = shoot;
						if (client == null) return;
						client.OnSnapshot(shoot, true);
				}

				void OnChangeHealth(PlayerState health) {
						Debug.Log("Health Changed. New Health: " + health.hitpoints);
						this.health = health;
						if (client == null) return;
						client.OnSnapshot(health, false);

						if (OnHealthUpdated != null) {
								OnHealthUpdated(health.hitpoints);
						}
				}
		}
}
