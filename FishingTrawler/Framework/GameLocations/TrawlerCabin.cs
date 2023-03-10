using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace FishingTrawler.GameLocations
{
    internal class TrawlerCabin : GameLocation
    {
        private List<Location> _computerLocations;
        private const int CABIN_TILESHEET_INDEX = 2;

        public TrawlerCabin()
        {

        }

        internal TrawlerCabin(string mapPath, string name) : base(mapPath, name)
        {
            _computerLocations = new List<Location>();

            Layer buildingsLayer = map.GetLayer("Buildings");
            for (int x = 0; x < buildingsLayer.LayerWidth; x++)
            {
                for (int y = 0; y < buildingsLayer.LayerHeight; y++)
                {
                    Tile tile = buildingsLayer.Tiles[x, y];
                    if (tile is null)
                    {
                        continue;
                    }

                    if (tile.Properties.ContainsKey("CustomAction") && tile.Properties["CustomAction"] == "Guidance")
                    {
                        _computerLocations.Add(new Location(x, y));
                    }
                }
            }
        }

        internal void Reset()
        {
            // TODO: Reset the guidance system percentage?
        }

        protected override void resetLocalState()
        {
            base.resetLocalState();

            AmbientLocationSounds.addSound(new Vector2(4f, 3f), 2);

            if (miniJukeboxTrack.Value is null)
            {
                Game1.changeMusicTrack("fieldofficeTentMusic"); // Suggested tracks: Snail's Radio, Jumio Kart (Gem), Pirate Theme
            }
        }

        public override void UpdateWhenCurrentLocation(GameTime time)
        {
            base.UpdateWhenCurrentLocation(time);
        }

        public override void cleanupBeforePlayerExit()
        {
            if (Game1.startedJukeboxMusic)
            {
                FishingTrawler.SetTrawlerTheme(Game1.getMusicTrackName());
            }
            else if (string.IsNullOrEmpty(miniJukeboxTrack.Value) && !string.IsNullOrEmpty(FishingTrawler.trawlerThemeSong))
            {
                FishingTrawler.SetTrawlerTheme(null);
            }

            base.cleanupBeforePlayerExit();
        }

        public override void checkForMusic(GameTime time)
        {
            base.checkForMusic(time);
        }

        public override bool isTileOccupiedForPlacement(Vector2 tileLocation, StardewValley.Object toPlace = null)
        {
            // Preventing player from placing items here
            return true;
        }

        public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        {
            Tile tile = map.GetLayer("Buildings").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);
            if (tile != null && tile.Properties.ContainsKey("CustomAction"))
            {
                if (tile.Properties["CustomAction"] == "PathosCat")
                {
                    Game1.drawObjectDialogue(FishingTrawler.i18n.Get("game_message.pathos_cat"));
                    return true;
                }
            }

            return base.checkAction(tileLocation, viewport, who);
        }

        public override bool isActionableTile(int xTile, int yTile, Farmer who)
        {
            string actionProperty = doesTileHaveProperty(xTile, yTile, "CustomAction", "Buildings");
            if (actionProperty != null && actionProperty == "PathosCat")
            {
                return Enumerable.Range(who.getTileX(), 1).Contains(xTile);
            }

            return base.isActionableTile(xTile, yTile, who);
        }

        #region Guidance system event methods
        #endregion
    }
}
