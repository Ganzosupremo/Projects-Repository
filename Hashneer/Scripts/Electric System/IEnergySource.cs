namespace Hashneer.ElectricalSystem
{
    public interface IEnergySource
    {
        public float EnergyGenerationRate { get; set; }
        public float GenerationTime { get; set; }
        public bool IsActive { get; set; }

        public void ActivateSource();

        public void GenerateEnergy();
    }
}