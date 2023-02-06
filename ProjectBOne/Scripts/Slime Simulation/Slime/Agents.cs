using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SlimeSimulation
{
    [System.Serializable]
    public struct Agent
    {
        public Vector2 position;
        public float angle;
        public Vector3Int speciesMask;
        int unusedSpeciesChannel;
        public int speciesIndex;
    }
}

