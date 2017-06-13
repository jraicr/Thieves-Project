using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Barebones.Networking;
using Barebones.MasterServer;
using UnityEngine.UI;

namespace Thieves.Client.UI {
		public class ClientConnectionStatusUI : MonoBehaviour {

				protected static ConnectionStatus LastStatus;

				private IClientSocket _connection;

				public Image image;

				public Text text;

				public Color unknownColor = new Color(90 / 255f, 90 / 255f, 90 / 255f, 1);
				public Color onlineColor = new Color(114 / 255f, 198 / 255f, 80 / 255f, 1);
				public Color connectingColor = new Color(220 / 255f, 160 / 255f, 50 / 255f, 1);
				public Color offlineColor = new Color(200 / 255f, 60 / 255f, 60 / 255f, 1);

				public bool changeTextColor = true;

				protected virtual void Start() {
						_connection = GetConnection();
						_connection.StatusChanged += UpdateStatusView;

						UpdateStatusView(_connection.Status);
				}

				protected virtual void UpdateStatusView(ConnectionStatus status) {
						LastStatus = status;

						switch (status) {
								case ConnectionStatus.Connected:
										if (image != null) image.color = onlineColor;
										if (changeTextColor) text.color = onlineColor;
										text.text = "Connected";
										break;

								case ConnectionStatus.Disconnected:
										if (image != null) image.color = offlineColor;
										if (changeTextColor) text.color = offlineColor;

										text.text = "Offline";
										break;

								case ConnectionStatus.Connecting:
										if (image != null) image.color = connectingColor;
										if (changeTextColor) text.color = connectingColor;

										text.text = "Connecting";
										break;

								default:
										if (image != null) image.color = unknownColor;
										if (changeTextColor) text.color = unknownColor;

										text.text = "Unknown";
										break;
						}
				}

				protected virtual IClientSocket GetConnection() {
						return Msf.Connection;
				}

				protected virtual void OnDestroy() {
						_connection.StatusChanged -= UpdateStatusView;
				}
		}
}
