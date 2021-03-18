using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingTrawler.Objects.Tools
{
    internal class BailingBucket : Tool
    {
        public BailingBucket() : base("Bailing Bucket", 0, 0, 0, false, 0)
        {
            this.modData.Add(ModEntry.bailingBucketKey, "true");
        }

        public override Item getOne()
        {
            return this;
        }

        protected override string loadDisplayName()
        {
            return "Bailing Bucket";
        }

        protected override string loadDescription()
        {
            return "A trusy, albeit rusty bucket! Use to pick up water in the hull and empty it on the ship's deck.";
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            base.IndexOfMenuItemView = 0;
            spriteBatch.Draw(ModResources.bucketTexture, location + new Vector2(32f, 32f), new Rectangle(0, 0, 16, 16), color * transparency, 0f, new Vector2(8f, 8f), 4f * scaleSize, SpriteEffects.None, layerDepth);
        }


        public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
        {
            base.CurrentParentTileIndex = 12;
            base.IndexOfMenuItemView = 12;
            bool overrideCheck = false;
            Rectangle orePanRect = new Rectangle(location.orePanPoint.X * 64 - 64, location.orePanPoint.Y * 64 - 64, 256, 256);
            if (orePanRect.Contains(x, y) && Utility.distance(who.getStandingX(), orePanRect.Center.X, who.getStandingY(), orePanRect.Center.Y) <= 192f)
            {
                overrideCheck = true;
            }
            who.lastClick = Vector2.Zero;
            x = (int)who.GetToolLocation().X;
            y = (int)who.GetToolLocation().Y;
            who.lastClick = new Vector2(x, y);
            if (location.orePanPoint != null && !location.orePanPoint.Equals(Point.Zero))
            {
                Rectangle panRect = who.GetBoundingBox();
                if (overrideCheck || panRect.Intersects(orePanRect))
                {
                    who.faceDirection(2);
                    who.FarmerSprite.animateOnce(303, 50f, 4);
                    return true;
                }
            }
            who.forceCanMove();
            return true;
        }
    }
}
