using UnityEngine.Networking;
using UnityEngine;
using Thieves.Client.PlayerNetworking;
using Thieves.Client.PlayerController;
using Thieves.Share.PlayerController;
using Thieves.GameServer.PlayerNetworking;

namespace Thieves.Share.PlayerNetworking {
		[NetworkSettings(channel=2)] public class NetworkedPlayer : NetworkBehaviour {
				public PlayerClient prefabInterpolated;
				public PlayerClient prefabPredicted;
				public PlayerServer prefabServer;

				public int inputBufferSize = 3;
				public float timeBetweenBullets = 0.15f;
				public int startingHealth = 100;

				[SyncVar]
				[HideInInspector]
				public string playerName;
				
				[SyncVar(hook="OnChangeMove")]
				public PlayerState move;

				[SyncVar(hook="OnChangeShoot")]
				public PlayerState shoot;

				[SyncVar(hook = "OnChangeHolster")]
				public PlayerState holster;

				[SyncVar]
				[HideInInspector]
				public int currentHealth;

				private PlayerServer server;
				private PlayerClient client;

				[ServerCallback] void Awake () {
						gameObject.AddComponent<PlayerHistory>();
						gameObject.AddComponent<PlayerHealth>();
						server = Instantiate(prefabServer, transform);
						currentHealth = startingHealth;
				}

				[ClientCallback] void Start () {
						client = Instantiate (isLocalPlayer ? prefabPredicted : prefabInterpolated, transform);
						//currentHealth = startingHealth;
				}

				[Command(channel=0)] public void CmdMove (PlayerInput[] inputs) {
						server.Move (inputs);
				}

				[ClientRpc] public void RpcUpdateHealth(int health) {
						currentHealth = health;
				}

				[ClientRpc] public void RpcResetPosition(Vector3 position) {
						client.transform.position = position;
				}

				[ClientRpc] public void RpcForceSwitchHolster() {
						PlayerSim sim = client.GetComponent<PlayerSim>();
						PlayerHolster holster = client.GetComponent<PlayerHolster>();

						if (sim != null) {
								sim.SetUnholster(!sim.GetUnholster());
						}

						holster.SwitchHolster(sim != null, sim == null);
				}

				void OnChangeMove (PlayerState move) {
						this.move = move;
						if (client == null) return;
						client.OnSnapshot (move, false, false);
				}

				void OnChangeShoot (PlayerState shoot) {
						this.shoot = shoot;
						if (client == null) return;
						client.OnSnapshot (shoot, true, false);
				}

				void OnChangeHolster(PlayerState holster) {
						this.holster = holster;
						if (client == null) return;
						client.OnSnapshot(holster, false, true);
				}

				public bool IsAuthoritativeServer() {
						return (server != null);
				}
		}
}
