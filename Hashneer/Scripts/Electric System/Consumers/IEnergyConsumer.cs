namespace Hashneer.ElectricalSystem
{
    public interface IEnergyConsumer
    {
        public float EnergyConsumptionRate { get; set; }
        public float EnergyConsumptionTime { get; set; }
    }
}