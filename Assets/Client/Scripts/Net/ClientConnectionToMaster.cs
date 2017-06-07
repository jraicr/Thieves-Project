using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Barebones.Networking;
using Barebones.MasterServer;

namespace Thieves.Client.Network {
		public class ClientConnectionToMaster : ConnectionToMaster {


				new public void Start() {
						base.Start();
				}

				public override IClientSocket GetConnection() {
						return base.GetConnection();
				}

				public void OnLevelWasLoaded(int level) {
																		
				}

		}
}
