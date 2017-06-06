using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Barebones.MasterServer;

namespace Thieves.Client.UI {
		/// <summary>
		///  Represents a basic view of registration form
		/// </summary>
		public class RegisterUI : MonoBehaviour {
				public InputField email;
				public Text errorText;
				public Text guestNotice;
				public InputField password;

				public Button registerButton;
				public InputField repeatPassword;

				public InputField username;

				protected void OnAwake() {
						Msf.Client.Auth.LoggedIn += OnLoggedIn;

						email = email ?? transform.FindChild("Email").GetComponent<InputField>();
						errorText = errorText ?? transform.FindChild("Error").GetComponent<Text>();
						guestNotice = guestNotice ?? transform.FindChild("GuestNotice").GetComponent<Text>();
						registerButton = registerButton ?? transform.FindChild("Button").GetComponent<Button>();
						password = password ?? transform.FindChild("Password").GetComponent<InputField>();
						repeatPassword = repeatPassword ?? transform.FindChild("RepeatPassword").GetComponent<InputField>();
						username = username ?? transform.FindChild("Username").GetComponent<InputField>();

						errorText.gameObject.SetActive(false);
				}

				// TODO make more checks
				public bool ValidateInput() {
						var error = "";

						if (username.text.Length <= 3)
								error += "Username too short\n";

						if (password.text.Length <= 3)
								error += "Password is too short\n";

						if (!password.text.Equals(repeatPassword.text))
								error += "Passwords don't match\n";

						if (email.text.Length <= 3)
								error += "Email too short\n";

						if (error.Length > 0) {
								error = error.Remove(error.Length - 1);
								ShowError(error);
								return false;
						}

						return true;
				}

				private void UpdateGuestNotice() {
						var auth = Msf.Client.Auth;
						if (guestNotice != null && auth != null && auth.AccountInfo != null) {
								guestNotice.gameObject.SetActive(auth.IsLoggedIn && auth.AccountInfo.IsGuest);
						}
				}

				private void OnEnable() {
						gameObject.transform.localPosition = Vector3.zero;
						UpdateGuestNotice();
				}

				protected void OnLoggedIn() {
						UpdateGuestNotice();
						gameObject.SetActive(false);
				}

				private void ShowError(string message) {
						errorText.gameObject.SetActive(true);
						errorText.text = message;
				}

				public void OnRegisterClick() {
						// Disable error
						errorText.gameObject.SetActive(false);

						// Ignore if didn't pass validation
						if (!ValidateInput()) {
								return;
						}

						var data = new Dictionary<string, string> {
								{"username", username.text},
								{"password", password.text},
								{"email", email.text}
						};

						var promise = Msf.Events.FireWithPromise(Msf.EventNames.ShowLoading);

						Msf.Client.Auth.Register(data, (successful, message) => {
								promise.Finish();

								if (!successful && (message != null)) {
										ShowError(message);
										return;
								}

								OnSuccess();
						});
				}

				protected void OnSuccess() {
						// Hide registration form
						gameObject.SetActive(false);

						// Show the dialog box
						Msf.Events.Fire(Msf.EventNames.ShowDialogBox,
								DialogBoxData.CreateInfo("Registration successful!"));

						// Show the login forum
						Msf.Events.Fire(Msf.EventNames.RestoreLoginForm, new LoginFormData {
								Username = username.text,
								Password = ""
						});

						if ((Msf.Client.Auth.AccountInfo != null) && Msf.Client.Auth.AccountInfo.IsGuest)
								Msf.Client.Auth.LogOut();
				}

				void OnDestroy() {
						Msf.Client.Auth.LoggedIn -= OnLoggedIn;
				}
		}
}
