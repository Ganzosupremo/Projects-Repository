using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hashneer.BitcoinMining.Experimental
{
    public class Miner : MonoBehaviour
    {
        private int _iterations = 0;
        private long nonce;
        private bool blockFound;
        private Coroutine stopMiningRoutine;

        public string id;
        public bool miningActive = false;
        public float hashrate;

        private void Update()
        {
            Listen();
        }

        public void Init()
        {
            id = Guid.NewGuid().ToString();
            StartCoroutine(StartMining());
        }

        /// <summary>
        /// listen for new nonce values from the network
        /// and updates the currentNonce variable accordingly
        /// </summary>
        public void Listen()
        {
            nonce = Blockchain.Instance.currentNonce;
            blockFound = Blockchain.Instance.BlockFound;

            if (blockFound)
            {
                if (stopMiningRoutine != null)
                    StopCoroutine(stopMiningRoutine);
                stopMiningRoutine = StartCoroutine(StopMining());
            }
        }

        private IEnumerator StopMining()
        {
            miningActive = false;
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 2f));
            miningActive= true;
        }

        private IEnumerator StartMining()
        {
            miningActive = false;
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.2f, 1.5f));
            miningActive = true;
        }

        public Block Mine(string data)
        {
            if (!miningActive) return null;

            Block block = Blockchain.Instance.CreateBlock(data, nonce, id);

            // Calculate until the hash is valid or the max iterations have been reached
            while (!IsHashValid(block.Hash, Blockchain.Instance.Difficulty) && _iterations < Settings.maxHashIterations)
            {
                block.Hash = block.CalculateHash(nonce);
                Blockchain.Instance.SetBlockFound(false);
                _iterations++;
            }

            Blockchain.Instance.SetBlockFound(true);

            if (Blockchain.Instance.CheckBlock(block, nonce, id))
            {
                Blockchain.Instance.NextBlock();
                Wallet.Instance.Balance += block.BlockSubsidy;
                Wallet.Instance.UpdateBalance();
                return block;
            }
            else
                return null;
        }

        /// <summary>
        /// Calculates the hash of the block until the hash begins with a certain number of leading zeroes.
        /// Converts the hash to a hexadecimal string and then count the number of leading zeroes.
        /// </summary>
        /// <param name="hash">The hash of the block to verify</param>
        /// <param name="difficulty">Determines how hard it is to find a valid hash</param>
        /// <returns>True if a valid hash has been found</returns>
        private bool IsHashValid(byte[] hash, int difficulty)
        {
            //if (difficulty < 0) difficulty = 1;

            //string hexHash = BitConverter.ToString(hash).Replace("-", "");
            //int leadingZeroes = 0;

            //// Check the number of leading zeroes
            //for (int i = 0; i < hexHash.Length; i++)
            //{
            //    if (hexHash[i] == '0')
            //        leadingZeroes++;
            //    else
            //        break;
            //}

            //return leadingZeroes >= difficulty;

            var block = Blockchain.Instance.GetLatestBlock().CalculateHash(Blockchain.Instance.currentNonce);

            bool found = Blockchain.Instance.GetLatestBlock().Hash.SequenceEqual(block);
            Debug.Log(found);

            return found;
        }
    }
}



