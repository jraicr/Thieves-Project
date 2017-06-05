using UnityEngine;
using System.Collections;
using Barebones.MasterServer;
using Barebones.Networking;
using UnityEngine.UI;

/// <summary>
/// Handles inputs from the email confirmation window
/// </summary>
namespace Thieves.Client.UI {
    public class EmailConfirmationUI : MonoBehaviour {
        public Button resendButton;
        public InputField code;

        // Use this for initialization
        void Awake() {

        }

        public void OnConfirmClick() {
            Msf.Client.Auth.ConfirmEmail(code.text, (successful, error) => {
                if (!successful) {
                    Msf.Events.Fire(Msf.EventNames.ShowDialogBox,
                        DialogBoxData.CreateError("Confirmation failed: " + error));
                    Logs.Error("Confirmation failed: " + error);
                    return;
                }

                Msf.Events.Fire(Msf.EventNames.ShowDialogBox,
                        DialogBoxData.CreateInfo("Email confirmed successfully"));

                // Hide the window
                gameObject.SetActive(false);
            });
        }

        public void OnResendClick() {
            resendButton.interactable = false;

            Msf.Client.Auth.RequestEmailConfirmationCode((successful, error) => {
                if (!successful) {
                    Msf.Events.Fire(Msf.EventNames.ShowDialogBox,
                        DialogBoxData.CreateError("Confirmation code request failed: " + error));

                    Logs.Error("Confirmation code request failed: " + error);

                    resendButton.interactable = true;
                    return;
                }

                Msf.Events.Fire(Msf.EventNames.ShowDialogBox,
                        DialogBoxData.CreateInfo("Confirmation code was sent to your e-mail. " +
                                                 "It should arrive within few minutes"));
            });
        }
    }
}