using System;
using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using System.Text;

namespace Hashneer.BitcoinMining.Experimental
{
    public static class MSHA256
    {
        private static SHA256 sha256;
        public static string Init(string rawData)
        {
            string hashedData = ComputeSha256Hash(rawData);
            return hashedData;
        }

        public static string ComputeSha256Hash(string stringToHash)
        {
            using (sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(stringToHash));

                StringBuilder stringBuilder = new();
                for (int i = 0; i < bytes.Length; i++)
                {
                    stringBuilder.Append(bytes[i].ToString("x2"));
                }

                return stringBuilder.ToString();
            }
        }

        public static string ComputeHash(string stringToHash)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(stringToHash));

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString());
            }

            return builder.ToString();
        }

        //public static uint RotateInt(uint inputWord, int numberOfBitsToRotate)
        //{
        //    sha256.ComputeHash();

        //}

        public static int Ch(int x, int y, int z)
        {
            return ((x & y) ^ (~x & z));
        }

        public static int Maj(int x, int y, int z)
        {
            return ((x & y) ^ (x & z) ^ (y & z));
        }

        //public static int Sig0f(int x)
        //{
        //    return (RotateInt(x, 2) ^ RotateInt(x, 13) ^ RotateInt(x, 22));
        //}

        //public static int Sig1f(int x)
        //{
        //    return (RotateInt(x, 6) ^ RotateInt(x, 11) ^ RotateInt(x, 25));
        //}

        //public static int Sig0(int x)
        //{
        //    return RotateInt(x, 7) ^ RotateInt(x, 18) ^ (x >> 3);
        //}

        //public static int Sig1(int x)
        //{
        //    return (RotateInt(x, 17) ^ RotateInt(x, 19) ^ (x >> 10));
        //}

        static void Hash(int[] input, int bitLength, int[] outputLocation)
        {
            uint[] H_0 = { 0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a, 0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19 };

            uint[] K = new uint[64]
            {
                0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5,
                0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
                0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3,
                0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
                0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc,
                0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
                0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7,
                0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
                0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13,
                0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
                0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3,
                0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
                0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5,
                0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
                0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208,
                0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
            };

            int wordlength = bitLength / 32 + 1;
            int k = (512 * 512 - bitLength - 1) % 512;
            int[] message = new int[1000];

            for (int i = 0; i < wordlength; i++)
                message[i] = input[i];

            if (bitLength % 32 != 0)
                message[bitLength / 32] = message[bitLength / 32] | (1 << (32 - 1 - bitLength % 32));
            else
                message[bitLength / 32] = 1 << 31;

            uint rounds;

            // Assuming our data isn't bigger than 2^32 bits long... which it won't be for a block hash.
            if (wordlength % 16 == 0 || wordlength % 16 == 15)
            {
                message[wordlength + 15 + 16 - wordlength % 16] = bitLength;
                rounds = (uint)wordlength / 16 + 2;
            }
            else
            {
                message[wordlength + 15 - wordlength % 16] = bitLength;
                rounds = (uint)wordlength / 16 + 1;
            }

            uint[,] M = new uint[32, 16];

            for (int i = 0; i < 16; i++)
                for (int j = 0; j <= rounds; j++)
                    M[j, i] = (uint)message[i + j * 16];

            uint[,] H = new uint[32, 8];

            for (int i = 0; i < 8; i++)
                H[0, i] = H_0[i];

            //// Here our hash function rounds actually start.
            //for (int i = 1; i <= rounds; i++)
            //{
            //    var a = H[i - 1, 0];
            //    var b = H[i - 1, 1];
            //    var c = H[i - 1, 2];
            //    var d = H[i - 1, 3];
            //    var e = H[i - 1, 4];
            //    var f = H[i - 1, 5];
            //    var g = H[i - 1, 6];
            //    var h = H[i - 1, 7];

            //    uint[] W = new uint[64];

            //    for (int j = 0; j < 64; j++)
            //    {
            //        var ch = Ch(e, f, g);
            //        var maj = Maj(a, b, c);
            //        var sig0 = Sig0f(a);
            //        var sig1 = Sig1f(e);

            //        if (j < 16)
            //            W[j] = M[i - 1, j];
            //        else
            //            W[j] = sig1(W[j - 2]) + W[j - 7] + sig0(W[j - 15]) + W[j - 16];

            //        var T1 = h + sig1 + ch + K[j] + W[j];
            //        var T2 = sig0 + maj;
            //        h = g;
            //        g = f;
            //        f = e;
            //        e = (uint)(d + T1);
            //        d = c;
            //        c = b;
            //        b = a;
            //        a = (uint)(T1 + T2);
            //    }

            //    H[i, 0] = a + H[i - 1, 0];
            //    H[i, 1] = b + H[i - 1, 1];
            //    H[i, 2] = c + H[i - 1, 2];
            //    H[i, 3] = d + H[i - 1, 3];
            //    H[i, 4] = e + H[i - 1, 4];
            //    H[i, 5] = f + H[i - 1, 5];
            //    H[i, 6] = g + H[i - 1, 6];
            //    H[i, 7] = h + H[i - 1, 7];
        //  }
        }
    }
}

