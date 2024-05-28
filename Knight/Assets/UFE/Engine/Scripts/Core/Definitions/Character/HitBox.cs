using UnityEngine;
using System;
using FPLibrary;

namespace UFE3D
{
    [Serializable]
    public class HitBox : ICloneable
    {
        public Rect rect = new Rect(0, 0, 4, 4); // TODO remove this variable

        public bool followXBounds;
        public bool followYBounds;

        public Transform position;

        #region trackable definitions
        public bool hitState;
        public bool hide; // Whether the hit box collisions will be detected

        public bool defaultVisibility = true; // Whether the GameObject will be active in the hierarchy
        public BodyPart bodyPart;
        public FPVector mappedPosition;
        public FPVector localPosition;
        public HitBoxShape shape;
        public CollisionType collisionType;
        public HitBoxType type;
        public FPRect _rect = new FPRect();
        public Fix64 _radius = .5;
        public FPVector _offSet;
        #endregion

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}