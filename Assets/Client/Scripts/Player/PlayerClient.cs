using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Thieves.Share.PlayerNetworking;
using Thieves.Share.Utils;

namespace Thieves.Client.PlayerNetworking {
		public class PlayerClient : MonoBehaviour {

				public NetworkClient nClient;

				private NetworkedPlayer networkedPlayer;
				private IPlayerClient client;
				private MonotonicTime monotonicTime;
				private Queue<PlayerSnapshot> snapshots;

				private void Awake() {
						client = GetComponent<IPlayerClient>();
						nClient = GameObject.Find("NetworkManager").GetComponent<NetworkManager>().client;
						monotonicTime = FindObjectOfType<MonotonicTime>();
						snapshots = new Queue<PlayerSnapshot>();
						networkedPlayer = GetComponentInParent<NetworkedPlayer>();
				}

				private void FixedUpdate() {
						while (snapshots.Count > 0) {
								PlayerSnapshot snapshot = snapshots.Dequeue();
								client.OnSnapshot(snapshot);

								if (snapshot.shoot || snapshot.holster) continue;
								monotonicTime.SetTime(snapshot.state.timestamp);
						}
				}

				public void OnSnapshot(PlayerState state, bool shoot, bool holster) {
						snapshots.Enqueue(new PlayerSnapshot {
								state = state,
								shoot = shoot,
								holster = holster
						});
				}

				public void ClearSnapshots() {
						snapshots.Clear();
				}
		}
}