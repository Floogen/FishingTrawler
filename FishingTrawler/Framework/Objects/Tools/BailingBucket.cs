using FishingTrawler.Framework.Managers;
using FishingTrawler.Framework.Utilities;
using FishingTrawler.GameLocations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using PyTK.CustomElementHandler;

namespace FishingTrawler.Objects.Tools
{
    internal class BailingBucket : MilkPail
    {
        private string _displayName = FishingTrawler.i18n.Get("item.bailing_bucket.name");
        private readonly NetEvent0 _finishEvent = new NetEvent0();

        private bool _containsWater = false;
        private float _bucketScale = 0f;

        public BailingBucket() : base()
        {
            modData.Add(ModDataKeys.BAILING_BUCKET_KEY, "true");
            description = FishingTrawler.i18n.Get("item.bailing_bucket.description_empty");
        }

        public object getReplacement()
        {
            return new MilkPail();
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            return new Dictionary<string, string>();
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            modData = (replacement as MilkPail).modData;
        }

        public override Item getOne()
        {
            BailingBucket bucket = new BailingBucket();
            bucket._GetOneFrom(this);
            return bucket;
        }

        protected override string loadDisplayName()
        {
            return _displayName;
        }

        public override bool canBeTrashed()
        {
            if (FishingTrawler.IsPlayerOnTrawler())
            {
                return false;
            }

            return true;
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            IndexOfMenuItemView = 0;

            int spriteOffset = _containsWater ? 16 : 0;
            spriteBatch.Draw(AssetManager.bucketTexture, location + new Vector2(32f, 32f), new Rectangle(spriteOffset, 0, 16, 16), color * transparency, 0f, new Vector2(8f, 8f), 4f * (scaleSize + _bucketScale), SpriteEffects.None, layerDepth);
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddFields(_finishEvent);
            _finishEvent.onEvent += doFinish;
        }

        public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
        {
            if (!FishingTrawler.IsPlayerOnTrawler() || who is null || who != null && !Game1.player.Equals(who))
            {
                who.forceCanMove();
                return false;
            }

            if (location is TrawlerHull trawlerHull)
            {
                if (_containsWater)
                {
                    Game1.addHUDMessage(new HUDMessage(FishingTrawler.i18n.Get("game_message.bailing_bucket.empty_into_sea"), 3));
                }
                else if (trawlerHull.IsFlooding())
                {
                    _containsWater = true;
                    _bucketScale = 0.5f;
                    description = FishingTrawler.i18n.Get("item.bailing_bucket.description_full");

                    trawlerHull.ChangeWaterLevel(-5);
                    trawlerHull.localSound("slosh");
                    FishingTrawler.SyncTrawler(Messages.SyncType.WaterLevel, trawlerHull.GetWaterLevel(), FishingTrawler.GetFarmersOnTrawler());
                }
                else
                {
                    Game1.addHUDMessage(new HUDMessage(FishingTrawler.i18n.Get("game_message.bailing_bucket.no_water_to_bail"), 3));
                }
            }
            else if (location is TrawlerSurface trawlerSurface && _containsWater)
            {
                if (trawlerSurface.IsPlayerByBoatEdge(who))
                {
                    _containsWater = false;
                    _bucketScale = 0.5f;
                    description = FishingTrawler.i18n.Get("item.bailing_bucket.description_empty");

                    who.currentLocation.localSound("waterSlosh");
                }
                else
                {
                    Game1.addHUDMessage(new HUDMessage(FishingTrawler.i18n.Get("game_message.bailing_bucket.stand_closer_to_edge"), 3));
                }
            }
            else
            {
                Game1.addHUDMessage(new HUDMessage(FishingTrawler.i18n.Get("game_message.bailing_bucket.bail_from_hull"), 3));
            }

            who.forceCanMove();
            return true;
        }

        public override void tickUpdate(GameTime time, Farmer who)
        {
            if (_bucketScale > 0f)
            {
                _bucketScale -= 0.01f;
            }

            lastUser = who;
            base.tickUpdate(time, who);
            _finishEvent.Poll();
        }

        public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
        {
            base.DoFunction(location, x, y, power, who);
            who.Stamina -= 4f;
            CurrentParentTileIndex = 0;
            IndexOfMenuItemView = 0;

            finish();
        }

        private void finish()
        {
            _finishEvent.Fire();
        }

        private void doFinish()
        {
            lastUser.CanMove = true;
            lastUser.completelyStopAnimatingOrDoingAction();
            lastUser.UsingTool = false;
            lastUser.canReleaseTool = true;
        }
    }
}
