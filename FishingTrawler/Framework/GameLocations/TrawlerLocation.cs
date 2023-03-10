using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace FishingTrawler.Framework.GameLocations
{
    internal abstract class TrawlerLocation : GameLocation
    {
        public TrawlerLocation()
        {

        }

        internal TrawlerLocation(string mapPath, string name) : base(mapPath, name)
        {

        }

        internal abstract void Reset();

        internal bool IsWithinRangeOfTile(int tileX, int tileY, int xDistance, int yDistance, Farmer who)
        {
            if (Enumerable.Range(who.getTileX() - xDistance, (xDistance * 2) + 1).Contains(tileX))
            {
                if (Enumerable.Range(who.getTileY() - yDistance, (yDistance * 2) + 1).Contains(tileY))
                {
                    return true;
                }
            }

            return false;
        }

        protected override void resetLocalState()
        {
            base.resetLocalState();
            critters = new List<Critter>();

            if (string.IsNullOrEmpty(miniJukeboxTrack.Value))
            {
                Game1.changeMusicTrack("fieldofficeTentMusic"); // Suggested tracks: Snail's Radio, Jumio Kart (Gem), Pirate Theme
            }
        }

        public override void checkForMusic(GameTime time)
        {
            base.checkForMusic(time);
        }

        public override void cleanupBeforePlayerExit()
        {
            //Game1.changeMusicTrack("none");
            base.cleanupBeforePlayerExit();
        }

        public override bool isTileOccupiedForPlacement(Vector2 tileLocation, StardewValley.Object toPlace = null)
        {
            // Preventing player from placing items here
            return true;
        }
    }
}
