using UnityEngine;
using UnityEngine.UI;
using Barebones.MasterServer;

namespace Thieves.Client.UI {
    /// <summary>
    /// Represents a single row in the games list
    /// </summary>
    public class GamesListUIItem : MonoBehaviour {
        public GameInfoPacket rawData { get; protected set; }
        public Image bgImage;
        public Color defaultBgColor;
        public GamesListUI listView;
        public GameObject lockImage;
        public Text mapName;
        public Text gameName;
        public Text online;

        public Color selectedBgColor;

        public string unknownMapName = "Unknown";

        public int gameId { get; private set; }
        public bool isSelected { get; private set; }
        public bool isLobby { get; private set; }

        public bool isPasswordProtected {
            get { return rawData.IsPasswordProtected; }
        }

        // Use this for initialization
        private void Awake() {
            bgImage = GetComponent<Image>();
            defaultBgColor = bgImage.color;

            SetIsSelected(false);
        }

        public void SetIsSelected(bool isSelected) {
            this.isSelected = isSelected;
            bgImage.color = isSelected ? selectedBgColor : defaultBgColor;
        }

        public void Setup(GameInfoPacket data) {
            rawData = data;
            isLobby = data.Type == GameInfoType.Lobby;
            SetIsSelected(false);
            gameName.text = data.Name;
            gameId = data.Id;
            lockImage.SetActive(data.IsPasswordProtected);

            if (data.MaxPlayers > 0) {
                online.text = string.Format("{0}/{1}", data.OnlinePlayers, data.MaxPlayers);
            } else {
                online.text = data.OnlinePlayers.ToString();
            }

            mapName.text = data.Properties.ContainsKey(MsfDictKeys.MapName)
                ? data.Properties[MsfDictKeys.MapName] : unknownMapName;
        }

        public void OnClick() {
            listView.Select(this);
        }
    }
}