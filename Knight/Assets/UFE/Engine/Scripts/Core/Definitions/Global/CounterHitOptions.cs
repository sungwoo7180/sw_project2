using UnityEngine;
using FPLibrary;

namespace UFE3D
{
    [System.Serializable]
    public class CounterHitOptions
    {
        public bool startUpFrames = true;
        public bool activeFrames = false;
        public bool recoveryFrames = false;
        public Fix64 _damageIncrease = 10;
        public Fix64 _hitStunIncrease = 50;
        public AudioClip sound;
    }
}