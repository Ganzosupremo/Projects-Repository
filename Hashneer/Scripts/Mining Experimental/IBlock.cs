using System;

namespace Hashneer.BitcoinMining.Experimental
{
    public interface IBlock
    {
        public int BlockHeigth { get; set; }
        public DateTime Timestamp { get; set; }
        public long Nonce { get; set; }
        public byte[] PreviousHash { get; set; }
        public byte[] Hash { get; set; }
        public string Data { get; set; }
        public double BlockSubsidy { get; set; }

        public string MinerID { get; set; }
    }
}
