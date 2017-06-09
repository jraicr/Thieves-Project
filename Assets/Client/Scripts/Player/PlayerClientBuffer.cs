using System.Collections.Generic;
using UnityEngine;
using Thieves.Client.PlayerController;
using Thieves.Share.PlayerController;
using Thieves.Share.PlayerNetworking;

namespace Thieves.Client.PlayerNetworking {
		public class PlayerClientBuffer : MonoBehaviour, IInputProcessor {
				List<PlayerInput> inputBuffer;
				NetworkedPlayer player;

				void Awake() {
						inputBuffer = new List<PlayerInput>();
						player = GetComponentInParent<NetworkedPlayer>();
				}

				public void ProcessInput(PlayerInput input) {
						inputBuffer.Add(input);

						if (inputBuffer.Count < player.inputBufferSize && !input.holster) return;

						player.CmdMove(inputBuffer.ToArray());
						inputBuffer.Clear();
				}
		}
}