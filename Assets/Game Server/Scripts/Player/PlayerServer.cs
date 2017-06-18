using System.Collections.Generic;
using UnityEngine;
using Thieves.Share.PlayerController;
using Thieves.Share.PlayerNetworking;

namespace Thieves.GameServer.PlayerNetworking {
    [RequireComponent(typeof(PlayerServer))]
    [RequireComponent(typeof(PlayerSim))]
    [RequireComponent(typeof(PlayerHistory))]

    public class PlayerServer : MonoBehaviour {
        public Transform gunBarrelEnd;
        private Queue<PlayerInput> inputBuffer;
        private PlayerSim sim;
        private NetworkedPlayer player;
        private PlayerHistoryPool pool;
        private PlayerHistory history;
        private int movesMade;

        private void Awake() {
            inputBuffer = new Queue<PlayerInput>();
            sim = GetComponent<PlayerSim>();
            player = GetComponentInParent<NetworkedPlayer>();
            history = player.GetComponent<PlayerHistory>();
            pool = FindObjectOfType<PlayerHistoryPool>();
            player.move = sim.state;
        }

        private void FixedUpdate() {
            history.Record(transform); // It is important to computes transform before 5 input buffer completed.
            if (movesMade > 0) {

                if ((--movesMade) > 0) return;
            }

            int movesToMake = inputBuffer.Count < player.inputBufferSize ? inputBuffer.Count : player.inputBufferSize;

            if (movesToMake == 0) return;

            while (movesMade < movesToMake) {
                PlayerInput input = inputBuffer.Dequeue();
                PlayerAction playerActions = sim.Move(input, Time.fixedTime - Time.fixedDeltaTime * (movesToMake - (++movesMade)));
                bool holsterSwitched = sim.state.holster != sim.lastState.holster;

                if (!holsterSwitched && !playerActions.shoot) {
                    continue;

                } else if (holsterSwitched) {
                    player.move = sim.state; // Updates the Move state with the new holster value
                    continue;

                } else if (!sim.state.holster && playerActions.shoot) {
                    player.shoot = sim.state; // Updates the Shoot state with new shoot value
                    pool.Shoot(gunBarrelEnd, input.timestamp, gameObject.GetComponentInParent<NetworkedPlayer>().GetInstanceID());
                }
            }

            player.move = sim.state;
        }

        public void Move(PlayerInput[] inputs) {
            foreach (PlayerInput input in inputs) {
                inputBuffer.Enqueue(input);
            }
        }

        public void SetInitialState(Vector3 position, Vector2 turn, int startingHealth) {
            PlayerState newPlayerState = PlayerState.CreateStartingState(position, turn, startingHealth);
            sim.SetState(newPlayerState);
            player.move = sim.state;
            player.health = sim.state;
        }
    }
}
