namespace Thieves.Share.PlayerController {
		public class PlayerAction {
				public bool shoot;
				public bool stealth;

				public PlayerAction() {
						this.shoot = false;
						this.stealth = false;
				}

				public PlayerAction(bool shoot, bool stealth) {
						this.shoot = shoot;
						this.stealth = stealth;
				}
		}
}