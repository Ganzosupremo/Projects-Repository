using Hashneer.BitcoinMining;
using Hashneer.ElectricalSystem.Experimental;
using System;

namespace Hashneer
{
    public static class StaticEventHandler
    {
        #region MINE BLOCK EVENT
        /// <summary>
        /// The event that gets trigger when a new block has been mined
        /// </summary>
        public static event Action<MineBlockArgs> OnMineBlock;

        /// <summary>
        /// This method is called on the <seealso cref="Blockchain"/>
        ///  when a new block has been found.
        /// </summary>
        public static void CallMineBlockEvent(Block block, float difficulty)
        {
            OnMineBlock?.Invoke(new MineBlockArgs()
            {
                block = block,
                difficulty = difficulty,
            });
        }
        #endregion

        #region BLOCK MINED EVENT - NOT IMPLEMENTED
        //public static event Action<BlockMinedArgs> OnBlockMined;

        ///// <summary>
        ///// This method is also called on the <seealso cref="Blockchain"/>
        ///// but AFTER a block has been mined. This tells the miners they need to look
        ///// for a new block.
        ///// </summary>
        //public static void CallBlockMinedEvent(bool blockMined, float difficulty)
        //{
        //    OnBlockMined?.Invoke(new BlockMinedArgs()
        //    {
        //        difficulty= difficulty,
        //        blockMined = blockMined
        //    });
        //}
        #endregion

        #region ON ENERGY PRODUCED
        /// <summary>
        /// Gets triggered when any <seealso cref="EnergySource"/> has produced more energy. 
        /// </summary>
        public static event Action<EnergyProducedEventArgs> OnEnergyProduced;

        /// <summary>
        /// Every time any <seealso cref="EnergySource"/> produces energy this method is called.
        /// </summary>
        public static void CallEnergyProducedEvent(float energyProduced, EnergySource energySource)
        {
            OnEnergyProduced?.Invoke(new EnergyProducedEventArgs() 
            { 
                producedEnergy = energyProduced,
                energySource = energySource
            });
        }
        #endregion

        #region ON ENERGY CONSUMED
        public static event Action<EnergyConsumedEventArgs> OnEnergyConsumed;

        public static void CallEnergyConsumedEvent(float energyNeeded, EnergyConsumer consumer)
        {
            OnEnergyConsumed?.Invoke(new EnergyConsumedEventArgs()
            {
                energyNeeded = energyNeeded,
                consumer = consumer
            });
        }
        #endregion
    }

    public class MineBlockArgs : EventArgs
    {
        public Block block;
        public float difficulty;
    }

    public class EnergyProducedEventArgs : EventArgs
    {
        public float producedEnergy;
        public EnergySource energySource;
    }

    public class EnergyConsumedEventArgs : EventArgs
    {
        public float energyNeeded;
        public EnergyConsumer consumer;
    }
}
