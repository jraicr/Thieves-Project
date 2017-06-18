using UnityEngine;
using Thieves.Share.PlayerNetworking;
using Thieves.Share.PlayerController;

namespace Thieves.Client.PlayerController {
    public class PlayerHolster : MonoBehaviour {

        public Transform gunBarrelEnd;
        public Transform gun;
        public float range = 100f;

        private PlayerSim sim;
        private NetworkedPlayer networkedPlayer;
        private LineRenderer gunLine;
        private int shootableMask;

        void Awake() {
            sim = GetComponent<PlayerSim>();
            gunLine = gunBarrelEnd.GetComponent<LineRenderer>();
            networkedPlayer = GetComponentInParent<NetworkedPlayer>();
            shootableMask = LayerMask.GetMask("Shootable", "Player");
        }

        void FixedUpdate() {
            if (networkedPlayer.isLocalPlayer) {
                if (!sim.state.holster) {
                    gunLine.SetPosition(0, gunBarrelEnd.position);
                    RaycastHit shootHit;
                    bool hitSomething = Physics.Raycast(gunBarrelEnd.position, gunBarrelEnd.forward, out shootHit, range, shootableMask);
                    gunLine.SetPosition(1, hitSomething ? shootHit.point : gunBarrelEnd.position + gunBarrelEnd.forward * range);
                    //if (hitSomething) Debug.Log("[Client] Hitting " + shootHit.collider.gameObject.name);
                }
            }
        }

        public void SwitchHolster(bool holster, bool localPlayer = true) {
            SwitchWeapon(holster);

            if (localPlayer) {
                // Unholster effects
                if (!holster && !gunLine.enabled) {
                    gunLine.enabled = true;

                    // Holster effects    
                } else if (holster && gunLine.enabled) {
                    gunLine.enabled = false;
                }
            }
        }


        private void SwitchWeapon(bool holster) {
            if (!holster) {
                RotateWeapon(new Vector3(0f, 0f, 0f));
                //gun.transform.localPosition = new Vector3(-1.495f, 1.144f, 0.072f);

            } else {
                RotateWeapon(new Vector3(50f, 0f, 0f));
                //gun.transform.localPosition = new Vector3(-1.527f, 1.561f, -0.253f);
            }
        }

        private void RotateWeapon(Vector3 eulerAngles) {
            gun.transform.localEulerAngles = eulerAngles;
            gunBarrelEnd.localEulerAngles = eulerAngles;
        }

    }
}
