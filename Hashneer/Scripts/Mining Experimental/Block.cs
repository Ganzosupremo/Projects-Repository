using System;
using System.Text;
using UnityEngine;

namespace Hashneer.BitcoinMining.Experimental
{
    public class Block : IBlock
    {
        public int BlockHeigth { get; set; }
        public DateTime Timestamp { get; set; }
        public long Nonce { get; set; }
        public byte[] PreviousHash { get; set; }
        public byte[] Hash { get; set; }
        public string Data { get; set; }
        public double BlockSubsidy { get; set; }

        public string MinerID { get; set; }

        public Block(DateTime timestamp, byte[] previousHash, 
            string data, double blockSubsidy, long nonce, string miner)
        {
            BlockHeigth = 0;
            BlockSubsidy = blockSubsidy;
            Timestamp = timestamp;
            PreviousHash = previousHash;
            Data = data;
            Nonce = nonce;
            MinerID = miner;
            Hash = CalculateHash(Nonce);
        }

        public byte[] CalculateHash(long nonce)
        {
            #region Other Hashing Methods
            //SHA256 sha256 = SHA256.Create();

            //byte[] inputBytes = Encoding.ASCII.GetBytes($"{Timestamp}-{PreviousHash ?? ""}-{Data}");
            //byte[] outputBytes = sha256.ComputeHash(inputBytes);

            //return Convert.ToBase64String(outputBytes);

            // Less resource intensive
            //int hash = 17;
            //hash = hash * 23 + Timestamp.GetHashCode();
            //hash = hash * 23 + (PreviousHash?.GetHashCode() ?? 0);
            //hash = hash * 23 + Data.GetHashCode();
            //return hash.ToString();

            // Even less resource intensive
            //const uint fnv_prime = 16777619;
            #endregion
            uint hash = 2166136261;
            byte[] bytes = BitConverter.GetBytes(hash);

            byte[] data = Encoding.UTF8.GetBytes($"{Timestamp}-{PreviousHash}-{Data}-{nonce}");
            for (int i = 0; i < data.Length; i++)
            {
                hash ^= data[i];
                hash *= Settings.fnvPrime;
            }
            bytes = BitConverter.GetBytes(hash);
            return bytes;
        }


        public override string ToString()
        {
            return $"Current Hash: {BitConverter.ToString(Hash).Replace("-","")}" +
                $"\n Previous Hash: {BitConverter.ToString(PreviousHash).Replace("-", "")}" +
                $"\n Heigth: {BlockHeigth}" +
                $"\n Block Reward: {BlockSubsidy}" +
                $"\n Nonce: {Nonce}" +
                $"\n Time: {Timestamp}";
        }
    }
}


