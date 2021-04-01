using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingTrawler.Messages
{
    public enum SyncType
    {
        Unknown,
        WaterLevel,
        FishCaught
    }

    internal class TrawlerSyncMessage
    {
        public SyncType SyncType { get; set; }
        public int Quantity { get; set; }

        public TrawlerSyncMessage()
        {

        }

        public TrawlerSyncMessage(SyncType syncType, int waterLevel)
        {
            SyncType = syncType;
            Quantity = waterLevel;
        }
    }
}
