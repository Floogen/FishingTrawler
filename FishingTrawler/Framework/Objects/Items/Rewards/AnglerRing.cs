using FishingTrawler.Framework.Utilities;
using StardewValley.Objects;

namespace FishingTrawler.Framework.Objects.Items.Rewards
{
    public class AnglerRing
    {
        private const string JUKEBOX_RING_BASE_ID = "528";

        public static Ring CreateInstance()
        {
            var ring = new Ring(JUKEBOX_RING_BASE_ID);
            ring.modData[ModDataKeys.ANGLER_RING_KEY] = true.ToString();

            return ring;
        }
    }
}