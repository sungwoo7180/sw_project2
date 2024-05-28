using System.Collections.Generic;
using UnityEngine;

namespace UFE3D
{
    public class InstantiatedGameObject
    {
        public string id;
        public GameObject gameObject;
        public MrFusion mrFusion;
        public Dictionary<ParticleSystem, float> particles;
        public long creationFrame;
        public long? destructionFrame;

        public InstantiatedGameObject(
            string id = null,
            GameObject gameObject = null,
            MrFusion mrFusion = null,
            Dictionary<ParticleSystem, float> particleStorage = null,
            long creationFrame = 0,
            long? destructionFrame = null
        )
        {
            this.id = id;
            this.gameObject = gameObject;
            this.mrFusion = mrFusion;
            this.particles = particleStorage;
            this.creationFrame = creationFrame;
            this.destructionFrame = destructionFrame != null ? new long?(destructionFrame.Value) : null;
        }

        public InstantiatedGameObject(InstantiatedGameObject other) : this(
            other.id,
            other.gameObject,
            other.mrFusion,
            other.particles,
            other.creationFrame,
            other.destructionFrame
        )
        { }
    }
}