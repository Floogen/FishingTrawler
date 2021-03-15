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
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace FishingTrawler.GameLocations
{
    internal class TrawlerSurface : GameLocation
    {
        // Fluff ideas: Cloud shadows (if not rainy), rock pillar, small sandy hill island with palm tree
        private Rectangle _smallCloudSource = new Rectangle(0, 64, 48, 48);
        private Rectangle _mediumCloudSource = new Rectangle(0, 160, 96, 48);
        private Rectangle _longCloudSource = new Rectangle(0, 112, 96, 48);

        private Rectangle _rockPillarSource = new Rectangle(0, 0, 40, 53);
        private Rectangle _rockWithTreeSource = new Rectangle(48, 16, 96, 96);

        // Source spritesheet
        private Texture2D _spriteSheet;

        // Speed related offsets
        private float _slowOffset = -5f;
        private float _fastOffset = -7f;

        private float _nextSmoke = 0f;
        private const int CLOUD_ID = 1010101;
        private const int GROUND_ID = 2020202;

        internal TrawlerSurface()
        {

        }

        internal TrawlerSurface(string mapPath, string name) : base(mapPath, name)
        {
            base.ignoreDebrisWeather.Value = true;
            base.critters = new List<Critter>();
        }

        protected override void resetLocalState()
        {
            base.critters = new List<Critter>();
            base.resetLocalState();

            AmbientLocationSounds.addSound(new Vector2(44f, 23f), 2);

            // Set up the textures
            string assetPath = ModEntry.modHelper.Content.GetActualAssetKey("assets", ContentSource.ModFolder);
            _spriteSheet = Game1.temporaryContent.Load<Texture2D>(Path.Combine(assetPath, "BellsAndWhistles.png"));
        }

        public override void checkForMusic(GameTime time)
        {
            if (Game1.random.NextDouble() < 0.006)
            {
                base.localSound("seagulls");
            }

            // Make this playable?
            Game1.changeMusicTrack("fieldofficeTentMusic"); // Suggested tracks: Snail's Radio, Jumio Kart (Gem), Pirate Theme
        }

        public override void cleanupBeforePlayerExit()
        {
            AmbientLocationSounds.removeSound(new Vector2(44f, 23f));

            base.cleanupBeforePlayerExit();
        }

        public override void tryToAddCritters(bool onlyIfOnScreen = false)
        {
            // Overidden to hide birds, but also hides vanilla clouds (which works in our favor)
        }

        internal Rectangle PickRandomCloud()
        {
            switch (Game1.random.Next(1, 3))
            {
                case 1:
                    return _smallCloudSource;
                case 2:
                    return _mediumCloudSource;
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

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
        }

        public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
        {
            base.drawAboveAlwaysFrontLayer(b);
        }

        public override void UpdateWhenCurrentLocation(GameTime time)
        {
            base.UpdateWhenCurrentLocation(time);

            if (this._nextSmoke > 0f)
            {
                this._nextSmoke -= (float)time.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                Vector2 smokePosition = new Vector2(43.5f, 19.5f) * 64f;

                TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 1600, 64, 128), 200f, 9, 1, smokePosition, flicker: false, flipped: false, 1f, 0.015f, Color.Gray, 1f, 0.025f, 0f, 0f);
                sprite.acceleration = new Vector2(-0.30f, -0.15f);
                base.temporarySprites.Add(sprite);

                this._nextSmoke = 0.2f;
            }

            if (!Game1.isSnowing && !Game1.isRaining)
            {
                if (!base.temporarySprites.Any(t => t.id == CLOUD_ID) || (Game1.random.NextDouble() < 0.05 && base.temporarySprites.Where(t => t.id == CLOUD_ID).Count() < 10))
                {
                    string assetPath = ModEntry.modHelper.Content.GetActualAssetKey("assets", ContentSource.ModFolder);

                    // Fill up with some clouds
                    for (int x = 0; x < Game1.random.Next(1, 5); x++)
                    {
                        float randomScale = Game1.random.Next(3, 13);
                        bool randomFlipped = Game1.random.Next(0, 2) == 0 ? true : false;

                        TemporaryAnimatedSprite cloud = new TemporaryAnimatedSprite(Path.Combine(assetPath, "BellsAndWhistles.png"), PickRandomCloud(), 200f, 1, 9999, PickSpawnPosition(true), flicker: false, flipped: randomFlipped, 1f, 0f, Color.White, randomScale, 0f, 0f, 0f);
                        cloud.motion = new Vector2(_slowOffset, 0f);
                        cloud.drawAboveAlwaysFront = true;
                        cloud.id = CLOUD_ID;

                        base.temporarySprites.Add(cloud);
                    }
                }
            }

            if (!base.temporarySprites.Any(t => t.id == GROUND_ID) && Game1.random.NextDouble() < 0.10)
            {
                string assetPath = ModEntry.modHelper.Content.GetActualAssetKey("assets", ContentSource.ModFolder);
                bool randomFlipped = Game1.random.Next(0, 2) == 0 ? true : false;

                TemporaryAnimatedSprite decoration = new TemporaryAnimatedSprite(Path.Combine(assetPath, "BellsAndWhistles.png"), PickRandomDecoration(), 200f, 1, 9999, PickSpawnPosition(false), flicker: false, flipped: randomFlipped, 1f, 0f, Color.White, 4f, 0f, 0f, 0f);
                decoration.motion = new Vector2(_fastOffset, 0f);
                decoration.id = GROUND_ID;

                base.temporarySprites.Add(decoration);
            }
        }

        public override bool isTileOccupiedForPlacement(Vector2 tileLocation, StardewValley.Object toPlace = null)
        {
            return true;
        }
    }
}
