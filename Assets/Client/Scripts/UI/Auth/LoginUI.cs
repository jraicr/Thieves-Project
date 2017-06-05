using UnityEngine;
using UnityEngine.UI;
using Barebones.MasterServer;

namespace Thieves.Client.UI {
    /// <summary>
    /// Represents a basic view for login form
    /// </summary>
    public class LoginUI : MonoBehaviour {
        public Text errorText;
        public Button loginButton;
        public InputField password;
        public Toggle remember;
        public InputField username;
        public GameObject passwordResetWindow;

        protected string RememberPrefKey = "msf.auth.remember";
        protected string UsernamePrefKey = "msf.auth.username";

        protected virtual void Awake() {
            errorText = errorText ?? transform.FindChild("Error").GetComponent<Text>();
            loginButton = loginButton ?? transform.FindChild("Button").GetComponent<Button>();
            password = password ?? transform.FindChild("Password").GetComponent<InputField>();
            remember = remember ?? transform.FindChild("Remember").GetComponent<Toggle>();
            username = username ?? transform.FindChild("Username").GetComponent<InputField>();

            if (passwordResetWindow == null) {
                var window = FindObjectOfType<PasswordResetUI>();
                passwordResetWindow = window != null ? window.gameObject : null;
            }

            errorText.gameObject.SetActive(false);

            Msf.Client.Auth.LoggedIn += OnLoggedIn;
        }

        // Use this for initialization
        private void Start() {
            RestoreRememberedValues();
        }

        private void OnEnable() {
            gameObject.transform.localPosition = Vector3.zero;
        }

        protected void OnLoggedIn() {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Tries to restore previously held values
        /// </summary>
        protected virtual void RestoreRememberedValues() {
            username.text = PlayerPrefs.GetString(UsernamePrefKey, username.text);
            remember.isOn = PlayerPrefs.GetInt(RememberPrefKey, -1) > 0;
        }

        /// <summary>
        /// Checks if inputs are valid
        /// </summary>
        /// <returns></returns>
        protected virtual bool ValidateInput() {
            var error = "";

            if (username.text.Length < 3)
                error += "Username is too short \n";

            if (password.text.Length < 3)
                error += "Password is too short \n";

            if (error.Length > 0) {
                // We've got an error
                error = error.Remove(error.Length - 1);
                ShowError(error);
                return false;
            }

            return true;
        }

        protected void ShowError(string message) {
            errorText.gameObject.SetActive(true);
            errorText.text = message;
        }

        /// <summary>
        ///     Called after clicking login button
        /// </summary>
        protected virtual void HandleRemembering() {
            if (!remember.isOn) {
                // Remember functionality is off. Delete all values
                PlayerPrefs.DeleteKey(UsernamePrefKey);
                PlayerPrefs.DeleteKey(RememberPrefKey);
                return;
            }

            // Remember is on
            PlayerPrefs.SetString(UsernamePrefKey, username.text);
            PlayerPrefs.SetInt(RememberPrefKey, 1);
        }

        public virtual void OnLoginClick() {
            if (Msf.Client.Auth.IsLoggedIn) {
                Msf.Events.Fire(Msf.EventNames.ShowDialogBox,
                    DialogBoxData.CreateError("You're already logged in"));

                Logs.Error("You're already logged in");
                return;
            }

            // Disable error
            errorText.gameObject.SetActive(false);

            // Ignore if didn't pass validation
            if (!ValidateInput())
                return;

            HandleRemembering();

            Msf.Client.Auth.LogIn(username.text, password.text, (accountInfo, error) => {
                if (accountInfo == null && (error != null))
                    ShowError(error);
            });

        }

        public virtual void OnPasswordForgotClick() {
            passwordResetWindow.SetActive(true);
        }

        void OnDestroy() {
            Msf.Client.Auth.LoggedIn -= OnLoggedIn;
        }
    }
}