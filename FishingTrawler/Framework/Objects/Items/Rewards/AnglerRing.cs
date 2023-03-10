using FishingTrawler.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishingTrawler.Framework.Objects.Items.Rewards
{
    public class AnglerRing
    {
        private const int JUKEBOX_RING_BASE_ID = 528;

        public static Ring CreateInstance()
        {
            var ring = new Ring(JUKEBOX_RING_BASE_ID);
            ring.modData[ModDataKeys.ANGLER_RING_KEY] = true.ToString();

            return ring;
        }
    }
}