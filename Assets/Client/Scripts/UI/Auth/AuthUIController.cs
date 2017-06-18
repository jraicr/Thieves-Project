using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Barebones.MasterServer;

/// <summary>
/// Containts references to Auth UI components, and methods to display them.
/// </summary>
namespace Thieves.Client.UI {
    public class AuthUIController : MonoBehaviour {
        public AuthUI authUI;
        public LoginUI loginWindow;
        public RegisterUI registerWindow;
        public PasswordResetUI passwordResetWindow;
        public EmailConfirmationUI emailConfirmationWindow;

        public List<GameObject> enableObjectsOnLogIn;
        public List<GameObject> disableObjectsOnLogout;

        public static AuthUIController Instance;

        protected virtual void Awake() {
            Instance = this;
            authUI = authUI ?? FindObjectOfType<AuthUI>();
            loginWindow = loginWindow ?? FindObjectOfType<LoginUI>();
            registerWindow = registerWindow ?? FindObjectOfType<RegisterUI>();
            passwordResetWindow = passwordResetWindow ?? FindObjectOfType<PasswordResetUI>();
            emailConfirmationWindow = emailConfirmationWindow ?? FindObjectOfType<EmailConfirmationUI>();

            Msf.Client.Auth.LoggedIn += OnLoggedIn;
            Msf.Client.Auth.LoggedOut += OnLoggedOut;

            if (Msf.Client.Auth.IsLoggedIn) {
                OnLoggedIn();
            }
        }

        private void OnLoggedIn() {
            foreach (var obj in enableObjectsOnLogIn) {
                if (obj != null)
                    obj.SetActive(true);
            }
        }

        protected virtual void OnLoggedOut() {
            if (authUI != null)
                authUI.gameObject.SetActive(true);

            foreach (var obj in disableObjectsOnLogout) {
                if (obj != null)
                    obj.SetActive(false);
            }
        }

        protected virtual void OnDestroy() {
            if (Instance == this)
                Instance = null;

            Msf.Client.Auth.LoggedOut -= OnLoggedOut;
            Msf.Client.Auth.LoggedIn -= OnLoggedIn;
        }

        /// <summary>
        /// Displays login window
        /// </summary>
        public virtual void ShowLoginWindow() {
            loginWindow.gameObject.SetActive(true);
        }

        /// <summary>
        /// Displays registration window
        /// </summary>
        public virtual void ShowRegisterWindow() {
            registerWindow.gameObject.SetActive(true);
        }

        /// <summary>
        /// Displays password reset window
        /// </summary>
        public virtual void ShowPasswordResetWindow() {
            passwordResetWindow.gameObject.SetActive(true);
        }

        /// <summary>
        /// Displays e-mail confirmation window
        /// </summary>
        public virtual void ShowEmailConfirmationWindow() {
            emailConfirmationWindow.gameObject.SetActive(true);
        }

        public void LogOut() {
            Msf.Client.Auth.LogOut();
        }
    }
}