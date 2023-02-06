using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hashneer.ElectricalSystem
{
    public class ElectricalGridManager : SingletonMonobehaviour<ElectricalGridManager>
    {
        public float EnergyLevels { get; private set; } = 100f;
        public float totalEnergyConsumption;

        private float energyGenerationRate = 0f;
        private List<IEnergyConsumer> energyConsumers = new();
        private List<IEnergySource> energySources = new();
        private readonly float maxCapacity = 1000f;

        protected override void Awake()
        {
            base.Awake();
            energyConsumers = FindAllEnergyConsumers();
            energySources = FindAllEnergySources();
        }

        private void Update()
        {
            if (EnergyLevels < maxCapacity)
                EnergyLevels += energyGenerationRate * Time.deltaTime;
            else
                EnergyLevels = maxCapacity;

            foreach (IEnergySource source in energySources)
            {
                if (source.IsActive)
                    source.GenerateEnergy();
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Update the list of energy sources and consumers
            energyConsumers = FindAllEnergyConsumers();
            energySources = FindAllEnergySources();
        }

        public void UpdateGenerationRate()
        {
            float totalEnergyGeneration = 0f;
            foreach (IEnergySource source in energySources)
            {
                totalEnergyGeneration += source.EnergyGenerationRate;
                //Debug.Log(energyGenerationRate);
            }
            energyGenerationRate = totalEnergyGeneration * Time.deltaTime;
        }

        public void UseEnergy(float consumptionRate)
        {
            if (EnergyLevels >= consumptionRate)
            {
                foreach (IEnergyConsumer consumer in energyConsumers)
                {
                    float totalConsumption = consumer.EnergyConsumptionRate / consumer.EnergyConsumptionTime * Time.deltaTime;
                    EnergyLevels -= totalConsumption;
                    totalEnergyConsumption = consumer.EnergyConsumptionRate;
                    //Debug.Log("Energy Used");
                }
            }
            else
            {
                // show a message indicating that the electrical grid needs to be recharged
                Debug.Log("Not Enough Energy!");
            }
        }

        private List<IEnergyConsumer> FindAllEnergyConsumers()
        {
            IEnumerable<IEnergyConsumer> energyConsumers = FindObjectsOfType<MonoBehaviour>().OfType<IEnergyConsumer>();
            return new List<IEnergyConsumer>(energyConsumers);
        }

        private List<IEnergySource> FindAllEnergySources()
        {
            IEnumerable<IEnergySource> energySources = FindObjectsOfType<MonoBehaviour>().OfType<IEnergySource>();
            return new List<IEnergySource>(energySources);
        }
    }
}