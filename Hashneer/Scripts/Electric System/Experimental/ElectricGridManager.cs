using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hashneer.ElectricalSystem.Experimental
{
    public class ElectricGridManager : SingletonMonobehaviour<ElectricGridManager>
    {
        /// <summary>
        /// This energy get consume when the UseEnergy() is called.
        /// </summary>
        public float EnergyLevels { get; private set; }
        public float TotalEnergyGeneration { get; private set; }
        public float TotalEnergyConsumption { get; private set; }
        
        private int maxEnergyStorage = 150;
        /// <summary>
        /// Not using this list for anything for the moment,
        /// but just in case just have it there
        /// </summary>
        private List<EnergySource> energySources = new();
        /// <summary>
        /// Not using this list for anything for the moment,
        /// but just in case just have it there
        /// </summary>
        private List<EnergyConsumer> energyConsumers = new();

        protected override void Awake()
        {
            base.Awake();
        }

        private void OnEnable()
        {
            StaticEventHandler.OnEnergyProduced += StaticEventHandler_OnEnergyProduced;
            StaticEventHandler.OnEnergyConsumed += StaticEventHandler_OnEnergyConsumed;
        }

        private void OnDisable()
        {
            StaticEventHandler.OnEnergyProduced -= StaticEventHandler_OnEnergyProduced;
            StaticEventHandler.OnEnergyConsumed -= StaticEventHandler_OnEnergyConsumed;
        }

        private void StaticEventHandler_OnEnergyProduced(EnergyProducedEventArgs energyProducedArgs)
        {
            UpdateEnergyGeneration(energyProducedArgs);
        }

        private void StaticEventHandler_OnEnergyConsumed(EnergyConsumedEventArgs energyConsumedEventArgs)
        {
            UseEnergy(energyConsumedEventArgs);
        }

        /// <summary>
        /// WIP - Updates how many energy is been generated.
        /// Uses a static event to get the total energy produced from all the energy sources
        /// </summary>
        /// <param name="energyGenerationRate"></param>
        /// <param name="generationTime"></param>
        private void UpdateEnergyGeneration(EnergyProducedEventArgs energyProducedArgs)
        {
            float energyProduced = 0f;

            foreach (EnergySource source in energySources)
            {
                energyProduced += Mathf.Abs(energyProducedArgs.producedEnergy * 0.02f);
            }

            if (!energyProducedArgs.energySource.isInList)
            {
                energySources.Add(energyProducedArgs.energySource);
                energyProducedArgs.energySource.isInList = true;
            }

            TotalEnergyGeneration = energyProduced;

            if (EnergyLevels < maxEnergyStorage)
                EnergyLevels += TotalEnergyGeneration;
            else
                EnergyLevels = maxEnergyStorage;
        }

        /// <summary>
        /// WIP - Uses the energy stored on the EnergyLevels.
        /// Uses an event to calculate the total consumption and deactivates the consumer
        /// if not enough energy is being produced.
        /// </summary>
        private void UseEnergy(EnergyConsumedEventArgs energyConsumedEventArgs)
        {
            float energyConsumed = 0f;

            // Sums up all the total comsumption
            foreach (EnergyConsumer consumer in energyConsumers)
            {
                energyConsumed += Mathf.Abs(energyConsumedEventArgs.energyNeeded * 0.02f);
            }

            // Add the consumer to the list
            if (!energyConsumedEventArgs.consumer.isInList)
            {
                energyConsumers.Add(energyConsumedEventArgs.consumer);
                energyConsumedEventArgs.consumer.isInList = true;
            }

            // Reactivate a consumer if it became inactive because of lack of enough energy
            if (!energyConsumedEventArgs.consumer.isActive && TotalEnergyGeneration > TotalEnergyConsumption)
                energyConsumedEventArgs.consumer.isActive = true;

            TotalEnergyConsumption = energyConsumed;

            if (energyConsumed < EnergyLevels)
            {
                EnergyLevels -= energyConsumed;
                //foreach (var consumer in energyConsumers)
                //{
                //    consumer.isActive = true;
                //}
                energyConsumedEventArgs.consumer.isActive = true;
            }
            else if (TotalEnergyGeneration == TotalEnergyConsumption)
            {
                //foreach (var consumer in energyConsumers)
                //{
                //    consumer.isActive = true;
                //}
                energyConsumedEventArgs.consumer.isActive = true;
                Debug.Log("Grid at max capacity! Expand the energy generation!");
            }
            else if (TotalEnergyGeneration < TotalEnergyConsumption)
            {
                //foreach (var consumer in energyConsumers)
                //{
                //    consumer.isActive = true;
                //}
                energyConsumedEventArgs.consumer.isActive = false;
                Debug.Log("Not Producing Enough Energy! Ramp Up Production!");
            }
            else
            {
                foreach (var consumer in energyConsumers)
                {
                    consumer.isActive = false;
                }
            }
        }

        public void UpgradeEnergyCapacity(int newCapacity)
        {
            maxEnergyStorage += newCapacity;
        }
    }
}
