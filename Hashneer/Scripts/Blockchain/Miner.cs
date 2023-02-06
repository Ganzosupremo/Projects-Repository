using Hashneer.ElectricalSystem;
using Hashneer.ElectricalSystem.Experimental;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace Hashneer.BitcoinMining
{
    public class Miner : EnergyConsumer
    {
        public string minerName;
        public float hashrate = 69;
        public float difficulty;
        public TextMeshProUGUI latestBlock;
        public TextMeshProUGUI nonce;

        private Coroutine mineCoroutine;
        private float reset;

        // Start is called before the first frame update
        private void Start()
        {
            difficulty = Blockchain.Instance.Difficulty;
            reset = consumptionTime;
            Blockchain.Instance.GetActiveMiners().Add(this);
        }

        private void FixedUpdate()
        {
            if (isActive)
                consumptionTime -= Time.deltaTime;

            if (consumptionTime <= 0f)
                UseEnergy();
        }

        private void OnEnable()
        {
            StaticEventHandler.OnMineBlock += StaticEventHandler_OnMineBlock;
        }

        private void OnDisable()
        {
            StaticEventHandler.OnMineBlock -= StaticEventHandler_OnMineBlock;
        }

        private void StaticEventHandler_OnMineBlock(MineBlockArgs mineBlockArgs)
        {
            if (mineCoroutine != null)
                StopMining();

            mineCoroutine = StartCoroutine(ConstructBlock(mineBlockArgs));
        }

        private IEnumerator ConstructBlock(MineBlockArgs mineBlockArgs)
        {
            if (!isActive) yield break;

            Block newBlock;
            while (true)
            {
                if (!Blockchain.Instance.IsMining)
                {
                    float miningTime = CalculateMiningTime(mineBlockArgs.difficulty);
                    difficulty = mineBlockArgs.difficulty;

                    newBlock = new(minerName, Blockchain.Instance.GetLatestBlock()?.Hash, DateTime.UtcNow);

                    yield return new WaitForSeconds(miningTime);
                    Blockchain.Instance.MineBlock(newBlock);

                    //Debug.Log($"Block Found By: {minerName}.");

                    // Update the UI
                    latestBlock.text = Blockchain.Instance.GetLatestBlock()?.ToString();
                    nonce.text = Blockchain.Instance.Difficulty.ToString();
                    break;
                }
                else
                {
                    //Debug.Log($"The network is currently mining a block...");
                    newBlock = null;
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        /// <summary>
        /// Consumes energy from the <seealso cref="ElectricGridManager"/>. If there is
        /// not enough energy mining will halt.
        /// </summary>
        private void UseEnergy()
        {
            StaticEventHandler.CallEnergyConsumedEvent(energyConsumptionRate, this);
            consumptionTime = reset;
        }

        /// <summary>
        /// Calculate the mining time based
        /// on the network difficulty and miner hashrate
        /// </summary>
        private float CalculateMiningTime(float difficulty)
        {
            // The higher the difficulty, the more hashes need to be calculated,
            // so the mining time will be longer
            return difficulty / hashrate;
        }

        private void StopMining()
        {
            if (mineCoroutine != null)
                StopCoroutine(mineCoroutine);
        }
    }
}