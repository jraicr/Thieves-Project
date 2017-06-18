using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Barebones.MasterServer;

namespace Thieves.Client.UI {
    /// <summary>
    /// Game creation window
    /// </summary>
    public class CreateGameUI : MonoBehaviour {
        public Dropdown map;

        public Image mapImage;

        public List<MapSelection> maps;
        public int maxNameLength = 14;
        public Dropdown maxPlayers;

        public int maxPlayersLowerLimit = 2;
        public int maxPlayersUpperLimit = 10;

        public int minNameLength = 3;

        public CreateGameProgressUI progressUI;
        public InputField roomName;

        public bool requireAuthentication = true;

        public bool setAsLastSiblingOnEnable = true;

        protected virtual void Awake() {
            progressUI = progressUI ?? FindObjectOfType<CreateGameProgressUI>();
            map.ClearOptions();
            map.AddOptions(maps.Select(m => new Dropdown.OptionData(m.Name)).ToList());

            OnMapChange();
        }

        public void OnEnable() {
            if (setAsLastSiblingOnEnable)
                transform.SetAsLastSibling();
        }

        public void OnCreateClick() {
            if (progressUI == null) {
                Logs.Error("You need to set a ProgressUi");
                return;
            }

            if (requireAuthentication && !Msf.Client.Auth.IsLoggedIn) {
                ShowError("You must be logged in to create a room");
                return;
            }

            var name = roomName.text.Trim();

            if (string.IsNullOrEmpty(name) || (name.Length < minNameLength) || (name.Length > maxNameLength)) {
                ShowError(string.Format("Invalid length of game name, shoul be between {0} and {1}", minNameLength,
                        maxNameLength));
                return;
            }

            var maxPlayers = 0;
            int.TryParse(this.maxPlayers.captionText.text, out maxPlayers);

            if ((maxPlayers < maxPlayersLowerLimit) || (maxPlayers > maxPlayersUpperLimit)) {
                ShowError(string.Format("Invalid number of max players. Value should be between {0} and {1}",
                        maxPlayersLowerLimit, maxPlayersUpperLimit));
                return;
            }

            var settings = new Dictionary<string, string>
            {
                                {MsfDictKeys.MaxPlayers, maxPlayers.ToString()},
                                {MsfDictKeys.RoomName, name},
                                {MsfDictKeys.MapName, GetSelectedMap().Name},
                                {MsfDictKeys.SceneName, GetSelectedMap().Scene.SceneName}
                        };

            Msf.Client.Spawners.RequestSpawn(settings, "", (requestController, errorMsg) => {
                if (requestController == null) {
                    progressUI.gameObject.SetActive(false);
                    Msf.Events.Fire(Msf.EventNames.ShowDialogBox,
                            DialogBoxData.CreateError("Failed to create a game: " + errorMsg));

                    Logs.Error("Failed to create a game: " + errorMsg);
                }

                progressUI.Display(requestController);
            });
        }

        public MapSelection GetSelectedMap() {
            var text = map.captionText.text;
            return maps.FirstOrDefault(m => m.Name == text);
        }

        public void OnMapChange() {
            var selected = GetSelectedMap();

            if (selected == null) {
                Logs.Error("Invalid map selection");
                return;
            }

            mapImage.sprite = selected.Sprite;
        }

        private void ShowError(string error) {
            Msf.Events.Fire(Msf.EventNames.ShowDialogBox, DialogBoxData.CreateError(error));
        }

        [Serializable]
        public class MapSelection {
            public string Name;
            public SceneField Scene;
            public Sprite Sprite;
        }
    }
}