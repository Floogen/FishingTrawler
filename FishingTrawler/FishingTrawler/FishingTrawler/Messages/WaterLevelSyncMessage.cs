using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingTrawler.Messages
{
    internal class WaterLevelSyncMessage
    {
        public int WaterLevel { get; set; }

        public WaterLevelSyncMessage()
        {

        }

        public WaterLevelSyncMessage(int waterLevel)
        {
            WaterLevel = waterLevel;
        }
    }
}
