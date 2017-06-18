using UnityEngine;

namespace Thieves.Share.Utils {
    public class MonotonicTime : MonoBehaviour {
        public float interpolationDelay = 0.2f;
        public float minDuration = 5f;
        public float durationMultiplier = 2f;
        public int maxSamples = 50;

        Vector2[] samples;
        Vector2 from;
        Vector2 to;
        int sampleIndex;
        int samplesTaken;

        void Awake() {
            samples = new Vector2[maxSamples];
        }

        public float GetTime() {
            float afterTo = Time.fixedTime - to.x;
            return Mathf.Round(((afterTo < 0f) ? Mathf.Lerp(from.y, to.y, Mathf.InverseLerp(from.x, to.x, Time.fixedTime)) : to.y + afterTo) * 50f) * 0.02f;
        }

        public void SetTime(float serverTime) {
            serverTime -= interpolationDelay;
            samples[sampleIndex++] = new Vector2(Time.fixedTime, serverTime);
            sampleIndex %= maxSamples;
            if ((samplesTaken++) == 0) {
                from = samples[0];
                to = from;
                to.x += 1f;
                to.y += 1f;
                return;
            }
            if (samplesTaken > maxSamples) {
                samplesTaken = maxSamples;
            }
            from = new Vector2(Time.fixedTime, GetTime());
            float duration = minDuration;
            while (true) {
                float toClientTime = Time.fixedTime + duration;
                float toServerTime = 0f;
                for (int i = 0; i < samplesTaken; i++) {
                    toServerTime += samples[i].y + toClientTime - samples[i].x;
                }
                toServerTime /= samplesTaken;
                if (toServerTime > from.y) {
                    to = new Vector2(toClientTime, toServerTime);
                    return;
                }
                duration *= durationMultiplier;
            }
        }
    }
}
