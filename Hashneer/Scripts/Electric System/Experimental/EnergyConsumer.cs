using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hashneer.ElectricalSystem.Experimental
{
    /// <summary>
    /// Every item that requieres energy in any way needs to inherit from this class.
    /// </summary>
    public abstract class EnergyConsumer : MonoBehaviour
    {
        public float energyConsumptionRate = 0f;
        public float consumptionTime = 0f;
        public bool isActive = true;
        [HideInInspector] public bool isInList = false;

        protected virtual void ToggleActive()
        {
            isActive = !isActive;
        }
    }
}
