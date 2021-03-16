using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace FishingTrawler.GameLocations
{
    internal class TrawlerHull : GameLocation
    {
        private List<Location> _hullHoleLocations;
        private const int TRAWLER_TILESHEET_INDEX = 2;
        private const float MINIMUM_WATER_LEVEL_FOR_FLOOR = 5f;
        private const float MINIMUM_WATER_LEVEL_FOR_ITEMS = 20f;

        internal int waterLevel;

        internal TrawlerHull()
        {

        }

        internal TrawlerHull(string mapPath, string name) : base(mapPath, name)
        {
            waterLevel = 0;
            _hullHoleLocations = new List<Location>();

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

                    if (tile.Properties.ContainsKey("CustomAction") && tile.Properties["CustomAction"] == "HullHole")
                    {
                        _hullHoleLocations.Add(new Location(x, y));
                    }
                }
            }
        }

        protected override void resetLocalState()
        {
            base.resetLocalState();
            base.critters = new List<Critter>();

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
            // Preventing player from placing items here
            return true;
        }

        public override void UpdateWhenCurrentLocation(GameTime time)
        {
            base.UpdateWhenCurrentLocation(time);

            Vector2 playerStandingPosition = new Vector2(Game1.player.getStandingX() / 64, Game1.player.getStandingY() / 64);
            if (base.lastTouchActionLocation.Equals(Vector2.Zero) && waterLevel >= MINIMUM_WATER_LEVEL_FOR_FLOOR)
            {
                string touchActionProperty = this.doesTileHaveProperty((int)playerStandingPosition.X, (int)playerStandingPosition.Y, "CustomTouchAction", "FloodWater");
                base.lastTouchActionLocation = new Vector2(Game1.player.getStandingX() / 64, Game1.player.getStandingY() / 64);
                if (touchActionProperty != null)
                {
                    if (touchActionProperty == "PlaySound")
                    {
                        string soundName = this.doesTileHaveProperty((int)playerStandingPosition.X, (int)playerStandingPosition.Y, "PlaySound", "FloodWater");
                        if (String.IsNullOrEmpty(soundName))
                        {
                            ModEntry.monitor.Log($"Tile at {playerStandingPosition} is missing PlaySound property on FloodWater layer!", LogLevel.Trace);
                            return;
                        }

                        ModEntry.monitor.Log($"{Game1.player.xVelocity}, {Game1.player.yVelocity}", LogLevel.Debug);
                        TemporaryAnimatedSprite sprite2 = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 50f, 9, 1, Game1.player.Position, flicker: false, flipped: false, 0f, 0.025f, Color.White, 1f, 0f, 0f, 0f);
                        sprite2.acceleration = new Vector2(Game1.player.xVelocity, Game1.player.yVelocity);
                        base.temporarySprites.Add(sprite2);
                        this.playSound(soundName);
                    }
                }
            }
        }

        public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        {
            return base.checkAction(tileLocation, viewport, who);
        }

        public override bool isActionableTile(int xTile, int yTile, Farmer who)
        {
            string action_property = this.doesTileHaveProperty(xTile, yTile, "CustomAction", "Buildings");
            if (action_property != null)
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
            if (who.getTileY() != 4 || who.getTileX() != tileX)
            {
                return false;
            }

            return true;
        }

        private int GetRandomBoardTile()
        {
            return 371 + Game1.random.Next(0, 5);
        }

        private bool IsHoleLeaking(int tileX, int tileY)
        {
            Tile hole = this.map.GetLayer("Buildings").Tiles[tileX, tileY];
            if (hole != null && this.doesTileHaveProperty(tileX, tileY, "CustomAction", "Buildings") == "HullHole")
            {
                return bool.Parse(hole.Properties["IsLeaking"]);
            }

            ModEntry.monitor.Log("Called [IsHoleLeaking] on tile that doesn't have IsLeaking property on Buildings layer, returning false!", LogLevel.Trace);
            return false;
        }

        public void AttemptPlugLeak(int tileX, int tileY, Farmer who)
        {
            AnimatedTile firstTile = this.map.GetLayer("Buildings").Tiles[tileX, tileY] as AnimatedTile;
            //ModEntry.monitor.Log($"({tileX}, {tileY}) | {isActionableTile(tileX, tileY, who)}", LogLevel.Trace);

            if (firstTile != null && isActionableTile(tileX, tileY, who) && IsWithinRangeOfLeak(tileX, tileY, who))
            {
                if (firstTile.Properties["CustomAction"] == "HullHole" && bool.Parse(firstTile.Properties["IsLeaking"]) is true)
                {
                    // Stop the leaking
                    firstTile.Properties["IsLeaking"] = false;

                    // Update the tiles
                    bool isFirstTile = true;
                    for (int y = tileY; y < 5; y++)
                    {
                        if (isFirstTile)
                        {
                            // Board up the hole
                            this.setMapTile(tileX, y, GetRandomBoardTile(), "Buildings", null, TRAWLER_TILESHEET_INDEX);

                            // Add the custom properties for tracking
                            this.map.GetLayer("Buildings").Tiles[tileX, tileY].Properties.CopyFrom(firstTile.Properties);

                            this.playSound("crafting");

                            isFirstTile = false;
                            continue;
                        }

                        string targetLayer = y == 4 ? "Back" : "Buildings";

                        AnimatedTile animatedTile = this.map.GetLayer(targetLayer).Tiles[tileX, y] as AnimatedTile;
                        int tileIndex = animatedTile.TileFrames[0].TileIndex - 1;

                        this.setMapTile(tileX, y, tileIndex, targetLayer, null, TRAWLER_TILESHEET_INDEX);
                    }
                }
            }
        }

        private int[] GetHullLeakTileIndexes(int startingIndex)
        {
            List<int> indexes = new List<int>();
            for (int offset = 0; offset < 6; offset++)
            {
                indexes.Add(startingIndex + offset);
            }

            return indexes.ToArray();
        }

        public void AttemptCreateHullLeak()
        {
            //ModEntry.monitor.Log("Attempting to create hull leak...", LogLevel.Trace);

            List<Location> validHoleLocations = _hullHoleLocations.Where(loc => !IsHoleLeaking(loc.X, loc.Y)).ToList();

            if (validHoleLocations.Count() == 0)
            {
                return;
            }

            // Pick a random valid spot to leak
            Location holeLocation = validHoleLocations.ElementAt(Game1.random.Next(0, validHoleLocations.Count()));

            // Set the hole as leaking
            Tile firstTile = this.map.GetLayer("Buildings").Tiles[holeLocation.X, holeLocation.Y];
            firstTile.Properties["IsLeaking"] = true;

            bool isFirstTile = true;
            for (int tileY = holeLocation.Y; tileY < 5; tileY++)
            {
                if (isFirstTile)
                {
                    // Break open the hole, copying over the properties
                    this.setAnimatedMapTile(holeLocation.X, holeLocation.Y, holeLocation.Y == 1 ? GetHullLeakTileIndexes(401) : GetHullLeakTileIndexes(377), 60, "Buildings", null, TRAWLER_TILESHEET_INDEX);
                    this.map.GetLayer("Buildings").Tiles[holeLocation.X, holeLocation.Y].Properties.CopyFrom(firstTile.Properties);

                    this.playSound("barrelBreak");

                    isFirstTile = false;
                    continue;
                }

                string targetLayer = tileY == 4 ? "Back" : "Buildings";

                int[] animatedHullTileIndexes = GetHullLeakTileIndexes(this.map.GetLayer(targetLayer).Tiles[holeLocation.X, tileY].TileIndex + 1);
                this.setAnimatedMapTile(holeLocation.X, tileY, animatedHullTileIndexes, 60, targetLayer, null, TRAWLER_TILESHEET_INDEX);
            }
        }

        public void UpdateWaterLevel()
        {
            // Should be called from ModEntry.OnOneSecondUpdateTicking (at X second interval)
            // Foreach leak, add 1 to the water level
            waterLevel += _hullHoleLocations.Where(loc => IsHoleLeaking(loc.X, loc.Y)).Count();

            ModEntry.monitor.Log(waterLevel.ToString(), LogLevel.Debug);

            // Look at using PyTK (https://www.nexusmods.com/stardewvalley/mods/1726?tab=description)
            // Use it to load FloodLevel layer and decrease the opacity (make it more visible) depending on water level?
            this.map.GetLayer("FloodWater").Properties["@Opacity"] = waterLevel > MINIMUM_WATER_LEVEL_FOR_FLOOR ? (waterLevel * 0.01f) + 0.1f : 0f;
            this.map.GetLayer("FloodItems").Properties["@Opacity"] = waterLevel > MINIMUM_WATER_LEVEL_FOR_ITEMS ? 1f : 0f;
        }
    }
}
