using System.Collections.Generic;
using UnityEngine;
using Thieves.Client.PlayerController;
using Thieves.Client.Manager;
using Thieves.Share.PlayerNetworking;
using Thieves.Share.PlayerController;

namespace Thieves.Client.PlayerNetworking {
    public class PlayerPredicted : MonoBehaviour, IPlayerClient, IInputProcessor {
        private NetworkedPlayer player;
        private PlayerSim sim;
        private PlayerShoot shoot;
        private PlayerHolster holster;
        private Queue<PlayerPrediction> predictions;
        private CameraManager camManagement;
        private Animator animator;

        private void Awake() {
            player = GetComponentInParent<NetworkedPlayer>();
            sim = GetComponent<PlayerSim>();
            sim.SetState(player.move);
            shoot = GetComponent<PlayerShoot>();
            holster = GetComponent<PlayerHolster>();
            predictions = new Queue<PlayerPrediction>();
            animator = GetComponentInChildren<Animator>();

            // Cam settings
            camManagement = FindObjectOfType<CameraManager>();
            camManagement.AddCameraTarget(transform);
        }

        private PlayerPrediction Predict(PlayerInput input, bool predictShoot, bool predictHolster) {
            PlayerAction playerActions = sim.Move(input, 0f);
            bool holsterSwitched = sim.state.holster != sim.lastState.holster;

            if (predictHolster) {
                if (holsterSwitched) {
                    holster.SwitchHolster(sim.state.holster);
                }
            }

            if (predictShoot) {
                if (!sim.state.holster && playerActions.shoot) {
                    shoot.Shoot(true);
                }
            }

            return new PlayerPrediction {
                input = input,
                state = sim.state
            };
        }

        public void OnSnapshot(PlayerSnapshot snapshot) {
            if (predictions.Count == 0) return;

            PlayerPrediction prediction = predictions.Peek();

            if (snapshot.state.moveNum < prediction.state.moveNum) return;

            do {
                prediction = predictions.Dequeue();
            } while (snapshot.state.moveNum > prediction.state.moveNum);


            if (PlayerState.AreSimilar(snapshot.state, prediction.state)) return;

            Debug.Log("[Client Predicted] Rolling Back");

            sim.SetState(snapshot.state);
            Queue<PlayerPrediction> newPredictions = new Queue<PlayerPrediction>();

            foreach (PlayerPrediction oldPrediction in predictions) {
                newPredictions.Enqueue(Predict(oldPrediction.input, false, false));
            }
            predictions = newPredictions;
        }

        public void ProcessInput(PlayerInput input) {
            predictions.Enqueue(Predict(input, true, true));
            //animator.SetBool("IsWalking", lastPosition != sim.state.position);
            //animator.SetBool("IsWalking", input.move != Vector2.zero);
        }

        public void ClearPredictions() {
            predictions.Clear();
        }
    }
}