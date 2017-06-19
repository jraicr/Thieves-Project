using Barebones.MasterServer;
using UnityEngine;

namespace Thieves.Client.UI {
    public class AuthUI : MonoBehaviour {
        public GameObject loginWindow;
        public GameObject registerWindow;

        public bool deactivateOnLogIn = true;
        public static AuthUI Instance;

        void Start() {
            loginWindow = loginWindow ?? FindObjectOfType<LoginUi>().gameObject;
            registerWindow = registerWindow ?? FindObjectOfType<RegisterUi>().gameObject;
            Msf.Client.Auth.LoggedIn += OnLoggedIn;

            // In case we're already logged in 
            if (Msf.Client.Auth.IsLoggedIn) {
                OnLoggedIn();
            }
        }

        private void OnLoggedIn() {
            if (deactivateOnLogIn) {
                gameObject.SetActive(false);
            }
        }

        public void OnLoginClick() {
            if (!Msf.Client.Auth.IsLoggedIn) {
                loginWindow.gameObject.SetActive(true);
            }
        }

        public void OnRegisterClick() {
            if (!Msf.Client.Auth.IsLoggedIn) {
                registerWindow.SetActive(true);
            }
        }

        protected virtual void OnDestroy() {
            if (Instance == this)
                Instance = null;
            Msf.Client.Auth.LoggedIn -= OnLoggedIn;
        }
    }
}
