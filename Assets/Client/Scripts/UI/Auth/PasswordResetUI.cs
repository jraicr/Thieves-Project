using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Barebones.MasterServer;

namespace Thieves.Client.UI {
		public class PasswordResetUI : MonoBehaviour {
				public InputField email;
				public InputField resetCode;
				public InputField password;
				public InputField passwordRepeat;

				public Button sendCodeButton;
				public Button resetButton;

				// Use this for initialization
				void Start() {
						if (sendCodeButton != null)
								sendCodeButton.onClick.AddListener(OnSendCodeClick);

						if (resetButton != null)
								resetButton.onClick.AddListener(OnResetClick);
				}

				private void OnEnable() {
						gameObject.transform.localPosition = Vector3.zero;
				}

				public void OnSendCodeClick() {
						var email = this.email.text.ToLower().Trim();

						if (email.Length < 3 || !email.Contains("@")) {
								Msf.Events.Fire(Msf.EventNames.ShowDialogBox,
										DialogBoxData.CreateError("Invalid e-mail address provided"));
								return;
						}

						var promise = Msf.Events.FireWithPromise(Msf.EventNames.ShowLoading,
								"Requesting reset code");

						Msf.Client.Auth.RequestPasswordReset(email, (successful, error) => {
								promise.Finish();

								if (!successful) {
										Msf.Events.Fire(Msf.EventNames.ShowDialogBox,
												DialogBoxData.CreateError(error));
										return;
								}

								Msf.Events.Fire(Msf.EventNames.ShowDialogBox,
										DialogBoxData.CreateInfo(
										"Reset code has been sent to the provided e-mail address."));
						});
				}

				public void OnResetClick() {
						var email = this.email.text.Trim().ToLower();
						var code = resetCode.text;
						var newPassword = password.text;

						if (password.text != passwordRepeat.text) {
								Msf.Events.Fire(Msf.EventNames.ShowDialogBox,
										DialogBoxData.CreateError("Passwords do not match"));
								return;
						}

						if (newPassword.Length < 3) {
								Msf.Events.Fire(Msf.EventNames.ShowDialogBox,
										DialogBoxData.CreateError("Password is too short"));
								return;
						}

						if (string.IsNullOrEmpty(code)) {
								Msf.Events.Fire(Msf.EventNames.ShowDialogBox,
										DialogBoxData.CreateError("Invalid code"));
								return;
						}

						if (email.Length < 3 || !email.Contains("@")) {
								Msf.Events.Fire(Msf.EventNames.ShowDialogBox,
										DialogBoxData.CreateError("Invalid e-mail address provided"));
								return;
						}

						var data = new PasswordChangeData() {
								Email = email,
								Code = code,
								NewPassword = newPassword
						};

						var promise = Msf.Events.FireWithPromise(Msf.EventNames.ShowLoading,
								"Changing a password");

						Msf.Client.Auth.ChangePassword(data, (successful, error) => {
								promise.Finish();

								if (!successful) {
										Msf.Events.Fire(Msf.EventNames.ShowDialogBox,
												DialogBoxData.CreateError(error));
										return;
								}

								Msf.Events.Fire(Msf.EventNames.ShowDialogBox,
										DialogBoxData.CreateInfo(
										"Password changed successfully"));

								Msf.Events.Fire(Msf.EventNames.RestoreLoginForm, new LoginFormData {
										Username = null,
										Password = ""
								});

								gameObject.SetActive(false);
						});
				}
		}
}
