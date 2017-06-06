using System.Collections;
using System.Collections.Generic;
using Barebones.MasterServer;
using Barebones.Networking;
using UnityEngine;

namespace Thieves.GameServer.Profiles {

		/// <summary>
		/// Sets up profiles module
		/// </summary>
		public class PlayerProfiles : ServerModuleBehaviour {
				public const int coinsKey = 777;
				public const int weaponKey = 778;

				void Awake() {
						// Request for profiles module
						AddOptionalDependency<ProfilesModule>();
				}

				public override void Initialize(IServer server) {
						base.Initialize(server);

						var profilesModule = server.GetModule<ProfilesModule>();

						if (profilesModule == null)
								return;

						// Set the new factory
						profilesModule.ProfileFactory = CreateProfileInServer;

						Logs.Warn("Created player profile factory");
				}

				/// <summary>
				/// This method generates a "scheme" for profile on the server side
				/// </summary>
				/// <param name="username"></param>
				/// <param name="peer"></param>
				/// <returns></returns>
				public static ObservableServerProfile CreateProfileInServer(string username, IPeer peer) {
						return new ObservableServerProfile(username, peer) {

            // Start with 10 coins by default
            new ObservableInt(coinsKey, 10),
						new ObservableString(weaponKey, "Carrot")
						};
				}

				public static ObservableServerProfile CreateProfileInServer(string username) {
						return CreateProfileInServer(username, null);
				}
		}
}