using UnityEngine;
using UnityEngine.Networking;
using Thieves.GameServer.Profiles;
using Barebones.MasterServer;
using Thieves.GameServer.PlayerNetworking;

/// <summary>
/// Manages <see cref="UnetGameRoom"/>
/// </summary>
namespace Thieves.Share.Room {
		public class ThievesNetworkManager : NetworkManager {
				public Thieves.Share.Room.UnetGameRoom gameRoom;

				void Awake() {
						if (gameRoom == null) {
								Debug.LogError("Game Room property is not set on NetworkManager");
								return;
						}

						// Subscribe to events
						gameRoom.playerJoined += OnPlayerJoined;
						gameRoom.playerLeft += OnPlayerLeft;
				}

				/// <summary>
				/// Regular Unet method, which get's called when client disconnects from game server
				/// </summary>
				/// <param name="conn"></param>
				public override void OnServerDisconnect(NetworkConnection conn) {
						base.OnServerDisconnect(conn);

						// Don't forget to notify the room that a player disconnected
						gameRoom.ClientDisconnected(conn);
				}

				/// <summary>
				/// Invoked, when client provides a successful access token and enters the room
				/// </summary>
				/// <param name="player"></param>
				private void OnPlayerJoined(UnetMsfPlayer player) {
						// -----------------------------------
						// IF all you want to do is spawn a player:
						//
						// MiniNetworkManager.SpawnPlayer(player.Connection, player.Username, "carrot");
						// return;

						// -----------------------------------
						// If you want to use player profile

						// Create an "empty" (default) player profile
						var defaultProfile = PlayerProfiles.CreateProfileInServer(player.Username);

						// Get coins property from profile
						//var coinsProperty = defaultProfile.GetProperty<ObservableInt>(PlayerProfiles.coinsKey);

						// Fill the profile with values from master server
						Msf.Server.Profiles.FillProfileValues(defaultProfile, (successful, error) => {
								if (!successful) {
										Logs.Error("Failed to retrieve profile values: " + error);
								}

								// We can still allow players to play with default profile ^_^

								// Let's spawn the player character
								var playerObject = ServerNetworkManager.SpawnPlayer(player.Connection, player.Username);

								// Set coins value from profile
								//playerObject.Coins = coinsProperty.Value;

								//playerObject.CoinsChanged += () => {
								// Every time player coins change, update the profile with new value
								//coinsProperty.Set(playerObject.Coins);
								//};
						});
				}

				private void OnPlayerLeft(UnetMsfPlayer player) {

				}
		}
}