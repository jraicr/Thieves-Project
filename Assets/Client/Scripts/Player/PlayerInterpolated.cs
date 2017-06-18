using System.Collections.Generic;
using UnityEngine;
using Thieves.Client.PlayerController;
using Thieves.Share.PlayerNetworking;
using Thieves.Share.Utils;

namespace Thieves.Client.PlayerNetworking {
		public class PlayerInterpolated : MonoBehaviour, IPlayerClient {
				private MonotonicTime monotonicTime;
				private PlayerShoot shoot;
				private PlayerHolster holster;
				private LinkedList<PlayerSnapshot> snapshots;
				private int inputBufferSize;
				private bool lastHolster;
				private Animator animator;
				private Vector3 lastPosition;

				private void Awake() {
						monotonicTime = FindObjectOfType<MonotonicTime>();
						shoot = GetComponent<PlayerShoot>();
						holster = GetComponent<PlayerHolster>();
						snapshots = new LinkedList<PlayerSnapshot>();
						animator = GetComponentInChildren<Animator>();
						NetworkedPlayer player = GetComponentInParent<NetworkedPlayer>();
						inputBufferSize = player.inputBufferSize;
						lastHolster = true;

						PlayerSnapshot snapshot = new PlayerSnapshot {
								state = player.move,
								shoot = false
						};

						snapshots.AddLast(snapshot);
						SetState(snapshot.state);
						lastPosition = snapshot.state.position;
				}

				private void FixedUpdate() {
						PlayerSnapshot snapshot = PlayerSnapshot.Interpolate(snapshots, monotonicTime.GetTime(), true);
						//animator.SetBool("IsWalking", lastPosition != snapshot.state.position);
						lastPosition = snapshot.state.position;
						SetState(snapshot.state);

						if (snapshot.state.holster != lastHolster) {
								holster.SwitchHolster(snapshot.state.holster, false);
								lastHolster = snapshot.state.holster;
						}

						if (!snapshot.shoot) return;

						shoot.Shoot();
				}

				private void SetState(PlayerState state) {
						transform.position = state.position;
						transform.rotation = Quaternion.LookRotation(new Vector3(state.turn.x, 0f, state.turn.y));
				}

				public void OnSnapshot(PlayerSnapshot snapshot) {

						if (snapshots.Count == 1) {
								PlayerSnapshot first = snapshots.First.Value;
								float minTimestamp = snapshot.state.timestamp - inputBufferSize * Time.fixedDeltaTime;

								if (first.state.timestamp < minTimestamp) {
										PlayerSnapshot snapshotCopy = first;
										PlayerState stateCopy = first.state;
										stateCopy.timestamp = minTimestamp;
										snapshotCopy.state = stateCopy;
										snapshotCopy.shoot = false;
										snapshots.AddLast(snapshotCopy);
								}
						}

						LinkedListNode<PlayerSnapshot> earlier = snapshots.Last;

						while ((earlier != null) && (snapshot.state.timestamp < earlier.Value.state.timestamp)) {
								earlier = earlier.Previous;
						}

						if (earlier == null) {
								snapshots.AddFirst(snapshot);
								return;
						}

						if (snapshot.state.timestamp == earlier.Value.state.timestamp) {
								if (snapshot.shoot) {
										earlier.Value = snapshot;
								}

								return;
						}

						snapshots.AddAfter(earlier, snapshot);
				}
		}
}