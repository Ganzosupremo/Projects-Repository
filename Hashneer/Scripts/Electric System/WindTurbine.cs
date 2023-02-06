using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hashneer.ElectricalSystem
{
    public class WindTurbine : MonoBehaviour, IEnergySource
    {
        public float EnergyGenerationRate { get; set; } = 50f;
        public float GenerationTime { get; set; } = 10f;
        public bool IsActive { get; set; } = true;

        private float reset;

        private void Start()
        {
            reset = GenerationTime;
        }

        private void FixedUpdate()
        {
            GenerationTime -= Time.deltaTime;
        }

        public void GenerateEnergy()
        {
            if (GenerationTime <= 0)
            {
                ElectricalGridManager.Instance.UpdateGenerationRate();
                GenerationTime = reset;
            }
        }

        public void ActivateSource()
        {
            IsActive = !IsActive;
        }
    }
}