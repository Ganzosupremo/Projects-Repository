using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hashneer.ElectricalSystem.Experimental
{
    public class NuclearReactor : EnergySource
    {
        private float reset;

        private void Awake()
        {
            reset = generationTime;
        }

        private void FixedUpdate()
        {
            if (isActive) generationTime -= Time.deltaTime;

            if (isActive && generationTime <= 0f)
            {
                ProduceEnergy();
                generationTime = reset;
            }
        }
    }
}
