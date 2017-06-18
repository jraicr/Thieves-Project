using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Barebones.MasterServer;

namespace Thieves.Client.UI {
    /// <summary>
    /// Dialog box
    /// </summary>
    public class DialogBoxUI : MonoBehaviour {

        public Button button1;
        public Button button2;
        public Button button3;
        public InputField input;
        public Text dialogText;
        public Text placeHolderText;

        private DialogBoxData data;
        private Queue<DialogBoxData> queue;
        private bool wasDialogShown;
        private bool hasSubscribedToEvents;

        // Use this for initialization
        private void Awake() {
            queue = new Queue<DialogBoxData>();

            // Disable itself
            if (!wasDialogShown)
                gameObject.SetActive(false);
        }

        public void SubscribeToEvents() {
            if (hasSubscribedToEvents) {
                return;
            }

            hasSubscribedToEvents = true;

            Msf.Events.Subscribe(Msf.EventNames.ShowDialogBox, OnDialogEvent);
        }

        private void AfterHandlingClick() {
            if (queue.Count > 0) {
                ShowDialog(queue.Dequeue());
                return;
            }

            CloseDialog();
        }

        public void OnLeftClick() {
            if (data.LeftButtonAction != null)
                data.LeftButtonAction.Invoke();

            AfterHandlingClick();
        }

        public void OnMiddleClick() {
            if (data.MiddleButtonAction != null)
                data.MiddleButtonAction.Invoke();

            AfterHandlingClick();
        }

        public void OnRightClick() {
            if (data.RightButtonAction != null)
                data.RightButtonAction.Invoke();

            if (data.ShowInput)
                data.InputAction.Invoke(input.text);

            AfterHandlingClick();
        }

        public void ShowDialog(DialogBoxData data) {
            wasDialogShown = true;
            ResetAll();

            this.data = data;

            if ((data == null) || (data.Message == null))
                return;

            // Show the dialog box
            gameObject.SetActive(true);

            dialogText.text = data.Message;

            var buttonCount = 0;

            if (!string.IsNullOrEmpty(data.LeftButtonText)) {
                // Setup Left button
                button1.gameObject.SetActive(true);
                button1.GetComponentInChildren<Text>().text = data.LeftButtonText;
                buttonCount++;
            }

            if (!string.IsNullOrEmpty(data.MiddleButtonText)) {
                // Setup Middle button
                button2.gameObject.SetActive(true);
                button2.GetComponentInChildren<Text>().text = data.MiddleButtonText;
                buttonCount++;
            }

            if (!string.IsNullOrEmpty(data.RightButtonText)) {
                // Setup Right button
                button3.gameObject.SetActive(true);
                button3.GetComponentInChildren<Text>().text = data.RightButtonText;
            } else if (buttonCount == 0) {
                // Add a default "Close" button if there are no other buttons in the dialog
                button3.gameObject.SetActive(true);
                button3.GetComponentInChildren<Text>().text = "Close";
            }

            if (data.ShowInput) {
                input.gameObject.SetActive(true);
                input.text = data.DefaultInputText;
            }

            transform.SetAsLastSibling();
        }

        private void OnDialogEvent(object arg1, object arg2) {
            var data = arg1 as DialogBoxData;

            if (gameObject.activeSelf) {
                // We're already showing something
                // Display later by adding to queue
                queue.Enqueue(data);
                return;
            }

            ShowDialog(data);
        }

        private void ResetAll() {
            button1.gameObject.SetActive(false);
            button2.gameObject.SetActive(false);
            button3.gameObject.SetActive(false);
            input.gameObject.SetActive(false);
        }

        private void CloseDialog() {
            gameObject.SetActive(false);
        }

        public static void ShowError(string error) {
            // Fire an event to display a dialog box.
            // We're not opening it directly, in case there's a custom 
            // dialog box event handler
            Msf.Events.Fire(Msf.EventNames.ShowDialogBox, DialogBoxData.CreateError(error));
        }
    }

    public class DialogBoxData {
        public string DefaultInputText = "";
        public Action<string> InputAction;
        public Action LeftButtonAction;

        public string LeftButtonText;
        public Action MiddleButtonAction;

        public string MiddleButtonText;
        public Action RightButtonAction;

        public string RightButtonText = "Close";

        public bool ShowInput;

        public DialogBoxData(string message) {
            Message = message;
        }

        public string Message { get; private set; }

        public static DialogBoxData CreateError(string message) {
            return new DialogBoxData(message);
        }

        public static DialogBoxData CreateInfo(string message) {
            return new DialogBoxData(message);
        }

        public static DialogBoxData CreateTextInput(string message, Action<string> onComplete,
                string rightButtonText = "OK") {
            var data = new DialogBoxData(message);
            data.ShowInput = true;
            data.RightButtonText = rightButtonText;
            data.InputAction = onComplete;
            data.LeftButtonText = "Close";
            return data;
        }
    }
}