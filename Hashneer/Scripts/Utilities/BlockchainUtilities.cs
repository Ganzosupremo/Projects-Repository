using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Hashneer.BitcoinMining.Experimental
{
    public static class BlockchainUtilities
    {
        /// <summary>
        /// Generates a new hash for the next block
        /// </summary>
        /// <param name="block"></param>
        /// <returns>The newly generated hash</returns>
        public static byte[] GenerateHash(this IBlock block)
        {
            using (SHA256 sha256 = new SHA256Managed())
            using (MemoryStream memoryStream = new MemoryStream())
            using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
            {
                binaryWriter.Write(block.Data);
                binaryWriter.Write(block.BlockHeigth);
                binaryWriter.Write(block.Timestamp.ToBinary());
                binaryWriter.Write(block.PreviousHash);

                var memoryStreamArray = memoryStream.ToArray();

                return sha256.ComputeHash(memoryStreamArray);
            }

            //using (SHA256 sha256 = new SHA256Managed())
            //{
            //    byte[] bytes = sha256.ComputeHash(block.Hash);

            //    //StringBuilder builder = new();
            //    //for (int i = 0; i < bytes.Length; i++)
            //    //{
            //    //    builder.Append(bytes[i].ToString());
            //    //}

            //    return bytes;
            //}
        }

        /// <summary>
        /// Mines a new hash, and therefore a new block
        /// </summary>
        /// <param name="block"></param>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception">When the difficulty is null.</exception>
        public static byte[] MineHash(this IBlock block, byte[] difficulty)
        {
            if (difficulty == null)
                throw new System.Exception(nameof(difficulty));

            byte[] hash = new byte[0];
            int d = difficulty.Length;
            //float counter = 60f;

            // This crashes unity don't use
            // need to find another method to compare the sequences
            // and to mine new blocks depending on difficulty
            // because now the blocks are mined instantly
            //while (!hash.Take(2).SequenceEqual(difficulty))
            //{
            //    Debug.Log("Hello");
            //}

            //block.BlockSubsidy--;
            difficulty[^d]++;
            hash = block.GenerateHash();
            //counter--;
            Debug.Log("Previous Hash: " + block.PreviousHash);
            
            return hash;

            //Debug.Log("Hash: " + block.Hash.ToString());
            

            //while (counter <= 100)
            //{
            //    block.Nonce++;
            //    hash = block.GenerateHash();
            //    counter++;
            //}

        }

        /// <summary>
        /// Validates a new block
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static bool IsValid(this IBlock block)
        {
            //var block1 = block.GenerateHash();
            //if (block.Hash.SequenceEqual(block1))
            //    return true;
            //else
            //    return false;
            return true;
        }

        /// <summary>
        /// Check if the previous hash of the previous block is valid
        /// </summary>
        /// <param name="block"></param>
        /// <param name="previousBlock"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool IsPreviousBlockValid(this IBlock block, IBlock previousBlock)
        {
            if (previousBlock == null)
                throw new ArgumentNullException(nameof(previousBlock));

            var previous = previousBlock.GenerateHash();
            if (previousBlock.IsValid())// && block.PreviousHash.SequenceEqual(previous))
            return true;
            else
            return false;
        }

        /// <summary>
        /// Validates the new block, 
        /// using the hash of the previous block
        /// </summary>
        /// <param name="blocksToValidate"></param>
        /// <returns></returns>
        public static bool IsValid(this IEnumerable<IBlock> blocksToValidate)
        {
            var enumerable = blocksToValidate.ToList();
            if (enumerable.Zip(enumerable.Skip(1),
                Tuple.Create).All(block => block.Item2.IsValid() && block.Item2.IsPreviousBlockValid(block.Item1)))
            return true;
            else
                return false;

        }
    }
}


