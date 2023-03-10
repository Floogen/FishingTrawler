﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Text;
//using PyTK.CustomElementHandler;

namespace FishingTrawler.Framework.Objects.Items.Rewards
{
    public class AnglerRing : Ring
    {
        internal static Texture2D ringTexture;

        public AnglerRing() : base(531)
        {
            Category = -96;
            displayName = FishingTrawler.i18n.Get("item.angler_ring.name");
            description = FishingTrawler.i18n.Get("item.angler_ring.description");
            price.Value = 0;
        }

        public override Item getOne()
        {
            AnglerRing ring = new AnglerRing();
            ring._GetOneFrom(this);
            return ring;
        }

        public object getReplacement()
        {
            return new Ring(531);
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            //saveData.Add("something", "myValue");
            return new Dictionary<string, string>(); ;
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            // Unused
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            spriteBatch.Draw(ringTexture, location + new Vector2(32f, 32f) * scaleSize, new Rectangle(0, 0, 16, 16), color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, scaleSize * 3f, SpriteEffects.None, layerDepth);
        }

        public override void drawTooltip(SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText)
        {
            Utility.drawTextWithShadow(spriteBatch, Game1.parseText(description, Game1.smallFont, getDescriptionWidth()), font, new Vector2(x + 16, y + 16 + 4), Game1.textColor);
            y += (int)font.MeasureString(Game1.parseText(description, Game1.smallFont, getDescriptionWidth())).Y;

            Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(20, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
            Utility.drawTextWithShadow(spriteBatch, "+2 Fishing", font, new Vector2(x + 16 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
            y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
        }

        public override Point getExtraSpaceNeededForTooltipSpecialIcons(SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, StringBuilder descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom)
        {
            Point dimensions = new Point(0, startingHeight);
            int extra_rows_needed = 1;

            dimensions.X = (int)Math.Max(minWidth, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", 9999)).X + horizontalBuffer);
            dimensions.Y += extra_rows_needed * Math.Max((int)font.MeasureString("TT").Y, 48);
            return dimensions;
        }

        public override void onEquip(Farmer who, GameLocation location)
        {
            who.addedFishingLevel.Value += 2;
        }

        public override void onUnequip(Farmer who, GameLocation location)
        {
            who.addedFishingLevel.Value = Math.Max(0, who.addedFishingLevel.Value - 2);
        }

        public override void onDayUpdate(Farmer who, GameLocation location)
        {
            onEquip(who, location);
        }
    }
}