using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hashneer.BitcoinMining.Experimental
{
    //At the current state of the blockchain class, if I add more miners, more blocks are mined simultaneously along with the number of miners, but I want to make that no matter how many miners are active, just one block will be mined at a time, and then the next one and so on, can you implement it?
    public class Blockchain : SingletonMonobehaviour<Blockchain>
    {
        private List<Block> m_blocks;
        public List<Block> BlockChain { get => m_blocks; set => m_blocks = value; }
        public bool BlockFound { get; private set; }

        // Difficulty Adjustment Algrorithm
        public int Difficulty { get; private set; }
        private int m_minersCount;
        private float movingAverageBlockTime = 0f;
        /// <summary>
        /// Number of blocks to include in the moving average
        /// </summary>
        private int windowsSize = 10; // 
        private List<double> blockTimes = new();
        public long currentNonce = 0;

        // Block Reward
        public double BlockReward { get; set; }
        public int BlockCounter { get; set; }

        protected override void Awake()
        {
            base.Awake();
        }

        public void Init(int minersCount)
        {
            m_minersCount = minersCount;
            InitializeChain();
            AddGenesisBlock();
        }

        private void InitializeChain()
        {
            m_blocks = new List<Block>();
            Difficulty = 2;
            BlockCounter = 0;
            BlockReward = 50;
            currentNonce = 21;
        }

        private void AddGenesisBlock()
        {
            BlockChain.Add(new Block(DateTime.UtcNow, 
                new byte[] {0x00}, "Genesis Block", BlockReward, currentNonce, "Genesis"));
        }


        public Block CreateBlock(string data, long nonce, string minerID)
        {
            BlockFound = true;
            AdjustDifficulty();
            
            // Cut the reward in half if the target blocks have been reached
            BlockCounter++;
            if (BlockCounter % Settings.blockRewardCutoff == 0)
                BlockReward /= 2;

            // Create a new block and add it to the chain
            Block block = new(DateTime.UtcNow, GetLatestBlock()?.Hash, data, BlockReward, nonce, minerID)
            {
                BlockHeigth = BlockCounter,
            };

            BlockChain.Add(block);
            UpdateAverageBlockTime();

            BlockFound = false;

            return block;
        }

        public Block GetLatestBlock()
        {
            return BlockChain.LastOrDefault();
        }

        /// <summary>
        /// Checks the time passed since the last block was mined, and compares it to the average block time. 
        /// If the time passed is less than 90% of the average block time, the difficulty is incremented. 
        /// If the time passed is more than 110% of the average block time, the difficulty is decremented.
        /// </summary>
        private void AdjustDifficulty()
        {
            if (BlockChain.Count > 1)
            {
                var timeSinceLastBlock = DateTime.UtcNow - BlockChain[^2].Timestamp;

                if (timeSinceLastBlock < TimeSpan.FromSeconds(movingAverageBlockTime * 0.9))
                {
                    if (Difficulty < Settings.maxDifficulty)
                        Difficulty++;
                    currentNonce++;
                }
                else if(timeSinceLastBlock > TimeSpan.FromSeconds(movingAverageBlockTime * 1.1))
                {
                    if (Difficulty > Settings.minDifficulty)
                        Difficulty--;
                    currentNonce--;
                }
            }
            else
            {
                Difficulty = Mathf.CeilToInt(m_minersCount / 2);
            }

        }

        /// <summary>
        /// Calculates the time passed between the current block and the previous block 
        /// and uses it to update the average block time.
        /// </summary>
        private void UpdateAverageBlockTime()
        {
            if (BlockChain.Count > 1)
            {
                var blockTime = (DateTime.UtcNow - BlockChain[^2].Timestamp).TotalSeconds;
                blockTimes.Add(blockTime);

                if (blockTimes.Count > windowsSize)
                    blockTimes.RemoveAt(0);

                movingAverageBlockTime = (float)blockTimes.Average();
            }
        }

        /// <summary>
        /// Verify who mined the block
        /// </summary>
        public bool CheckBlock(Block block, long nonce, string minerId)
        {
            if (block.Nonce != nonce)
                return false;

            // other validation checks
            if (/*block.Hash == block.CalculateHash(nonce) &&*/ block.MinerID == minerId)
            {
                BlockChain.Add(block);
                // Broadcast the block, nonce, minerId to the network
                currentNonce++;
                BlockFound = true;
                // reward the miner
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Broadcast the new nonce to the network
        /// </summary>
        public void NextBlock()
        {
            BlockFound = false;
            currentNonce++;
        }

        public void SetBlockFound(bool found)
        {
            BlockFound = found;
        }
    }
}

