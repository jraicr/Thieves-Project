using System.Collections;
using System.Collections.Generic;
using Barebones.MasterServer;
using UnityEngine;

namespace Thieves.GameServer.Lobby {
    public class FactoriesSetup : MonoBehaviour {
        public HelpBox Header = new HelpBox() {
            Text = "This script adds lobby factories to the module " +
                   "(1vs1, 2vs2, 4vs4)"
        };

        void Start() {
            // Add lobby factories

            var module = FindObjectOfType<LobbiesModule>();

            module.AddFactory(new LobbyFactoryAnonymous("1 vs 1",
                module, DemoLobbyFactories.OneVsOne));

            module.AddFactory(new LobbyFactoryAnonymous("2 vs 2",
                module, DemoLobbyFactories.TwoVsTwoVsFour));

            module.AddFactory(new LobbyFactoryAnonymous("4 vs 4",
                module, DemoLobbyFactories.ThreeVsThreeQueue));
        }
    }
}