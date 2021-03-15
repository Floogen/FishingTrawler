using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile.Dimensions;
using xTile.Tiles;

namespace FishingTrawler.GameLocations
{
    internal class TrawlerHull : GameLocation
    {
        internal TrawlerHull()
        {

        }

        internal TrawlerHull(string mapPath, string name) : base(mapPath, name)
        {

        }

        protected override void resetLocalState()
        {
            base.critters = new List<Critter>();
            base.resetLocalState();

            AmbientLocationSounds.addSound(new Vector2(7f, 0f), 0);
            AmbientLocationSounds.addSound(new Vector2(13f, 0f), 0);

            Game1.changeMusicTrack("fieldofficeTentMusic");
        }

        public override void checkForMusic(GameTime time)
        {
            base.checkForMusic(time);
        }

        public override void cleanupBeforePlayerExit()
        {
            //Game1.changeMusicTrack("none");
            AmbientLocationSounds.removeSound(new Vector2(7f, 0f));
            AmbientLocationSounds.removeSound(new Vector2(13f, 0f));

            base.cleanupBeforePlayerExit();
        }

        public override bool isTileOccupiedForPlacement(Vector2 tileLocation, StardewValley.Object toPlace = null)
        {
            return true;
        }

        public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        {
            Tile tile = this.map.GetLayer("Buildings").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);
            if (tile != null && tile.Properties.ContainsKey("CustomAction"))
            {
                if (tile.Properties["CustomAction"] == "HullHole" && bool.Parse(tile.Properties["IsLeaking"]) is true)
                {
                    // Patch hole
                }

                return true;
            }

            return base.checkAction(tileLocation, viewport, who);
        }
    }
}
