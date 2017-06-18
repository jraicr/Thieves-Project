using System.Collections.Generic;
using System.Linq;
using Barebones.MasterServer;
using UnityEngine;
using UnityEngine.Networking;
using Thieves.Share.PlayerNetworking;

namespace Thieves.GameServer.PlayerNetworking {

		public class ServerNetworkManager : NetworkManager {
				public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
						SpawnPlayer(conn, "Player");
				}

				/// <summary>
				/// Spawns a character for connected client, and assigns it to connection
				/// </summary>
				/// <returns></returns>
				public static NetworkedPlayer SpawnPlayer(NetworkConnection connection, string playerName, Transform trnform = null) {
						Vector3 spawnPos = Vector3.zero;
						Vector2 facingDown = Vector2.down;

						// Create an instance
						var player = Instantiate(Resources.Load<NetworkedPlayer>("Prefabs/Networked Player"));
						var server = player.GetComponentInChildren<PlayerServer>();

						if (trnform == null) {
								// Move to position
								Spawn(player, server, spawnPos, facingDown, player.startingHealth);
						} else {
								Spawn(player, server, trnform.position, facingDown, player.startingHealth);
						}

						NetworkServer.AddPlayerForConnection(connection, player.gameObject, 0);
						player.playerName = playerName;
						return player;
				}

				public void DisconnectAllPlayers() {
						foreach (var player in FindObjectsOfType<NetworkedPlayer>()) {
								player.connectionToClient.Disconnect();
						}
				}

				private static void Spawn(NetworkedPlayer player, PlayerServer server, Vector3 spawnPosition, Vector2 facing, int startingHealth) {
						player.transform.position = spawnPosition;
						server.SetInitialState(spawnPosition, facing, player.startingHealth);
				}
		}
}