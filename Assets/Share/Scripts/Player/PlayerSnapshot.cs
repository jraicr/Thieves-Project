using System.Collections.Generic;
using UnityEngine;

namespace Thieves.Share.PlayerNetworking {
		public struct PlayerSnapshot {
				public PlayerState state;
				public bool shoot;
				public bool stealth;

				static public PlayerSnapshot Interpolate(LinkedList<PlayerSnapshot> snapshots, float time, bool removeOld) {
						LinkedListNode<PlayerSnapshot> fromNode = snapshots.First;
						LinkedListNode<PlayerSnapshot> toNode = fromNode.Next;
						bool shoot = false;
						bool stealth = false;

						while ((toNode != null) && (toNode.Value.state.timestamp <= time)) {
								fromNode = toNode;
								toNode = fromNode.Next;
								if (!removeOld) continue;
								shoot = shoot || fromNode.Value.shoot;
								stealth = stealth || fromNode.Value.stealth;
								snapshots.RemoveFirst();
						}

						PlayerSnapshot from = fromNode.Value;
						PlayerState state = from.state;

						if (toNode != null) {
								PlayerState to = toNode.Value.state;
								float normalizedTime = Mathf.InverseLerp(state.timestamp, to.timestamp, time);
								Vector3 turn = Vector3.Slerp(new Vector3(state.turn.x, 0f, state.turn.y), new Vector3(to.turn.x, 0f, to.turn.y), normalizedTime);
								state = new PlayerState {
										timestamp = time,
										moveNum = 0,
										position = Vector3.Lerp(state.position, to.position, normalizedTime),
										turn = new Vector2(turn.x, turn.z),
										holster = to.holster,
										nextBullet = 0f
								};
						}
						return new PlayerSnapshot {
								state = state,
								shoot = shoot,
								stealth = stealth
						};
				}
		}
}
