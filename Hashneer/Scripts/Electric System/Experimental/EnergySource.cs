using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hashneer.ElectricalSystem.Experimental
{
    /// <summary>
    /// Every class that generates energy needs to inherit from this class.
    /// </summary>
    public abstract class EnergySource : MonoBehaviour
    {
        public float energyGenerationRate = 0f;
        public float generationTime = 0f;
        public bool isActive = true;
        [HideInInspector] public bool isInList = false;

        protected virtual void ToggleActive()
        {
            isActive = !isActive;
        }

        protected void ProduceEnergy()
        {
            StaticEventHandler.CallEnergyProducedEvent(energyGenerationRate, this);
        }
    }
}
