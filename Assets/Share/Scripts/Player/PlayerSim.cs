using UnityEngine;
using Thieves.Share.PlayerNetworking;

namespace Thieves.Share.PlayerController {
    [RequireComponent(typeof(CharacterController))]
    public class PlayerSim : MonoBehaviour {
        public float stealthSpeed = 8f;
        public float normalSpeed = 14f;
        public float gravity = 20.0f;
        public PlayerState state;
        private Vector3 moveDirection = Vector3.zero;

        private bool unholster;

        private CharacterController characterController;
        private float timeBetweenBullets;

        private void Awake() {
            characterController = GetComponent<CharacterController>();
            timeBetweenBullets = GetComponentInParent<NetworkedPlayer>().timeBetweenBullets;
            unholster = false;
        }

        public void SetState(PlayerState state) {
            this.state = state;
            transform.position = new Vector3(state.position.x, state.position.y, state.position.z);
            transform.rotation = Quaternion.LookRotation(Vector3.forward);
        }

        public PlayerAction Move(PlayerInput input, float timestamp) {

            if (characterController.isGrounded) {
                moveDirection = new Vector3(input.move.x, 0f, input.move.y).normalized;
                moveDirection *= (input.stealth ? stealthSpeed : normalSpeed);
            }

            moveDirection.y -= gravity * Time.fixedDeltaTime;
            characterController.Move(moveDirection * Time.fixedDeltaTime);
            Vector3 position = transform.position;
            transform.position = new Vector3(position.x, position.y, position.z);

            float nextBullet = state.nextBullet;

            if (nextBullet > 0f) {
                nextBullet -= Time.fixedDeltaTime;
            }

            bool shoot = false;
            bool typedHolster = false;

            if (input.holster) {
                typedHolster = true;
                unholster = !unholster;
            }

            if ((nextBullet <= 0f) && input.shoot && unholster) {
                shoot = true;
                nextBullet += timeBetweenBullets;
            }

            bool stealth = input.stealth;

            state = new PlayerState {
                timestamp = timestamp,
                moveNum = 1 + state.moveNum,
                position = new Vector3(transform.position.x, transform.position.y, transform.position.z),
                turn = (input.turn == Vector2.zero) ? state.turn : input.turn,
                nextBullet = nextBullet
            };

            transform.rotation = Quaternion.LookRotation(new Vector3(state.turn.x, 0f, state.turn.y));

            return new PlayerAction(shoot, typedHolster, stealth);
        }

        public bool GetUnholster() {
            return unholster;
        }

        public void SetUnholster(bool unholster) {
            this.unholster = unholster;
        }
    }
}
