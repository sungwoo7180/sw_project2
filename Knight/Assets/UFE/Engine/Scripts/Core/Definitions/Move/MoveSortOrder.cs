using UnityEngine;
using System;

namespace UFE3D
{
    [Serializable]
    public class MoveSortOrder : ICloneable
    {
        public int castingFrame;
        public int value;

        [HideInInspector] public bool editorToggle = false;

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}