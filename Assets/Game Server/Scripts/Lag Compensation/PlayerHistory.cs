using UnityEngine;

namespace Thieves.GameServer.PlayerNetworking {
    public class PlayerHistory : MonoBehaviour {
        PlayerHistoryPool pool;
        Transform[] history;
        int index;

        void Awake() {
            pool = FindObjectOfType<PlayerHistoryPool>();
            history = pool.Take();

            foreach (Transform h in history) {
                h.parent = transform;
            }
        }


        public void Record(Transform serverTransform) {
            Transform h = history[index];
            index = (index + 1) % history.Length;
            pool.Record(h, serverTransform);
        }
    }
}
