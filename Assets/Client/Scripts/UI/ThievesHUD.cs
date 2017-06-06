using System.Collections.Generic;
using System.Linq;
using Barebones.Networking;
using UnityEngine;
using UnityEngine.UI;
using Barebones.MasterServer;

namespace Thieves.Client.UI {
		public class ThievesHUD : MonoBehaviour {
				protected const string HudExpansionPrefKey = "msf.hudExpanded";

				public bool fullViewByDefault = true;

				public bool isExpanded = true;

				[Header("Game objects")]
				public GameObject masterRunning;
				public GameObject spawners;
				public GameObject rooms;
				public GameObject masterConnection;
				public GameObject loginStatus;
				public GameObject disclaimer;

				protected IClientSocket connection;

				[Header("Connection Status")]
				public Text connectionStatusText;
				public Text connectionPermissionsText;
				public Image connectionStatusBg;

				[Header("Other Text Components")]
				public Text spawnersText;
				public Text roomsText;
				public Text versionText;
				public Text loginStatusText;

				public bool editorOnly = false;

				void Awake() {
						if (!Msf.Runtime.IsEditor && editorOnly) {
								gameObject.SetActive(false);
						}

						connection = Msf.Connection;
						isExpanded = PlayerPrefs.GetInt(HudExpansionPrefKey, 1) > 0;
				}

				void Start() {
						versionText.text = Application.version;

						// Subscribe
						connection.StatusChanged += OnConnectionStatusChanged;
						MasterServerBehaviour.MasterStarted += OnMasterStatusChanged;
						MasterServerBehaviour.MasterStopped += OnMasterStatusChanged;
						Msf.Server.Rooms.RoomRegistered += OnRoomCountChanged;
						Msf.Server.Rooms.RoomDestroyed += OnRoomCountChanged;
						Msf.Server.Spawners.SpawnerRegistered += OnSpawnersCountChange;
						Msf.Client.Auth.LoggedIn += OnLoginStatusChanged;
						Msf.Client.Auth.LoggedOut += OnLoginStatusChanged;

						// Update all views
						UpdateAllViews();
				}

				private void OnMasterStatusChanged(MasterServerBehaviour obj) {
						UpdateRunningMasterView(obj.IsRunning);
				}

				private void OnSpawnersCountChange(SpawnerController obj) {
						UpdateSpawnersView();
				}

				private void OnRoomCountChanged(RoomController obj) {
						UpdateRoomsView();
				}

				private void OnConnectionStatusChanged(ConnectionStatus status) {
						UpdateConnectionStatusView();
						masterConnection.gameObject.SetActive(isExpanded && (connection.Status != ConnectionStatus.Disconnected));
				}

				protected void OnLoginStatusChanged() {
						UpdateLoginStatusView(connection.Status);
				}

				public void ToggleFullWindow() {
						isExpanded = !isExpanded;
						UpdateAllViews();

						PlayerPrefs.SetInt(HudExpansionPrefKey, isExpanded ? 1 : 0);
				}

				private void UpdateAllViews() {
						UpdateRunningMasterView(MasterServerBehaviour.IsMasterRunning);
						UpdateConnectionStatusView();
						UpdateRoomsView();
						UpdateSpawnersView();
						disclaimer.SetActive(isExpanded);
						UpdateLoginStatusView(connection.Status);
				}


				private void UpdateConnectionStatusView() {
						switch (connection.Status) {
								case ConnectionStatus.Connected:
										connectionStatusText.text = "Connected To Master";
										// Update login view to **show login** info
										UpdateLoginStatusView(connection.Status);
										break;
								case ConnectionStatus.Connecting:
										connectionStatusText.text = "Connecting...";
										connectionPermissionsText.text = "To " + connection.ConnectionIp + ":" + connection.ConnectionPort;
										break;
								case ConnectionStatus.Disconnected:
										connectionStatusText.text = "Not Connected";
										// Update login view to **hide login** info
										UpdateLoginStatusView(connection.Status);
										break;
						}

						if (connection.Status == ConnectionStatus.Connected) {
								connectionPermissionsText.text = "Permission Level: " + Msf.Security.CurrentPermissionLevel;
						}

						masterConnection.SetActive(isExpanded && (connection.Status != ConnectionStatus.Disconnected));
				}

				private void UpdateLoginStatusView(ConnectionStatus status) {
						if (status == ConnectionStatus.Connected) {
								if (Msf.Client.Auth.IsLoggedIn) {
										var username = Msf.Client.Auth.AccountInfo.Username;
										loginStatus.SetActive(true);
										loginStatusText.text = "Logged in as <color=#FFC208FF>" + username + "</color>";
								} else {
										loginStatusText.text = "You are not logged in.";
								}

								loginStatus.SetActive(isExpanded);

						} else {
								loginStatus.SetActive(false);
						}
				}

				private void UpdateRunningMasterView(bool isRunning) {
						masterRunning.gameObject.SetActive(isRunning && isExpanded);
				}

				private void UpdateRoomsView() {
						var rooms = Msf.Server.Rooms.GetLocallyCreatedRooms().ToList();
						this.rooms.SetActive(rooms.Count > 0 && isExpanded);
						roomsText.text = "Created Rooms: " + rooms.Count;
				}

				private void UpdateSpawnersView() {
						var spawners = Msf.Server.Spawners.GetLocallyCreatedSpawners().ToList();
						this.spawners.SetActive(spawners.Count > 0 && isExpanded);
						spawnersText.text = "Started Spawners: " + spawners.Count;
				}

				void OnDestroy() {
						// Unsubscribe
						connection.StatusChanged -= OnConnectionStatusChanged;
						MasterServerBehaviour.MasterStarted -= OnMasterStatusChanged;
						MasterServerBehaviour.MasterStopped -= OnMasterStatusChanged;
						Msf.Server.Rooms.RoomRegistered -= OnRoomCountChanged;
						Msf.Server.Rooms.RoomDestroyed -= OnRoomCountChanged;
						Msf.Server.Spawners.SpawnerRegistered -= OnSpawnersCountChange;
						Msf.Client.Auth.LoggedIn -= OnLoginStatusChanged;
						Msf.Client.Auth.LoggedOut -= OnLoginStatusChanged;
				}

		}
}