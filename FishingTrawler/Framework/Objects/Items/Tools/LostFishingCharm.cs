using FishingTrawler.Framework.Utilities;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using System.Net.NetworkInformation;

namespace FishingTrawler.Framework.Objects.Items.Tools
{
    public class LostFishingCharm
    {
        public static GenericTool CreateInstance()
        {
            var charm = new GenericTool(string.Empty, string.Empty, -1, 6, 6);
            charm.modData[ModDataKeys.LOST_FISHING_CHARM_KEY] = true.ToString();

            return charm;
        }

        public static bool Use(GameLocation location, int x, int y, Farmer who)
        {
            if (FishingTrawler.ShouldMurphyAppear(Game1.getLocationFromName("IslandSouthEast")))
            {
                Game1.warpFarmer("IslandSouthEast", 10, 40, 2);
            }
            else if (FishingTrawler.ShouldMurphyAppear(Game1.getLocationFromName("Beach")))
            {
                Game1.warpFarmer("Beach", 87, 39, 2);
            }
            else
            {
                Game1.drawObjectDialogue(FishingTrawler.i18n.Get("game_message.lost_fishing_charm.no_murphy"));
            }

            who.forceCanMove();
            return false;
        }

        public static bool IsValid(Tool tool)
        {
            if (tool is not null && tool.modData.ContainsKey(ModDataKeys.LOST_FISHING_CHARM_KEY))
            {
                return true;
            }

            return false;
        }
    }
}
