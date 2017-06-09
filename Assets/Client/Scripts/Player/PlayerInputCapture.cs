using UnityEngine;
using Thieves.Share.PlayerController;
using Thieves.Share.PlayerNetworking;
using Thieves.Share.Utils;

namespace Thieves.Client.PlayerController {
		public class PlayerInputCapture : MonoBehaviour {
				private IInputProcessor[] inputProcessors;
				private MonotonicTime monotonicTime;
				private int floorMask;
				private float camRayLength = 100f;
				private bool typedHolster;
				private Vector2 lastTurn;
				private NetworkedPlayer player;

				private void Awake() {
						inputProcessors = GetComponents<IInputProcessor>();
						monotonicTime = FindObjectOfType<MonotonicTime>();
						player = GetComponentInParent<NetworkedPlayer>();
						floorMask = LayerMask.GetMask("Floor");
						typedHolster = false;
				}

				private void Update() {

						if (!typedHolster) {
								typedHolster = Input.GetButtonDown("Holster");
						}
				}

				private void FixedUpdate() {
						Vector2 turn = Vector2.zero;
						Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
						RaycastHit floorHit;

						if (Physics.Raycast(camRay, out floorHit, camRayLength, floorMask)) {

								Vector3 playerToMouse = floorHit.point - transform.position;
								turn = new Vector2(playerToMouse.x, playerToMouse.z);

								bool shoot = Input.GetButton("Fire1");
								bool stealth = Input.GetButton("Stealth");
								bool holster = typedHolster;

								if (holster) {
										typedHolster = false;
								}

								PlayerInput input = new PlayerInput {
										timestamp = monotonicTime.GetTime(),
										move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")),
										turn = turn,
										shoot = shoot,
										holster = holster,
										stealth = stealth
								};

								// Avoid input buffering if there isn't new input
								if (input.move == Vector2.zero && input.holster == false && input.shoot == false && input.turn == lastTurn && input.stealth == false) return;
								lastTurn = input.turn;

								foreach (IInputProcessor inputProcessor in inputProcessors) {
										inputProcessor.ProcessInput(input);
								}
						}

				}
		}
}