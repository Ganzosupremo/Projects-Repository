using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hashneer.BitcoinMining
{
    public static class Settings
    {
        #region Blockchain Settings
        public const int maxHashIterations = 1000;
        public const int fnvPrime = 16777619;
        public const int maxDifficulty = 10000;
        public const int minDifficulty = 1;

        /// <summary>
        /// The number of blocks when the block reward will be halved
        /// </summary>
        public const int blockRewardCutoff = 100;
        #endregion
    }
}
