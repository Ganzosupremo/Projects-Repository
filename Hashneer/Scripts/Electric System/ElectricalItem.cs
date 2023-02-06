using UnityEngine;

namespace Hashneer.ElectricalSystem
{
    public class ElectricalItem : MonoBehaviour, IEnergyConsumer
    {
        public float EnergyConsumptionRate { get; set; } = 100f;
        public float EnergyConsumptionTime { get; set; } = 25f;

        private float reset;

        private void Start()
        {
            reset = EnergyConsumptionTime;
        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            EnergyConsumptionTime -= Time.deltaTime;

            if (EnergyConsumptionTime <= 0)
            {
                UseEnergy(EnergyConsumptionRate);
            }
        }

        public void UseEnergy(float consumptionRate)
        {
            ElectricalGridManager.Instance.UseEnergy(consumptionRate);
            EnergyConsumptionTime = reset;
        }
    }
}