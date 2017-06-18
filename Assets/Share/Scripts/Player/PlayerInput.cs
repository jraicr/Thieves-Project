using UnityEngine;

namespace Thieves.Share.PlayerController {
    public struct PlayerInput {
        public float timestamp;
        public sbyte x;
        public sbyte y;
        public Vector2 turn;
        public bool shoot;
        public bool holster;
        public bool stealth;

        public sbyte CompressFloat(float n) {
            return (sbyte)(n * sbyte.MaxValue);
        }

        public Vector2 UncompressVector2(sbyte x, sbyte y) {
            return new Vector2((float)x / sbyte.MaxValue, (float)y / sbyte.MaxValue);
        }
    }
}
