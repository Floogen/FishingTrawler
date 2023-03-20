using FishingTrawler.Framework.GameLocations;
using FishingTrawler.Framework.Objects.Items.Rewards;
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
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace FishingTrawler.GameLocations
{
    internal class TrawlerSurface : TrawlerLocation
    {
        // Source rectangles for drawing fluff
        private Rectangle _smallCloudSource = new Rectangle(0, 64, 48, 48);
        private Rectangle _mediumCloudSource = new Rectangle(0, 160, 96, 48);
        private Rectangle _largeCloudSource = new Rectangle(0, 208, 144, 96);
        private Rectangle _longCloudSource = new Rectangle(0, 112, 96, 48);

        private Rectangle _rockPillarSource = new Rectangle(0, 0, 40, 53);
        private Rectangle _rockWithTreeSource = new Rectangle(48, 16, 96, 96);

        // Minigame stat related
        internal int fishCaughtQuantity;
        internal int fishCaughtMultiplier;

        // Animation related
        internal static int hullFuelLevel = 100;

        // Helpers
        private List<Location> _netRipLocations;

        // Speed related offsets
        private float _slowOffset = -5f;
        private float _fastOffset = -7f;
        private float _nextBubble = 0.1f;

        private const string FLAG_LAYER_NAME = "Flags";
        private const string ROPE_LAYER_NAME = "Front";
        private const int CLOUD_ID = 1010101;
        private const int GROUND_ID = 2020202;
        private const int FLAGS_TILESHEET_INDEX = 2;
        private const int TRAWLER_TILESHEET_INDEX = 4;

        public TrawlerSurface()
        {

        }

        internal TrawlerSurface(string mapPath, string name) : base(mapPath, name)
        {
            ignoreDebrisWeather.Value = true;
            critters = new List<Critter>();

            fishCaughtQuantity = 0;
            fishCaughtMultiplier = 1;
            _netRipLocations = new List<Location>();

            Layer ropeLayer = map.GetLayer(ROPE_LAYER_NAME);
            for (int x = 0; x < ropeLayer.LayerWidth; x++)
            {
                for (int y = 0; y < ropeLayer.LayerHeight; y++)
                {
                    Tile tile = ropeLayer.Tiles[x, y];
                    if (tile is null)
                    {
                        continue;
                    }

                    if (tile.Properties.ContainsKey("CustomAction") && tile.Properties["CustomAction"] == "RippedNet")
                    {
                        _netRipLocations.Add(new Location(x, y));
                    }
                }
            }

            // Set water tiles for Dynamic Reflections
            var backLayer = this.map.GetLayer("Back");
            this.waterTiles = new bool[backLayer.LayerWidth, backLayer.LayerHeight];
            for (int x = 0; x < backLayer.LayerWidth; x++)
            {
                for (int y = 0; y < backLayer.LayerHeight; y++)
                {
                    Tile tile = backLayer.PickTile(new Location(x * 64, y * 64), Game1.viewport.Size);

                    if (tile is null || tile.TileIndex != 543)
                    {
                        continue;
                    }

                    tile.TileIndexProperties["Water"] = "T";
                }
            }
        }

        internal override void Reset()
        {
            foreach (Location netRippedLocation in _netRipLocations.Where(loc => IsNetRipped(loc.X, loc.Y)))
            {
                AttemptFixNet(netRippedLocation.X, netRippedLocation.Y, Game1.player, true);
            }

            UpdateFishCaught(fishCaughtOverride: 0);

            // Clear out the TemporaryAnimatedSprite we preserved
            resetLocalState();
        }

        protected override void resetLocalState()
        {
            base.resetLocalState();

            AmbientLocationSounds.addSound(new Vector2(44f, 23f), 2);
        }

        public override void checkForMusic(GameTime time)
        {
            if (Game1.random.NextDouble() < 0.006 && !(Game1.isSnowing || Game1.isRaining))
            {
                localSound("seagulls");
            }

            if (string.IsNullOrEmpty(miniJukeboxTrack.Value) && Game1.getMusicTrackName() != "fieldofficeTentMusic")
            {
                Game1.changeMusicTrack("fieldofficeTentMusic"); // Suggested tracks: Snail's Radio, Jumio Kart (Gem), Pirate Theme
            }
        }

        public override void tryToAddCritters(bool onlyIfOnScreen = false)
        {
            // Overidden to hide birds, but also hides vanilla clouds (which works in our favor)
        }

        public override void UpdateWhenCurrentLocation(GameTime time)
        {
            base.UpdateWhenCurrentLocation(time);

            if (hullFuelLevel > 0)
            {
                Rectangle back_rectangle = new Rectangle(33 * 64, 23 * 64, 16, 6 * 64);
                if (_nextBubble > 0f)
                {
                    _nextBubble -= (float)time.ElapsedGameTime.TotalSeconds;
                }
                else
                {
                    Vector2 position2 = Utility.getRandomPositionInThisRectangle(back_rectangle, Game1.random);
                    TemporaryAnimatedSprite sprite2 = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 50f, 9, 1, position2, flicker: false, flipped: false, 0f, 0.025f, Color.White, 1f, 0f, 0f, 0f);
                    sprite2.acceleration = new Vector2(-0.25f, 0f);
                    if (Context.IsSplitScreen)
                    {
                        FishingTrawler.multiplayer.broadcastSprites(this, sprite2);
                    }
                    else
                    {
                        this.temporarySprites.Add(sprite2);

                    }
                    _nextBubble = hullFuelLevel > 50 ? 0.01f : 0.05f;
                }
            }
        }

        internal Rectangle PickRandomCloud()
        {
            switch (Game1.random.Next(1, 5))
            {
                case 1:
                    return _smallCloudSource;
                case 2:
                    return _mediumCloudSource;
                case 3:
                    return _largeCloudSource;
                default:
                    return _longCloudSource;
            }
        }

        internal Rectangle PickRandomDecoration()
        {
            switch (Game1.random.Next(1, 3))
            {
                case 1:
                    return _rockPillarSource;
                default:
                    return _rockWithTreeSource;
            }
        }

        internal Vector2 PickSpawnPosition(bool isCloud)
        {
            int reservedLowerYPosition = 15;
            int reservedUpperYPosition = 33;

            int reservedLowerXPosition = 70;
            int reservedUpperXPosition = 80;

            if (isCloud)
            {
                return new Vector2(Game1.random.Next(reservedLowerXPosition, reservedUpperXPosition), Game1.random.Next(13, 40)) * 64f;
            }

            if (Game1.random.Next(0, 2) == 0)
            {
                return new Vector2(Game1.random.Next(reservedLowerXPosition, reservedUpperXPosition), Game1.random.Next(13, reservedLowerYPosition)) * 64f;
            }
            else
            {
                return new Vector2(Game1.random.Next(reservedLowerXPosition, reservedUpperXPosition), Game1.random.Next(reservedUpperYPosition, 40)) * 64f;
            }
        }

        public override bool isActionableTile(int xTile, int yTile, Farmer who)
        {
            string actionProperty = doesTileHaveProperty(xTile, yTile, "CustomAction", ROPE_LAYER_NAME);
            if (actionProperty != null && actionProperty == "RippedNet")
            {
                if (!base.IsWithinRangeOfTile(xTile, yTile, 2, 4, who))
                {
                    Game1.mouseCursorTransparency = 0.5f;
                }

                return true;
            }

            return base.isActionableTile(xTile, yTile, who);
        }

        private int[] GetNetRippedTileIndexes(int startingIndex)
        {
            List<int> indexes = new List<int>();
            for (int offset = 0; offset < 5; offset++)
            {
                indexes.Add(startingIndex + offset);
            }

            return indexes.ToArray();
        }

        private bool IsNetRipped(int tileX, int tileY)
        {
            Tile hole = map.GetLayer(ROPE_LAYER_NAME).Tiles[tileX, tileY];
            if (hole != null && doesTileHaveProperty(tileX, tileY, "CustomAction", ROPE_LAYER_NAME) == "RippedNet")
            {
                return bool.Parse(hole.Properties["IsRipped"]);
            }

            FishingTrawler.monitor.Log("Called [IsNetRipped] on tile that doesn't have IsRipped property on AlwaysFront layer, returning false!", LogLevel.Trace);
            return false;
        }

        private int[] GetFlagTileIndexes(int startingIndex)
        {
            List<int> indexes = new List<int>();
            for (int offset = 0; offset < 8; offset++)
            {
                indexes.Add(startingIndex + (Enum.GetNames(typeof(FlagType)).Length * 2) * offset);
            }

            return indexes.ToArray();
        }

        public void SetFlagTexture(FlagType flagType)
        {
            if (flagType == FlagType.Unknown)
            {
                // Clear the flag
                setMapTileIndex(40, 22, -1, FLAG_LAYER_NAME);
                setMapTileIndex(41, 22 - 1, -1, FLAG_LAYER_NAME);
                return;
            }

            setAnimatedMapTile(40, 22, GetFlagTileIndexes(2 * (int)flagType), 60, FLAG_LAYER_NAME, null, FLAGS_TILESHEET_INDEX);
            setAnimatedMapTile(41, 22, GetFlagTileIndexes(2 * (int)flagType + 1), 60, FLAG_LAYER_NAME, null, FLAGS_TILESHEET_INDEX);
        }

        public bool AttemptCreateNetRip(int tileX = -1, int tileY = -1)
        {
            //ModEntry.monitor.Log("Attempting to create net rip...", LogLevel.Trace);

            List<Location> validNetLocations = _netRipLocations.Where(loc => !IsNetRipped(loc.X, loc.Y)).ToList();

            if (validNetLocations.Count() == 0)
            {
                return false;
            }

            // Pick a random valid spot to rip
            Location netLocation = validNetLocations.ElementAt(Game1.random.Next(0, validNetLocations.Count()));
            if (tileX != -1 && tileY != -1)
            {
                if (!_netRipLocations.Any(loc => IsNetRipped(loc.X, loc.Y) is false && loc.X == tileX && loc.Y == tileY))
                {
                    return false;
                }

                netLocation = _netRipLocations.FirstOrDefault(loc => IsNetRipped(loc.X, loc.Y) is false && loc.X == tileX && loc.Y == tileY);
            }

            // Set the net as ripped
            Tile firstTile = map.GetLayer(ROPE_LAYER_NAME).Tiles[netLocation.X, netLocation.Y];
            firstTile.Properties["IsRipped"] = true;

            setAnimatedMapTile(netLocation.X, netLocation.Y, GetNetRippedTileIndexes(74), 90, ROPE_LAYER_NAME, null, TRAWLER_TILESHEET_INDEX);

            // Copy over the old properties
            map.GetLayer(ROPE_LAYER_NAME).Tiles[netLocation.X, netLocation.Y].Properties.CopyFrom(firstTile.Properties);

            playSound("crit");

            return true;
        }

        public bool AttemptFixNet(int tileX, int tileY, Farmer who, bool forceRepair = false)
        {
            AnimatedTile firstTile = map.GetLayer(ROPE_LAYER_NAME).Tiles[tileX, tileY] as AnimatedTile;

            if (firstTile is null)
            {
                return false;
            }

            if (!forceRepair && !(isActionableTile(tileX, tileY, who) && base.IsWithinRangeOfTile(tileX, tileY, 2, 4, who)))
            {
                return false;
            }

            if (!firstTile.Properties.ContainsKey("CustomAction") || !firstTile.Properties.ContainsKey("IsRipped"))
            {
                return false;
            }

            if (firstTile.Properties["CustomAction"] == "RippedNet" && bool.Parse(firstTile.Properties["IsRipped"]) is true)
            {
                // Stop the rip
                firstTile.Properties["IsRipped"] = false;
                base.AddRepairedTile(tileX, tileY);

                // Patch up the net
                setMapTile(tileX, tileY, 99, ROPE_LAYER_NAME, null, TRAWLER_TILESHEET_INDEX);

                // Add the custom properties for tracking
                map.GetLayer(ROPE_LAYER_NAME).Tiles[tileX, tileY].Properties.CopyFrom(firstTile.Properties);

                playSound("harvest");
            }


            return false;
        }

        public void UpdateFishCaught(int fuelLevel = 0, int fishCaughtOverride = -1)
        {
            if (fishCaughtOverride > -1)
            {
                fishCaughtQuantity = fishCaughtOverride;
            }
            else
            {
                foreach (var net in _netRipLocations)
                {
                    int fishCaughtInNet = (int)(FishingTrawler.config.fishPerNet * fishCaughtMultiplier);

                    // Check if any fuel bonus or penalties need to be applied
                    switch (fuelLevel)
                    {
                        case var _ when fuelLevel > 50:
                            fishCaughtInNet += 1;
                            break;
                        case var _ when fuelLevel <= 50 && fuelLevel > 0:
                            // No bonus fish per net
                            break;
                        case var _ when fuelLevel <= 0:
                            fishCaughtInNet = 0;
                            break;
                    }

                    // Check if ripped net penalty needs to be applied
                    if (IsNetRipped(net.X, net.Y))
                    {
                        fishCaughtInNet = -(int)(FishingTrawler.config.fishPerNet);
                    }

                    fishCaughtQuantity += fishCaughtInNet;
                }

                if (fishCaughtQuantity < 0)
                {
                    fishCaughtQuantity = 0;
                }
            }

            //ModEntry.monitor.Log($"Fish caught: {fishCaughtQuantity}", LogLevel.Debug);
        }

        public bool IsPlayerByBoatEdge(Farmer who)
        {
            int playerX = who.getStandingX() / 64;
            int playerY = who.getStandingY() / 64;

            string actionProperty = doesTileHaveProperty(playerX, playerY, "CustomAction", "Back");
            if (actionProperty != null && actionProperty == "EmptyBucketSpot")
            {
                return true;
            }

            return false;
        }

        public bool AreAnyNetsRipped()
        {
            return _netRipLocations.Any(loc => IsNetRipped(loc.X, loc.Y));
        }

        public bool AreAllNetsRipped()
        {
            return _netRipLocations.Count(loc => IsNetRipped(loc.X, loc.Y)) == _netRipLocations.Count();
        }

        public int GetRippedNetsCount()
        {
            return _netRipLocations.Count(loc => IsNetRipped(loc.X, loc.Y));
        }

        public Location? GetRandomWorkingNet()
        {
            var validNetLocations = _netRipLocations.Where(loc => !IsNetRipped(loc.X, loc.Y));
            if (validNetLocations.Count() == 0)
            {
                return null;
            }

            // Pick a random valid spot to leak
            return _netRipLocations.Where(loc => !IsNetRipped(loc.X, loc.Y)).ElementAt(Game1.random.Next(0, validNetLocations.Count()));
        }

        public int GetWorkingNetCount()
        {
            return _netRipLocations.Where(loc => IsNetRipped(loc.X, loc.Y) is false).Count();
        }
    }
}
