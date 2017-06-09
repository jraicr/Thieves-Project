using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.LuisPedroFonseca.ProCamera2D;

namespace Thieves.Client.Manager {
		public class CameraManager : MonoBehaviour {

				[SerializeField]
				private float CamOffsetX = 0f;

				[SerializeField]
				private float CamOffsetY = -21.76f;
				private Vector2 camTargetOffset;
				private float targetInfluenceH = 1f;
				private float targetInfluenceV = 1f;
				private float duration = 0f;

				private ProCamera2D mainCamera;
				private ProCamera2DShake camShakeEffect;

				private enum ShakePreset {
						GunShake = 0,
						HurtShake = 1
				}

				private void Awake() {
						camTargetOffset = new Vector2(CamOffsetX, CamOffsetY);
						mainCamera = FindObjectOfType<ProCamera2D>();
						camShakeEffect = mainCamera.GetComponent<ProCamera2DShake>();
				}

				public CameraTarget AddCameraTarget(Transform targetTransform) {
						return mainCamera.AddCameraTarget(targetTransform, this.targetInfluenceH, this.targetInfluenceV, this.duration, this.camTargetOffset);
				}

				public void GunShake() {
						if (camShakeEffect != null) {
								camShakeEffect.Shake((int)ShakePreset.GunShake);
						}
				}

				public void HurtShake() {
						if (camShakeEffect != null) {
								camShakeEffect.Shake((int)ShakePreset.HurtShake);
						}
				}
		}
}
