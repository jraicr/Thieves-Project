using UnityEngine;
using Thieves.Share.PlayerNetworking;
using Thieves.Client.Manager;


namespace Thieves.Client.PlayerController {
		public class PlayerShoot : MonoBehaviour {
				public Transform gunBarrelEnd;
				//public Light faceLight;
				//public Light gunLight;
				public float range = 100f;

				private AudioSource gunAudio;
				private int shootableMask;
				private float effectsDisplayTime = 0.2f;
				private float timer;

				private CameraManager camManagement;

				private void Awake() {
						gunAudio = gunBarrelEnd.GetComponent<AudioSource>();
						shootableMask = LayerMask.GetMask("Shootable", "Player");
						effectsDisplayTime *= GetComponentInParent<NetworkedPlayer>().timeBetweenBullets;
						camManagement = FindObjectOfType<CameraManager>();
				}

				private void FixedUpdate() {
						if (timer <= 0f) {
								//faceLight.enabled = false;
								//gunLight.enabled = false;
								return;
						}
						timer -= Time.fixedDeltaTime;
				}

				public void Shoot(bool localPlayer = false) {
						timer = effectsDisplayTime;
						//gunAudio.Play();

						if (localPlayer) {
								camManagement.GunShake();
						}

						//gunLight.enabled = true;
						//faceLight.enabled = true;

						RaycastHit shootHit;
						bool hitSomething = Physics.Raycast(gunBarrelEnd.position, gunBarrelEnd.forward, out shootHit, range, shootableMask);

						if (!hitSomething) return;

						PlayerHurt hurt = shootHit.transform.GetComponent<PlayerHurt>();
						if (hurt == null) return;

						hurt.TakeDamage(shootHit.point);
				}
		}
}