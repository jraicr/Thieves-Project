using System.Collections;
using System.Collections.Generic;
using Thieves.Share.Room;
using Barebones.MasterServer;
using UnityEngine;


namespace Thieves.GameServer.Room {
    public class UnetGameTerminator : MonoBehaviour {
        public HelpBox _header = new HelpBox() {
            Text = "This script quits the application if some of the conditions are met. "
        };

        public Thieves.Share.Room.UnetGameRoom room;

        [Tooltip("Terminates server if first player doesn't join in a given number of seconds")]
        public float firstPlayerTimeoutSecs = 25;

        [Tooltip("Terminates if room is not registered in a given number of seconds")]
        public float roomRegistrationTimeoutSecs = 60;

        [Tooltip("Once every given number of seconds checks if the room is empty." +
                         " If it is - terminates it")]
        public float terminateEmptyOnIntervals = 120;

        [Tooltip("Each second, will check if connected to master. If not - quits the application")]
        public bool terminateIfNotConnected = true;

        [Tooltip("If true, quit the application immediately, when the last player quits")]
        public bool terminateWhenLastPlayerQuits = true;

        private bool hasFirstPlayerShowedUp = false;

        // Use this for initialization
        void Start() {

            if (!Msf.Args.IsProvided(Msf.Args.Names.SpawnCode)) {
                // If this game server was not spawned by a spawner
                Destroy(gameObject);
                return;
            }

            if (room == null) {
                Logs.Error("Room is not set");
                return;
            }

            room.playerLeft += OnPlayerLeft;
            room.playerJoined += OnPlayerJoined;

            if (roomRegistrationTimeoutSecs > 0)
                StartCoroutine(StartStartedTimeout(roomRegistrationTimeoutSecs));

            if (firstPlayerTimeoutSecs > 0)
                StartCoroutine(StartFirstPlayerTimeout(firstPlayerTimeoutSecs));

            if (terminateEmptyOnIntervals > 0)
                StartCoroutine(StartEmptyIntervalsCheck(terminateEmptyOnIntervals));

            if (terminateIfNotConnected)
                StartCoroutine(StartWaitingForConnectionLost());
        }

        private void OnPlayerJoined(UnetMsfPlayer obj) {
            hasFirstPlayerShowedUp = true;
        }

        private void OnPlayerLeft(UnetMsfPlayer obj) {
            if (terminateWhenLastPlayerQuits && room.GetPlayers().Count == 0) {
                Application.Quit();
            }
        }

        /// <summary>
        /// Each second checks if we're still connected, and if we are not,
        /// terminates game server.
        /// </summary>
        /// <returns></returns>
        private IEnumerator StartWaitingForConnectionLost() {
            // Wait at least 5 seconds until first check
            yield return new WaitForSeconds(5);

            while (true) {
                yield return new WaitForSeconds(1);
                if (!Msf.Connection.IsConnected) {
                    Logs.Error("Terminating game server, no connection");
                    Application.Quit();
                }
            }
        }

        /// <summary>
        /// Each time, after the amount of seconds provided passes, checks
        /// if the server is empty, and if it is - terminates application
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private IEnumerator StartEmptyIntervalsCheck(float timeout) {
            while (true) {
                yield return new WaitForSeconds(timeout);
                if (room == null || room.GetPlayers().Count <= 0) {
                    Logs.Error("Terminating game server because it's empty at the time of an interval check.");
                    Application.Quit();
                }
            }
        }

        /// <summary>
        /// Waits a number of seconds, and checks if the game room was registered
        /// If not - terminates the application
        /// </summary>
        /// <returns></returns>
        private IEnumerator StartStartedTimeout(float timeout) {
            yield return new WaitForSeconds(timeout);
            if (room == null || !room.IsRoomRegistered)
                Application.Quit();
        }

        private IEnumerator StartFirstPlayerTimeout(float timeout) {
            yield return new WaitForSeconds(timeout);
            if (!hasFirstPlayerShowedUp) {
                Logs.Error("Terminated game server because first player didn't show up");
                Application.Quit();
            }
        }
    }
}
