using Microsoft.Xna.Framework;
using StardewModdingAPI;
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

namespace FishingTrawler.GameLocations
{
    internal class TrawlerCabin : GameLocation
    {
        private List<Location> _cabinPipeLocations;
        private const int CABIN_TILESHEET_INDEX = 2;

        internal TrawlerCabin()
        {

        }

        internal TrawlerCabin(string mapPath, string name) : base(mapPath, name)
        {
            _cabinPipeLocations = new List<Location>();

            Layer buildingsLayer = this.map.GetLayer("Buildings");
            for (int x = 0; x < buildingsLayer.LayerWidth; x++)
            {
                for (int y = 0; y < buildingsLayer.LayerHeight; y++)
                {
                    Tile tile = buildingsLayer.Tiles[x, y];
                    if (tile is null)
                    {
                        continue;
                    }

                    if (tile.Properties.ContainsKey("CustomAction") && tile.Properties["CustomAction"] == "RustyPipe")
                    {
                        _cabinPipeLocations.Add(new Location(x, y));
                    }
                }
            }
        }

        internal void Reset()
        {
            foreach (Location pipeLocation in _cabinPipeLocations.Where(loc => IsPipeLeaking(loc.X, loc.Y)))
            {
                AttemptPlugLeak(pipeLocation.X, pipeLocation.Y, Game1.player, true);
            }
        }

        protected override void resetLocalState()
        {
            base.resetLocalState();

            AmbientLocationSounds.addSound(new Vector2(4f, 3f), 2);
        }

        public override void cleanupBeforePlayerExit()
        {
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

        public override bool isActionableTile(int xTile, int yTile, Farmer who)
        {
            string actionProperty = this.doesTileHaveProperty(xTile, yTile, "CustomAction", "Buildings");
            if (actionProperty != null && actionProperty == "RustyPipe")
            {
                if (!IsWithinRangeOfLeak(xTile, yTile, who))
                {
                    Game1.mouseCursorTransparency = 0.5f;
                }

                return true;
            }

            return base.isActionableTile(xTile, yTile, who);
        }

        private bool IsWithinRangeOfLeak(int tileX, int tileY, Farmer who)
        {
            if (who.getTileY() != 4 || !Enumerable.Range(who.getTileX() - 1, 3).Contains(tileX))
            {
                return false;
            }

            return true;
        }

        private bool IsPipeLeaking(int tileX, int tileY)
        {
            Tile hole = this.map.GetLayer("Buildings").Tiles[tileX, tileY];
            if (hole != null && this.doesTileHaveProperty(tileX, tileY, "CustomAction", "Buildings") == "RustyPipe")
            {
                return bool.Parse(hole.Properties["IsLeaking"]);
            }

            ModEntry.monitor.Log("Called [IsHoleLeaking] on tile that doesn't have IsLeaking property on Buildings layer, returning false!", LogLevel.Trace);
            return false;
        }

        public void AttemptPlugLeak(int tileX, int tileY, Farmer who, bool forceRepair = false)
        {
            AnimatedTile firstTile = this.map.GetLayer("Buildings").Tiles[tileX, tileY] as AnimatedTile;
            //ModEntry.monitor.Log($"({tileX}, {tileY}) | {isActionableTile(tileX, tileY, who)}", LogLevel.Debug);

            if (firstTile is null)
            {
                return;
            }

            if (!forceRepair && !(isActionableTile(tileX, tileY, who) && IsWithinRangeOfLeak(tileX, tileY, who)))
            {
                return;
            }

            if (firstTile.Properties["CustomAction"] == "RustyPipe" && bool.Parse(firstTile.Properties["IsLeaking"]) is true)
            {
                // Stop the leak
                firstTile.Properties["IsLeaking"] = false;

                // Patch up the net
                this.setMapTile(tileX, tileY, 129, "Buildings", null, CABIN_TILESHEET_INDEX);
                this.setMapTileIndex(tileX, tileY - 1, -1, "Buildings");

                // Add the custom properties for tracking
                this.map.GetLayer("Buildings").Tiles[tileX, tileY].Properties.CopyFrom(firstTile.Properties);

                this.playSound("hammer");
            }
        }

        private int[] GetPipeLeakingIndexes(int startingIndex)
        {
            List<int> indexes = new List<int>();
            for (int offset = 0; offset < 3; offset++)
            {
                indexes.Add(startingIndex + offset);
            }

            return indexes.ToArray();
        }

        public void AttemptCreatePipeLeak()
        {
            List<Location> validPipeLocations = _cabinPipeLocations.Where(loc => !IsPipeLeaking(loc.X, loc.Y)).ToList();

            if (validPipeLocations.Count() == 0)
            {
                return;
            }

            // Pick a random valid spot to rip
            Location netLocation = validPipeLocations.ElementAt(Game1.random.Next(0, validPipeLocations.Count()));

            // Set the net as ripped
            Tile firstTile = this.map.GetLayer("Buildings").Tiles[netLocation.X, netLocation.Y];
            firstTile.Properties["IsLeaking"] = true;

            this.setAnimatedMapTile(netLocation.X, netLocation.Y, GetPipeLeakingIndexes(248), 90, "Buildings", null, CABIN_TILESHEET_INDEX);
            this.setAnimatedMapTile(netLocation.X, netLocation.Y - 1, GetPipeLeakingIndexes(218), 90, "Buildings", null, CABIN_TILESHEET_INDEX);

            // Copy over the old properties
            this.map.GetLayer("Buildings").Tiles[netLocation.X, netLocation.Y].Properties.CopyFrom(firstTile.Properties);

            this.playSound("flameSpell");
        }

        public bool AreAnyPipesLeaking()
        {
            return _cabinPipeLocations.Any(loc => IsPipeLeaking(loc.X, loc.Y));
        }

        public int GetLeakingPipesCount()
        {
            return _cabinPipeLocations.Count(loc => IsPipeLeaking(loc.X, loc.Y));
        }
    }
}
