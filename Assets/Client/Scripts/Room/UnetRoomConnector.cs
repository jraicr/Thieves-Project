using System;
using System.Collections;
using Barebones.Logging;
using Barebones.MasterServer;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;

namespace Thieves.Share.Room {
    public class UnetRoomConnector : RoomConnector {
        public HelpBox _header = new HelpBox() {
            Text = "This script handles room access, and tries to connect to Unet HLAPI game server " +
                   "(by using Network Manager). It will be used when client receives an access to game.",
            Height = 50
        };

        /// <summary>
        ///     Log level of connector
        /// </summary>
        public LogLevel logLevel = LogLevel.Warn;

        public NetworkManager networkManager;

        protected Coroutine waitConnectionCoroutine;
        public BmLogger logger = Msf.Create.Logger(typeof(UnetRoomConnector).Name);

        public static RoomAccessPacket roomAccess;

        [Tooltip("If we can't connect in the given amount of time, it will be considered a failed attempt to connect")]
        public float connectionTimeout = 5f;

        public bool switchScenesIfWrongScene = true;

        public SceneField connectionFailedScene;
        public SceneField disconnectedScene;

        protected override void Awake() {
            base.Awake();
            logger.LogLevel = logLevel;
            networkManager = networkManager ?? FindObjectOfType<NetworkManager>();
        }

        protected virtual void Start() {
            // If we currently have a room access 
            // (it might have been set in a previous scene)
            if (roomAccess != null) {
                if (SceneManager.GetActiveScene().name == roomAccess.SceneName) {
                    // If we're atthe correct scene
                    ConnectToGame(roomAccess);
                } else if (switchScenesIfWrongScene) {
                    // Switch to correct scene 
                    SceneManager.LoadScene(roomAccess.SceneName);
                }
            }
        }

        public override void ConnectToGame(RoomAccessPacket access) {
            if (switchScenesIfWrongScene
                && SceneManager.GetActiveScene().name != access.SceneName) {
                // Save the access
                roomAccess = access;

                // Switch to correct scene 
                SceneManager.LoadScene(access.SceneName);
                return;
            }

            networkManager = networkManager ?? FindObjectOfType<NetworkManager>();

            // Remove the data after 
            roomAccess = null;

            // Just in case
            networkManager.maxConnections = 999;

            logger.Debug("Trying to connect to server at address: " + access.RoomIp + ":" + access.RoomPort);

            if (!networkManager.IsClientConnected()) {
                // If we're not connected already
                networkManager.networkAddress = access.RoomIp;
                networkManager.networkPort = access.RoomPort;
                networkManager.StartClient();
            }

            if (waitConnectionCoroutine != null)
                StopCoroutine(waitConnectionCoroutine);

            waitConnectionCoroutine = StartCoroutine(WaitForConnection(access));
        }

        protected virtual void OnFailedToConnect() {
            if (connectionFailedScene != null)
                SceneManager.LoadScene(disconnectedScene.SceneName);
        }

        public IEnumerator WaitForConnection(RoomAccessPacket access) {
            networkManager = networkManager ?? FindObjectOfType<NetworkManager>();

            logger.Debug("Connecting to game server... " + networkManager.networkAddress + ":" +
                         networkManager.networkPort);

            var timeUntilTimeout = connectionTimeout;

            // Wait until we connect
            while (!networkManager.IsClientConnected()) {
                yield return null;
                timeUntilTimeout -= Time.deltaTime;

                if (timeUntilTimeout < 0) {
                    logger.Warn("Client failed to connect to game server: " + access);
                    OnFailedToConnect();
                    yield break;
                }
            }

            logger.Info("Connected to game server, about to send access");

            // Connected, send the token
            networkManager.client.connection.Send(Thieves.Share.Room.UnetGameRoom.accessMsgType, new StringMessage(access.Token));

            // While connected
            while (networkManager.IsClientConnected())
                yield return null;

            // At this point, we're no longer connected
            if (disconnectedScene.IsSet())
                SceneManager.LoadScene(disconnectedScene.SceneName);
        }
    }
}