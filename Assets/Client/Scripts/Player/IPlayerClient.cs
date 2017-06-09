using Thieves.Share.PlayerNetworking;

namespace Thieves.Client.PlayerNetworking {
		public interface IPlayerClient {
				void OnSnapshot(PlayerSnapshot snapshot);
		}
}