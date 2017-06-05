using System;
using System.Collections;
using System.Collections.Generic;
using Barebones.Networking;
using UnityEngine;
using UnityEngine.UI;
using Barebones.MasterServer;

namespace Thieves.Client.UI {
    /// <summary>
    /// Displays progress of game creation
    /// </summary>
    public class CreateGameProgressUI : MonoBehaviour {
        public Button abortButton;

        public float enableAbortAfterSeconds = 10;
        public float forceCloseAfterAbortRequestTimeout = 10;

        public string pleaseWaitText = "Please wait...";

        protected SpawnRequestController request;
        public Image rotatingImage;

        public Text statusText;

        public bool setAsLastSiblingOnEnable = true;

        private void Update() {
            rotatingImage.transform.Rotate(Vector3.forward, Time.deltaTime * 360 * 2);

            if (request == null)
                return;

            if (statusText != null)
                statusText.text = string.Format("Progress: {0}/{1} ({2})",
                    (int)request.Status,
                    (int)SpawnStatus.Finalized,
                    request.Status);
        }

        public void OnEnable() {
            if (setAsLastSiblingOnEnable)
                transform.SetAsLastSibling();
        }

        public void OnAbortClick() {
            if (request == null) {
                // If there's no  request to abort, just hide the window
                gameObject.SetActive(false);
                return;
            }

            // Start a timer which will close the window
            // after timeout, in case abortion fails
            StartCoroutine(CloseAfterRequest(forceCloseAfterAbortRequestTimeout, request.SpawnId));

            // Disable abort button
            abortButton.interactable = false;

            request.Abort((isHandled, error) => {
                // If request is not handled, enable the button abort button
                abortButton.interactable = !isHandled;
            });
        }

        public IEnumerator EnableAbortDelayed(float seconds, int spawnId) {
            yield return new WaitForSeconds(seconds);

            if ((request != null) && (request.SpawnId == spawnId))
                abortButton.interactable = true;
        }

        public IEnumerator CloseAfterRequest(float seconds, int spawnId) {
            yield return new WaitForSeconds(seconds);

            if ((request != null) && (request.SpawnId == spawnId)) {
                gameObject.SetActive(false);

                // Send another abort request just in case
                // (maybe something unstuck?)
                request.Abort();
            }
        }

        protected void OnStatusChange(SpawnStatus status) {
            if (status < SpawnStatus.None) {
                // If game was aborted
                Msf.Events.Fire(Msf.EventNames.ShowDialogBox,
                    DialogBoxData.CreateInfo("Game creation aborted"));

                Logs.Error("Game creation aborted");

                // Hide the window
                gameObject.SetActive(false);
            }

            if (status == SpawnStatus.Finalized) {
                request.GetFinalizationData((data, error) => {
                    if (data == null) {
                        Msf.Events.Fire(Msf.EventNames.ShowDialogBox,
                            DialogBoxData.CreateInfo("Failed to retrieve completion data: " + error));

                        Logs.Error("Failed to retrieve completion data: " + error);

                        request.Abort();
                        return;
                    }

                    // Completion data received
                    OnFinalizationDataRetrieved(data);
                });
            }
        }

        public void OnFinalizationDataRetrieved(Dictionary<string, string> data) {
            if (!data.ContainsKey(MsfDictKeys.RoomId)) {
                throw new Exception("Game server finalized, but didn't include room id");
            }

            var roomId = int.Parse(data[MsfDictKeys.RoomId]);

            var password = data.ContainsKey(MsfDictKeys.RoomPassword)
                ? data[MsfDictKeys.RoomPassword]
                : "";

            Msf.Client.Rooms.GetAccess(roomId, password, (access, error) => {
                if (access == null) {
                    Msf.Events.Fire(Msf.EventNames.ShowDialogBox,
                            DialogBoxData.CreateInfo("Failed to get access to room: " + error));

                    Logs.Error("Failed to get access to room: " + error);

                    return;
                }

                OnRoomAccessReceived(access);
            });
        }

        public void OnRoomAccessReceived(RoomAccessPacket access) {
            // We're hoping that something will handle the Msf.Client.Rooms.AccessReceived event
            // (for example, SimpleAccessHandler)
        }

        public void Display(SpawnRequestController request) {
            if (this.request != null)
                this.request.StatusChanged -= OnStatusChange;

            if (request == null)
                return;

            request.StatusChanged += OnStatusChange;

            this.request = request;
            gameObject.SetActive(true);

            // Disable abort, and enable it after some time
            abortButton.interactable = false;
            StartCoroutine(EnableAbortDelayed(enableAbortAfterSeconds, request.SpawnId));

            if (statusText != null)
                statusText.text = pleaseWaitText;
        }

        private void OnDestroy() {
            if (request != null)
                request.StatusChanged -= OnStatusChange;
        }
    }
}