using UnityEngine;

namespace Thieves.Share.PlayerNetworking {
    public struct PlayerState {
        public float timestamp;
        public int moveNum;
        public Vector3 position;
        public Vector2 turn;
        public bool holster;
        public float nextBullet;
        public int hitpoints;

        static public PlayerState CreateStartingState() {
            return new PlayerState {
                timestamp = Time.fixedTime,
                moveNum = 0,
                position = new Vector3(0, 0, 0),
                turn = Vector3.forward,
                holster = true,
                nextBullet = 0f,
                hitpoints = -1
            };
        }

        static public PlayerState CreateStartingState(Vector3 position, Vector2 turn, int startingHealth) {
            return new PlayerState {
                timestamp = Time.fixedTime,
                moveNum = 0,
                position = position,
                turn = turn,
                holster = true,
                nextBullet = 0f,
                hitpoints = startingHealth
            };
        }

        static public bool AreSimilar(PlayerState x, PlayerState y) {
            return (x.position == y.position) && (x.nextBullet == y.nextBullet) && (x.hitpoints == y.hitpoints);
        }
    }
}