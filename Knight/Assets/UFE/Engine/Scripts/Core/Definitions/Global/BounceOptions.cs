using UnityEngine;
using FPLibrary;

namespace UFE3D
{
    [System.Serializable]
    public class BounceOptions
    {
        public Sizes bounceForce;
        public GameObject bouncePrefab;
        public float bounceKillTime = 2;
        public Fix64 _minimumBounceForce;
        public int _maximumBounces = 3;
        public bool sticky = false;
        public bool bounceHitBoxes = true;
        public bool shakeCamOnBounce = true;
        public Fix64 _shakeDensity;
        public AudioClip bounceSound;
    }
}