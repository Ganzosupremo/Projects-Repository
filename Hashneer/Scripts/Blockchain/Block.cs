using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Hashneer.BitcoinMining
{
    public class Block
    {
        public string Data { get; set; }
        public byte[] Hash { get; set; }
        public byte[] PreviousHash { get; set; }
        public DateTime Timestamp { get; set; }
        public long Nonce { get; set; } = 2;

        public Block(string data, byte[] previousHash, DateTime timeStamp)
        {
            Data = data;
            PreviousHash = previousHash;
            Nonce = 0;
            Timestamp = timeStamp;
            Hash = GenerateHash();
        }

        public void MineBlock(float difficulty)
        {
            Nonce++;
            Hash = GenerateHash();
        }

        public byte[] GenerateHash()
        {
            uint hash = 2166136261;
            byte[] bytes = BitConverter.GetBytes(hash);

            byte[] data = Encoding.UTF8.GetBytes($"{Timestamp}-{PreviousHash}-{Data}-{Nonce}");
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
            return $"Current Hash: {BitConverter.ToString(Hash).Replace("-", "")}" +
                $"\n Previous Hash: {BitConverter.ToString(PreviousHash).Replace("-", "")}" +
                //$"\n Heigth: {BlockHeigth}" +
                //$"\n Block Reward: {BlockSubsidy}" +
                $"\n Nonce: {Nonce}" +
                $"\n Time: {Timestamp}";
        }
    }
}
