using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Thieves.Client.PlayerController;
using Thieves.Client.PlayerNetworking;

namespace Thieves.Client.UI {
    public class IngameMenu : MonoBehaviour {

        public RectTransform menuContainer;

        private NetworkManager netManager;
        private PlayerInputCapture input;
        private bool showMenu;

        private void Start() {
            netManager = FindObjectOfType<NetworkManager>();
            input = FindObjectOfType<PlayerPredicted>().GetComponentInParent<PlayerInputCapture>();
            showMenu = false;
        }

        void Update() {
            if (input.escapeKeyDown && !showMenu) {
                ShowMenu(true);

            } else if (!input.escapeKeyDown && showMenu) {
                ShowMenu(false);
            }
        }

        public void ShowMenu(bool show) {
            showMenu = show;
            input.disabledGameInput = show;
            menuContainer.gameObject.SetActive(show);
        }

        public void OnClickContinueButton() {
            // reboot tracked variable. TODO: Rewrite how to handle with this UI - key concept... this is disgusting
            input.escapeKeyDown = false;  
            ShowMenu(false);
        }

        public void OnClickAbandoneButton() {
            netManager.StopClient();
        }
    }
}