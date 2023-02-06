using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hashneer.ElectricalSystem.Experimental
{
    public class Item : EnergyConsumer
    {
        private float reset;
        private void Start()
        {
            reset = consumptionTime;
        }

        private void FixedUpdate()
        {
            consumptionTime -= Time.deltaTime;

            if (consumptionTime <= 0f)
            {
                StaticEventHandler.CallEnergyConsumedEvent(energyConsumptionRate, this);
                consumptionTime = reset;
            }
        }
    }
}
