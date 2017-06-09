using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Thieves.Share.PlayerNetworking;
using Thieves.Share.Utils;

namespace Thieves.GameServer.PlayerNetworking {
		public class PlayerHistoryPool : MonoBehaviour {
				public Transform historyPrefab;
				//public Transform LagCompensationPointPrefab;

				public int historyLength = 60;
				public float historyOldest = 1f;
				public int numLayers = 21;
				public int numY = 100;
				public Vector3 delta = 3f * Vector3.down;
				public int damagePerShot = 20;

				private int shootableMask;
				private Queue<Transform[]> historyPool;
				private float interpolationDelay;

				void Awake() {
						//shootableMask = LayerMask.GetMask("Shootable", "Player");
						interpolationDelay = FindObjectOfType<MonotonicTime>().interpolationDelay;
						historyPool = new Queue<Transform[]>();
						int startHistory = 32 - numLayers;

						for (int i = startHistory; i < 32; i++) {

								for (int j = 0; j < startHistory; j++) {
										Physics.IgnoreLayerCollision(i, j);
								}

								for (int j = i + 1; j < 32; j++) {
										Physics.IgnoreLayerCollision(i, j);
								}
						}
				}

				public Transform[] Take() {
						if (historyPool.Count > 0) return historyPool.Dequeue();

						Transform[] history = new Transform[historyLength];

						for (int i = 0; i < historyLength; i++) {
								Transform h = Instantiate(historyPrefab);
								Deactivate(h.gameObject);
								history[i] = h;
						}
						return history;
				}

				public void Put(Transform[] history) {

						foreach (Transform h in history) {
								Deactivate(h.gameObject);
								h.parent = transform;
						}

						historyPool.Enqueue(history);
				}

				public void Record(Transform h, Transform playerTransform) {
						LagCompensationPoint point = LagCompensationPoint.FromTimestamp(Time.fixedTime, numLayers, numY, playerTransform.position, transform.position, delta);
						h.position = point.position;
						h.rotation = Quaternion.LookRotation(new Vector3(playerTransform.forward.x, 0f, playerTransform.forward.z));
						h.gameObject.layer = point.layer;
						h.gameObject.SetActive(true);
				}

				public void Shoot(Transform gunBarrelEnd, float timestamp, int attackerInstanceID) {
						timestamp = Mathf.Clamp(timestamp, Time.fixedTime - historyOldest, Time.fixedTime - interpolationDelay);
						LagCompensationPoint playerHistoryPoint = LagCompensationPoint.FromTimestamp(timestamp, numLayers, numY, gunBarrelEnd.position, transform.position, delta);
						LagCompensationPoint playerHistoryScenaryPoint = LagCompensationPoint.FromTimestamp(timestamp, numLayers, numY, gunBarrelEnd.position, transform.position, Vector3.zero);

						/*  Debugging Lag Compensation Points */
						//Debug.Log("[Server] New lag compensation point in position: " + playerHistoryScenaryPoint.position + "with layer: " + playerHistoryScenaryPoint.layer.ToString());
						//DrawLagCompensationPoint(playerHistoryScenaryPoint, Color.blue);
						//Debug.DrawRay(playerHistoryScenaryPoint.position, gunBarrelEnd.forward, Color.blue, 200f);
						RaycastHit[] shootHits = Physics.RaycastAll(new Ray(playerHistoryPoint.position, gunBarrelEnd.forward), 100f, 1 << playerHistoryPoint.layer);
						RaycastHit[] shootScenaryHits = Physics.RaycastAll(new Ray(playerHistoryScenaryPoint.position, gunBarrelEnd.forward), 100f, 1 << LayerMask.NameToLayer("Shootable"));
					 

						RaycastHit[] hitsToProcess = shootHits.Concat(shootScenaryHits).OrderBy(h => h.distance).ToArray();

						if (hitsToProcess.Length == 0) {
								return;
						}

						ProcessShootHits(hitsToProcess, timestamp, attackerInstanceID);
				}
				 
				private void ProcessShootHits(RaycastHit[] orderedHits, float timestamp, int attackerInstanceID) {
						foreach (RaycastHit r in orderedHits) {
								
								// If we detect shootable obstacles we break the loop since we can't shoot behind walls
								if (r.collider.gameObject.layer == LayerMask.NameToLayer("Shootable")) {
										//Debug.Log("Shooting obstacles.");
										break;

								} else {
										// If we detect ourself we skip this iteration.
										if (r.collider.GetComponentInParent<NetworkedPlayer>().GetInstanceID() == attackerInstanceID) {
												//Debug.Log("Shooting ourself, skipping.");
												continue;
										}

										//LagCompensationPoint hittedPoint = LagCompensationPoint.FromTimestamp(timestamp, numLayers, numY, r.transform.position, transform.position, delta);
										//DrawLagCompensationPoint(hittedPoint, Color.red);
										//Debug.Log(r.collider.gameObject + " " + r.collider.gameObject.layer);
										//r.transform.GetComponent<MeshRenderer>().material.color = Color.cyan;
										//Debug.Log("Player hit!");

										/**
										 * As soon as the target is hitted and recieve damage we leave the foreach. So we will avoid
										 * to hit other players behind the target.
										 */
										r.transform.GetComponentInParent<PlayerHealth>().TakeDamage(damagePerShot);
										break;
								}
						}
				}

				private void Deactivate(GameObject g) {
						g.layer = Physics.IgnoreRaycastLayer;
						g.SetActive(false);
				}

				/*
				private void DrawLagCompensationPoint(LagCompensationPoint point, Color color) {
						Transform h = Instantiate(LagCompensationPointPrefab);
						h.position = point.position;
						h.gameObject.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", color);
				}*/
		}
}
