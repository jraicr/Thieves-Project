using System;
using System.Collections;
using System.Collections.Generic;
using Barebones.Logging;
using Barebones.MasterServer;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;


namespace Thieves.Share.Room {
		/// <summary>
		/// This script automatically creates a room in "master" server,
		/// when <see cref="OnStartServer"/> is called (most likely by Network Manager
		/// , when server is started).
		/// 
		/// After room is created, it also checks if this game server was "spawned", and 
		/// if so - it finalizes the spawn task
		/// </summary>
		public class UnetGameRoom : NetworkBehaviour {
				public static SpawnTaskController SpawnTaskController;

				/// <summary>
				/// Unet msg type 
				/// </summary>
				public static short accessMsgType = 3000;

				public HelpBox header = new HelpBox() {
						Text = "Waits for the Unet game server to start," +
									 "and then automatically creates a Room for it " +
									 "(registers server to 'Master').",
						Type = HelpBoxType.Info
				};

				[Header("General")]
				public LogLevel logLevel = LogLevel.Warn;

				[Header("Room options")]
				[Tooltip("This address will be sent to clients with an access token")]
				public string publicIp = "xxx.xxx.xxx.xxx";
				public string roomName = "Room Name";
				public int maxPlayers = 10;
				public bool isPublic = true;
				public string password = "";
				public bool allowUsersRequestAccess = true;

				[Header("Room properties")]
				public string mapName = "Amazing Map";

				[Header("Other")]
				public bool quitAppIfDisconnected = true;

				public BmLogger logger = Msf.Create.Logger(typeof(UnetGameRoom).Name);

				protected Dictionary<int, UnetMsfPlayer> playersByPeerId;
				protected Dictionary<string, UnetMsfPlayer> playersByUsername;
				protected Dictionary<int, UnetMsfPlayer> playersByConnectionId;

				public event Action<UnetMsfPlayer> playerJoined;
				public event Action<UnetMsfPlayer> playerLeft;

				public NetworkManager networkManager;

				public RoomController controller;

				protected virtual void Awake() {
						networkManager = networkManager ?? FindObjectOfType<NetworkManager>();

						logger.LogLevel = logLevel;

						playersByPeerId = new Dictionary<int, UnetMsfPlayer>();
						playersByUsername = new Dictionary<string, UnetMsfPlayer>();
						playersByConnectionId = new Dictionary<int, UnetMsfPlayer>();

						NetworkServer.RegisterHandler(accessMsgType, HandleReceivedAccess);

						Msf.Server.Rooms.Connection.Disconnected += OnDisconnectedFromMaster;
				}

				public bool IsRoomRegistered { get; protected set; }

				/// <summary>
				/// This will be called, when game server starts
				/// </summary>
				public override void OnStartServer() {
						// Find the manager, in case it was inaccessible on awake
						networkManager = networkManager ?? FindObjectOfType<NetworkManager>();

						// The Unet server is started, we need to register a Room
						BeforeRegisteringRoom();
						RegisterRoom();
				}

				/// <summary>
				/// This method is called before creating a room. It can be used to
				/// extract some parameters from cmd args or from span task properties
				/// </summary>
				protected virtual void BeforeRegisteringRoom() {
						if (SpawnTaskController != null) {
								logger.Debug("Reading spawn task properties to override some of the room options");

								// If this server was spawned, try to read some of the properties
								var prop = SpawnTaskController.Properties;

								// Room name
								if (prop.ContainsKey(MsfDictKeys.RoomName))
										roomName = prop[MsfDictKeys.RoomName];

								if (prop.ContainsKey(MsfDictKeys.MaxPlayers))
										maxPlayers = int.Parse(prop[MsfDictKeys.MaxPlayers]);

								if (prop.ContainsKey(MsfDictKeys.RoomPassword))
										password = prop[MsfDictKeys.RoomPassword];

								if (prop.ContainsKey(MsfDictKeys.MapName))
										mapName = prop[MsfDictKeys.MapName];
						}

						// Override the public address
						if (Msf.Args.IsProvided(Msf.Args.Names.MachineIp) && networkManager != null) {
								publicIp = Msf.Args.MachineIp;
								logger.Debug("Overriding rooms public IP address to: " + publicIp);
						}
				}

				public virtual void RegisterRoom() {
						var isUsingLobby = Msf.Args.IsProvided(Msf.Args.Names.LobbyId);

						var properties = SpawnTaskController != null
								? SpawnTaskController.Properties
								: new Dictionary<string, string>();

						if (!properties.ContainsKey(MsfDictKeys.MapName))
								properties[MsfDictKeys.MapName] = mapName;

						properties[MsfDictKeys.SceneName] = SceneManager.GetActiveScene().name;

						// 1. Create options object
						var options = new RoomOptions() {
								RoomIp = publicIp,
								RoomPort = networkManager.networkPort,
								Name = roomName,
								MaxPlayers = maxPlayers,

								// Lobby rooms should be private, because they are accessed differently
								IsPublic = isUsingLobby ? false : isPublic,
								AllowUsersRequestAccess = isUsingLobby ? false : allowUsersRequestAccess,

								Password = password,

								Properties = new Dictionary<string, string>()
								{
								{MsfDictKeys.MapName, mapName }, // Show the name of the map
                {MsfDictKeys.SceneName, SceneManager.GetActiveScene().name} // Add the scene name
            }
						};

						BeforeSendingRegistrationOptions(options, properties);

						// 2. Send a request to create a room
						Msf.Server.Rooms.RegisterRoom(options, (controller, error) => {
								if (controller == null) {
										logger.Error("Failed to create a room: " + error);
										return;
								}

								// Save the controller
								this.controller = controller;

								logger.Debug("Room Created successfully. Room ID: " + controller.RoomId);

								OnRoomRegistered(controller);
						});
				}

				/// <summary>
				/// Override this method, if you want to make some changes to registration options
				/// </summary>
				/// <param name="options">Room options, before sending them to register a room</param>
				/// <param name="spawnProperties">Properties, which were provided when spawning the process</param>
				protected virtual void BeforeSendingRegistrationOptions(RoomOptions options,
						Dictionary<string, string> spawnProperties) {
						// You can override this method, and modify room registration options

						// For example, you could copy some of the properties from spawn request,
						// like this:
						if (spawnProperties.ContainsKey("magicProperty"))
								options.Properties["magicProperty"] = spawnProperties["magicProperty"];
				}

				/// <summary>
				/// Called when room is registered to the "master server"
				/// </summary>
				/// <param name="roomController"></param>
				public void OnRoomRegistered(RoomController roomController) {
						IsRoomRegistered = true;

						// Set access provider (Optional)
						roomController.SetAccessProvider(CreateAccess);

						// If this room was spawned
						if (SpawnTaskController != null)
								SpawnTaskController.FinalizeTask(CreateSpawnFinalizationData());
				}

				/// <summary>
				/// Override, if you want to manually handle creation of access'es
				/// </summary>
				/// <param name="callback"></param>
				public virtual void CreateAccess(UsernameAndPeerIdPacket requester, RoomAccessProviderCallback callback) {
						callback.Invoke(new RoomAccessPacket() {
								RoomIp = controller.Options.RoomIp,
								RoomPort = controller.Options.RoomPort,
								Properties = controller.Options.Properties,
								RoomId = controller.RoomId,
								SceneName = SceneManager.GetActiveScene().name,
								Token = Guid.NewGuid().ToString()
						}, null);
				}

				/// <summary>
				/// This dictionary will be sent to "master server" when we want 
				/// notify "master" server that Spawn Process is completed
				/// </summary>
				/// <returns></returns>
				public virtual Dictionary<string, string> CreateSpawnFinalizationData() {
						return new Dictionary<string, string>()
						{
            // Add room id, so that whoever requested to spawn this game server,
            // knows which rooms access to request
            {MsfDictKeys.RoomId, controller.RoomId.ToString()},

            // Add room password, so that creator can request an access to a 
            // password-protected room
            {MsfDictKeys.RoomPassword, controller.Options.Password}
				};
				}

				/// <summary>
				/// This should be called when client leaves the game server.
				/// This method will remove player object from lookups
				/// </summary>
				/// <param name="connection"></param>
				public void ClientDisconnected(NetworkConnection connection) {
						UnetMsfPlayer player;
						playersByConnectionId.TryGetValue(connection.connectionId, out player);

						if (player == null)
								return;

						OnPlayerLeft(player);
				}

				protected virtual void HandleReceivedAccess(NetworkMessage netmsg) {
						var token = netmsg.ReadMessage<StringMessage>().value;

						controller.ValidateAccess(token, (validatedAccess, error) => {
								if (validatedAccess == null) {
										logger.Error("Failed to confirm access token:" + error);
										// Confirmation failed, disconnect the user
										netmsg.conn.Disconnect();
										return;
								}

								logger.Debug("Confirmed token access for peer: " + validatedAccess);

								// Get account info
								Msf.Server.Auth.GetPeerAccountInfo(validatedAccess.PeerId, (info, errorMsg) => {
										if (info == null) {
												logger.Error("Failed to get account info of peer " + validatedAccess.PeerId + "" +
																		 ". Error: " + errorMsg);
												return;
										}

										logger.Debug("Got peer account info: " + info);

										var player = new UnetMsfPlayer(netmsg.conn, info);

										OnPlayerJoined(player);
								});
						});
				}

				protected virtual void OnPlayerJoined(UnetMsfPlayer player) {
						// Add to lookups
						playersByPeerId[player.PeerId] = player;
						playersByUsername[player.Username] = player;
						playersByConnectionId[player.Connection.connectionId] = player;

						if (playerJoined != null)
								playerJoined.Invoke(player);
				}

				protected virtual void OnPlayerLeft(UnetMsfPlayer player) {
						// Remove from lookups
						playersByPeerId.Remove(player.PeerId);
						playersByUsername.Remove(player.Username);
						playersByConnectionId.Remove(player.Connection.connectionId);

						if (playerLeft != null)
								playerLeft.Invoke(player);

						// Notify controller that the player has left
						controller.PlayerLeft(player.PeerId);
				}

				private void OnDisconnectedFromMaster() {
						if (quitAppIfDisconnected)
								Application.Quit();
				}

				public UnetMsfPlayer GetPlayer(string username) {
						UnetMsfPlayer player;
						playersByUsername.TryGetValue(username, out player);
						return player;
				}

				public Dictionary<string, UnetMsfPlayer> GetPlayers() {
						return playersByUsername;
				}

				protected virtual void OnDestroy() {
						Msf.Server.Rooms.Connection.Disconnected -= OnDisconnectedFromMaster;
				}
		}
}
