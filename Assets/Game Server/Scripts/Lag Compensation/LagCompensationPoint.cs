using UnityEngine;

namespace Thieves.GameServer.PlayerNetworking {
    public struct LagCompensationPoint {
        public int layer;
        public Vector3 position;

        static public LagCompensationPoint FromTimestamp(float timestamp, int numLayers, int numY, Vector3 objectPosition, Vector3 poolPosition, Vector3 delta) {
            int toInt = (int)Mathf.Round(timestamp * 50f);

            return new LagCompensationPoint {
                layer = 32 - numLayers + (toInt % numLayers),
                position = objectPosition + poolPosition + delta * (toInt % numY)
            };
        }
    }
}
