using FishingTrawler.Framework.GameLocations;
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
    internal class TrawlerCabin : TrawlerLocation
    {
        private List<Location> _computerLocations;

        private const int TRAWLER_TILESHEET_INDEX = 2;
        private const float BASE_COMPUTER_MILLISECONDS = 60000f;
        private const float CYCLE_COMPUTER_MILLISECONDS = 30000f;

        private int _completedComputerCycles;
        private double _computerCooldownMilliseconds;

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

        internal override void Reset()
        {
            _completedComputerCycles = 0;
            _computerCooldownMilliseconds = BASE_COMPUTER_MILLISECONDS;
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

        public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
        {
            base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);

            if (IsComputerReady() is false)
            {
                _computerCooldownMilliseconds -= time.ElapsedGameTime.TotalMilliseconds;
                setMapTileIndex(3, 2, -1, "Front", TRAWLER_TILESHEET_INDEX);
            }
            else
            {
                setAnimatedMapTile(3, 2, new int[] { 13, 14, 15, 16, 17 }, 90, "Front", null, TRAWLER_TILESHEET_INDEX);
            }
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

        public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        {
            string actionProperty = doesTileHaveProperty(tileLocation.X, tileLocation.Y, "CustomAction", "Buildings");

            if (actionProperty is not null)
            {
                if (actionProperty == "PathosCat")
                {
                    Game1.drawObjectDialogue(FishingTrawler.i18n.Get("game_message.pathos_cat"));
                    return true;
                }
                else if (actionProperty == "Guidance" && base.IsWithinRangeOfTile(tileLocation.X, tileLocation.Y, 1, 1, who) is true)
                {
                    if (IsComputerReady() is false)
                    {
                        Game1.drawObjectDialogue(FishingTrawler.i18n.Get("game_message.computer.not_ready"));
                    }
                    else
                    {
                        AcceptPlottedCourse();
                    }
                    return true;
                }
            }

            return base.checkAction(tileLocation, viewport, who);
        }

        public override bool isActionableTile(int xTile, int yTile, Farmer who)
        {
            string actionProperty = doesTileHaveProperty(xTile, yTile, "CustomAction", "Buildings");

            if (actionProperty is not null)
            {
                if (actionProperty == "PathosCat")
                {
                    if (base.IsWithinRangeOfTile(xTile, yTile, 1, 1, who) is false)
                    {
                        Game1.mouseCursorTransparency = 0.5f;
                    }
                    return true;
                }
                else if (actionProperty == "Guidance")
                {
                    if (base.IsWithinRangeOfTile(xTile, yTile, 1, 1, who) is false)
                    {
                        Game1.mouseCursorTransparency = 0.5f;
                    }
                    return true;
                }
            }

            return base.isActionableTile(xTile, yTile, who);
        }

        #region Guidance system event methods
        public void AcceptPlottedCourse()
        {
            if (IsComputerReady() is false)
            {
                return;
            }
            FishingTrawler.eventManager.IncrementTripTimer(30000);

            _completedComputerCycles += 1;
            _computerCooldownMilliseconds = (_completedComputerCycles * CYCLE_COMPUTER_MILLISECONDS) + BASE_COMPUTER_MILLISECONDS;
        }

        public bool IsComputerReady()
        {
            return _computerCooldownMilliseconds <= 0;
        }
        #endregion
    }
}
