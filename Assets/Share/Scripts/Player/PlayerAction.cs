namespace Thieves.Share.PlayerController {
		public class PlayerAction {
				public bool shoot;
				public bool holster;
				public bool stealth;

				public PlayerAction() {
						this.shoot = false;
						this.holster = false;
						this.stealth = false;
				}

				public PlayerAction(bool shoot, bool holster, bool stealth) {
						this.shoot = shoot;
						this.holster = holster;
						this.stealth = stealth;
				}
		}
}