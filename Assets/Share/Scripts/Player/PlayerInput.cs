using UnityEngine;

namespace Thieves.Share.PlayerController {
    public struct PlayerInput {
        public float timestamp;
        public Vector2 move;
        public Vector2 turn;
        public bool shoot;
        public bool holster;
        public bool stealth;
    }
}
