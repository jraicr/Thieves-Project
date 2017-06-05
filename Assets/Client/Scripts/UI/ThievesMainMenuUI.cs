using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Barebones.MasterServer;

namespace Thieves.Client.UI {
    public class ThievesMainMenuUI : MonoBehaviour {
        public static ThievesMainMenuUI Instance;
        public DialogBoxUI dialogBox;
        public LoadingUI loading;

        public ClientConnectionStatusUI ConnectionStatus;

        protected virtual void Awake() {
            if (Msf.Args.DestroyUi) {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            dialogBox = dialogBox ?? FindObjectOfType<DialogBoxUI>();
            loading = loading ?? FindObjectOfType<LoadingUI>();

            SubscribeToEvents();
        }

        protected virtual void SubscribeToEvents() {
            if (dialogBox != null)
                dialogBox.SubscribeToEvents();

            if (loading != null)
                loading.SubscribeToEvents();
        }

        void OnDestroy() {
            if (Instance == this) {
                Instance = null;
            }
        }
    }
}