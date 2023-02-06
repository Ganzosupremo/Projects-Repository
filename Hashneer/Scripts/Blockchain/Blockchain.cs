using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Hashneer.BitcoinMining
{
    public class Blockchain : SingletonMonobehaviour<Blockchain>
    {
        public List<Block> Blocks { get; private set; }
        public float Difficulty { get; private set; } = 50f;
        public bool IsMining { get; private set; }
        /// <summary>
        /// The nonce does not have anything to do with how the blocks are
        ///  mined, is just there to be more like a real blockchain
        /// </summary>
        public int Nonce { get; private set; } = 2;

        private List<Miner> activeMiners = new();
        private double movingAverageBlockTime = 0f;
        /// <summary>
        /// This determines how many times can be on the blockTimes list at any given time,
        /// if the Count is greather than this, then the last item gets remove from the list. 
        /// </summary>
        private readonly int windowsSize = 10;
        private readonly List<double> blockTimes = new();

        protected override void Awake()
        {
            base.Awake();
            CreateGenesis();
            Nonce = Random.Range(0, int.MaxValue);
            //Blocks = new() { CreateGenesisBlock() };
        }

        //private void Start()
        //{
        //    InvokeRepeating(nameof(CheckMiners), 60f, 60f);
        //}

        private void CreateGenesis()
        {
            Blocks = new();
            Block genesis = new("Genesis Block", new byte[] { 0x0 }, DateTime.UtcNow);
            StaticEventHandler.CallMineBlockEvent(genesis, Difficulty);
            Blocks.Add(genesis);
        }

        public void MineBlock(string address)
        {
            if (IsMining) return;

            IsMining = true;
            AdjustDifficulty();
            Block newBlock = new(address, GetLatestBlock()?.Hash, DateTime.UtcNow);
            
            newBlock.MineBlock(Difficulty);
            newBlock.PreviousHash = Blocks.LastOrDefault().Hash;
            newBlock.Nonce = Nonce;
            
            Blocks.Add(newBlock);
            IsMining = false;
            StaticEventHandler.CallMineBlockEvent(newBlock, Difficulty);
            UpdateAverageBlockTime();
        }

        public void MineBlock(Block newBlock)
        {
            if (IsMining) return;

            IsMining = true;
            AdjustDifficulty();

            newBlock.MineBlock(Difficulty);
            newBlock.PreviousHash = Blocks.LastOrDefault()?.Hash;
            newBlock.Nonce = Nonce;
            Blocks.Add(newBlock);

            StaticEventHandler.CallMineBlockEvent(newBlock, Difficulty);
            IsMining = false;
            UpdateAverageBlockTime();
            //CheckMiners();
        }

        /// <summary>
        /// Checks the time passed since the last block was mined, and compares it to the average block time. 
        /// If the time passed is less than 25% of the average block time, the difficulty is incremented. 
        /// If the time passed is more than 175% of the average block time, the difficulty is decremented.
        /// </summary>
        private void AdjustDifficulty()
        {
            if (Blocks.Count > 1)
            {
                var timeSinceLastBlock = DateTime.UtcNow - Blocks[^2].Timestamp;

                if (timeSinceLastBlock < TimeSpan.FromSeconds(movingAverageBlockTime * 0.25))
                {
                    if (Difficulty < Settings.maxDifficulty)
                        Difficulty += 2;
                }

                if (timeSinceLastBlock > TimeSpan.FromSeconds(movingAverageBlockTime * 1.75))
                {
                    if (Difficulty > Settings.minDifficulty)
                        Difficulty--;
                }
            }
            Nonce = RecalculateNonce();
        }

        /// <summary>
        /// Calculates the time passed between the current block and the previous block 
        /// and uses it to update the average block time.
        /// </summary>
        private void UpdateAverageBlockTime()
        {
            if (Blocks.Count > 1)
            {
                var blockTime = (DateTime.UtcNow - Blocks[^2].Timestamp).TotalSeconds;
                blockTimes.Add((float)blockTime);

                // Remove the last block times if the Count is greather
                // than the windows size
                if (blockTimes.Count > windowsSize)
                    blockTimes.RemoveAt(0);

                movingAverageBlockTime = blockTimes.Average();
            }
        }

        /// <summary>
        /// WIP - this method should check the active miners, and of no miners are active
        /// the system mines new blocks, because the miners dont keep mining after they became active again.
        /// Probably is because the <seealso cref="StaticEventHandler.CallMineBlockEvent(Block, float)"/> just publish
        /// the event every time a new block is mined, but if no blocks are mined, means no events get published.
        /// </summary>
        private void CheckMiners()
        {
            if (activeMiners.Count != 0)
            {
                foreach (Miner miner in activeMiners)
                {
                    if (!miner.isActive)
                        activeMiners.Remove(miner);
                }
            }
            // The system mines new blocks so the chain keeps going
            if (activeMiners.Count == 0)
            {
                Block systemBlock = new("System", GetLatestBlock().Hash, DateTime.UtcNow);
                MineBlock(systemBlock);
                StaticEventHandler.CallMineBlockEvent(systemBlock, Difficulty);
            }
        }

        /// <summary>
        /// Recalculates the nonce of the network
        /// </summary>
        private int RecalculateNonce()
        {
            return Random.Range(0, int.MaxValue);
        }

        public Block GetLatestBlock()
        {
            return Blocks.LastOrDefault();
        }

        public List<Block> GetBlockList()
        {
            return Blocks;
        }

        public List<Miner> GetActiveMiners()
        {
            return activeMiners;
        }
    }
}
