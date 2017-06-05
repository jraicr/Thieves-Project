using System.Collections.Generic;
using System.ComponentModel;
using Barebones.Networking;
using Barebones.Utils;
using UnityEngine;
using UnityEngine.UI;
using Barebones.MasterServer;

namespace Thieves.Client.UI {
    /// <summary>
    /// Represents a list of game servers
    /// </summary>
    public class GamesListUI : MonoBehaviour {
        private GenericUIList<GameInfoPacket> items;
        public GameObject createRoomWindow;

        public Button gameJoinButton;
        public GamesListUIItem itemPrefab;
        public LayoutGroup layoutGroup;

        protected IClientSocket connection = Msf.Connection;

        // Use this for initialization
        protected virtual void Awake() {
            items = new GenericUIList<GameInfoPacket>(itemPrefab.gameObject, layoutGroup);

            connection.Connected += OnConnectedToMaster;
        }

        protected virtual void HandleRoomsShowEvent(object arg1, object arg2) {
            gameObject.SetActive(true);
        }

        private void OnEnable() {
            if (connection.IsConnected)
                RequestRooms();
        }

        protected void OnConnectedToMaster() {
            // Get rooms, if at the time of connecting the lobby is visible
            if (gameObject.activeSelf)
                RequestRooms();
        }

        public void Setup(IEnumerable<GameInfoPacket> data) {
            items.Generate<GamesListUIItem>(data, (packet, item) => { item.Setup(packet); });
            UpdateGameJoinButton();
        }

        private void UpdateGameJoinButton() {
            gameJoinButton.interactable = GetSelectedItem() != null;
        }

        public GamesListUIItem GetSelectedItem() {
            return items.FindObject<GamesListUIItem>(item => item.isSelected);
        }

        public void Select(GamesListUIItem gamesListItem) {
            items.Iterate<GamesListUIItem>(item => { item.SetIsSelected(!item.isSelected && (gamesListItem == item)); });
            UpdateGameJoinButton();
        }

        public void OnRefreshClick() {
            RequestRooms();
        }

        public void OnJoinGameClick() {
            var selected = GetSelectedItem();

            if (selected == null)
                return;

            if (selected.isLobby) {
                OnJoinLobbyClick(selected.rawData);
                return;
            }

            if (selected.isPasswordProtected) {
                // If room is password protected
                var dialogData = DialogBoxData
                    .CreateTextInput("Room is password protected. Please enter the password to proceed", password => {
                        Msf.Client.Rooms.GetAccess(selected.gameId, OnPassReceived, password);
                    });

                if (!Msf.Events.Fire(Msf.EventNames.ShowDialogBox, dialogData)) {
                    Logs.Error("Tried to show an input to enter room password, " +
                               "but nothing handled the event: " + Msf.EventNames.ShowDialogBox);
                }

                return;
            }

            // Room does not require a password
            Msf.Client.Rooms.GetAccess(selected.gameId, OnPassReceived);
        }

        protected virtual void OnJoinLobbyClick(GameInfoPacket packet) {
            var loadingPromise = Msf.Events.FireWithPromise(Msf.EventNames.ShowLoading, "Joining lobby");

            Msf.Client.Lobbies.JoinLobby(packet.Id, (lobby, error) => {
                loadingPromise.Finish();

                if (lobby == null) {
                    Msf.Events.Fire(Msf.EventNames.ShowDialogBox, DialogBoxData.CreateError(error));
                    return;
                }

                // Hide this window
                gameObject.SetActive(false);

                var lobbyUi = FindObjectOfType<LobbyUi>();

                if (lobbyUi == null && MsfUi.Instance != null) {
                    // Try to get it through MsfUi
                    lobbyUi = MsfUi.Instance.LobbyUi;
                }

                if (lobbyUi == null) {
                    Logs.Error("Couldn't find appropriate UI element to display lobby data in the scene. " +
                               "Override OnJoinLobbyClick method, if you want to handle this differently");
                    return;
                }

                lobbyUi.gameObject.SetActive(true);
                lobby.SetListener(lobbyUi);
            });
        }

        protected virtual void OnPassReceived(RoomAccessPacket packet, string errorMessage) {
            if (packet == null) {
                Msf.Events.Fire(Msf.EventNames.ShowDialogBox, DialogBoxData.CreateError(errorMessage));
                Logs.Error(errorMessage);
                return;
            }

            // Hope something handles the event
        }

        protected virtual void RequestRooms() {
            if (!connection.IsConnected) {
                Logs.Error("Tried to request rooms, but no connection was set");
                return;
            }

            var loadingPromise = Msf.Events.FireWithPromise(Msf.EventNames.ShowLoading, "Retrieving Rooms list...");

            Msf.Client.Matchmaker.FindGames(games => {
                loadingPromise.Finish();

                Setup(games);
            });
        }

        public void OnCreateGameClick() {
            if (createRoomWindow == null) {
                Logs.Error("You need to set a CreateRoomWindow");
                return;
            }
            createRoomWindow.gameObject.SetActive(true);
        }

        public void OnCloseClick() {
            gameObject.SetActive(false);
        }

        void OnDestroy() {
            Msf.Connection.Connected -= OnConnectedToMaster;
        }
    }
}