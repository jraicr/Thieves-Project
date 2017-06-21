using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thieves.Singleplayer.Player {
    public class PlayerController : MonoBehaviour {

        public float speed = 10f;
        private int floorMask;
        private float camRayLength = 100f;
        private bool typedHolster;
        private Vector2 lastTurn;
        private CharacterController charController;

        private void Awake() {
            charController = GetComponent<CharacterController>();
            floorMask = LayerMask.GetMask("Floor");
        }

        void Update() {
            CheckGameInput();
        }

        private void CheckGameInput() {
            Vector2 turn = Vector2.zero;
            Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit floorHit;

            if (Physics.Raycast(camRay, out floorHit, camRayLength, floorMask)) {

                Vector3 playerToMouse = floorHit.point - transform.position;
                turn = new Vector2(playerToMouse.x, playerToMouse.z);


                Vector2 inputMove = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                Vector3 moveDirection = new Vector3(inputMove.x, 0f, inputMove.y).normalized;
                moveDirection *= speed;

                charController.Move(moveDirection * Time.fixedDeltaTime);
                Vector3 position = transform.position;
                transform.position = new Vector3(position.x, position.y, position.z);
                transform.rotation = Quaternion.LookRotation(new Vector3(turn.x, 0f, turn.y));

            }
        }
    }
}