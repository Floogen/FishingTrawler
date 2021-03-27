using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

namespace FishingTrawler.Objects.Rewards
{
    internal enum FlagType
    {
        Unknown,
        Parley,
        JollyRoger,
        GamblersCrest,
        MermaidsBlessing,
        PatronSaint,
        SharksFin,
        Worldly
    }

    internal class AncientFlag : StardewValley.Objects.Furniture
    {
        private Texture2D _flagTexture;
        private Rectangle _sourceRectangle;

        internal readonly FlagType flagType;

        // Using Pirate Flag id (1900) as base vanilla object
        public AncientFlag() : this(FlagType.Unknown, Vector2.Zero)
        {

        }

        public AncientFlag(FlagType flagType) : this(flagType, Vector2.Zero)
        {

        }

        public AncientFlag(FlagType flagType, Vector2 tile) : base(1900, tile)
        {
            this._flagTexture = ModResources.ancientFlagsTexture;
            this._sourceRectangle = new Rectangle(32 * (int)flagType % _flagTexture.Width, 32 * (int)flagType / _flagTexture.Width * 32, 32, 32);
            this.flagType = flagType;

            this.modData.Add(ModEntry.ANCIENT_FLAG_KEY, flagType.ToString());
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            if (base.isTemporarilyInvisible)
            {
                return;
            }

            if (Furniture.isDrawingLocationFurniture)
            {
                spriteBatch.Draw(this._flagTexture, Game1.GlobalToLocal(Game1.viewport, this.drawPosition + ((base.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), this._sourceRectangle, Color.White * alpha, 0f, Vector2.Zero, 4f, base.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ((int)this.furniture_type == 12) ? (2E-09f + base.tileLocation.Y / 100000f) : ((float)(base.boundingBox.Value.Bottom - (((int)this.furniture_type == 6 || (int)this.furniture_type == 17 || (int)this.furniture_type == 13) ? 48 : 8)) / 10000f));
            }
            else
            {
                spriteBatch.Draw(this._flagTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - (this.sourceRect.Height * 4 - base.boundingBox.Height) + ((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), this._sourceRectangle, Color.White * alpha, 0f, Vector2.Zero, 4f, base.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ((int)this.furniture_type == 12) ? (2E-09f + base.tileLocation.Y / 100000f) : ((float)(base.boundingBox.Value.Bottom - (((int)this.furniture_type == 6 || (int)this.furniture_type == 17 || (int)this.furniture_type == 13) ? 48 : 8)) / 10000f));
            }
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            spriteBatch.Draw(this._flagTexture, location + new Vector2(32f, 32f), this._sourceRectangle, color * transparency, 0f, new Vector2(this._sourceRectangle.Width / 2, this._sourceRectangle.Height / 2), 2f * scaleSize, SpriteEffects.None, layerDepth);
            if (((drawStackNumber == StackDrawType.Draw && this.maximumStackSize() > 1 && this.Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && this.Stack != int.MaxValue)
            {
                Utility.drawTinyDigits(base.stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(base.stack, 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, color);
            }
        }

        public override void drawPlacementBounds(SpriteBatch spriteBatch, GameLocation location)
        {
            if (location is Beach)
            {
                // Draw nothing to avoid covering up Murphy when attempting to give him an ancient flag
                return;
            }

            base.drawPlacementBounds(spriteBatch, location);
        }


        protected override string loadDisplayName()
        {
            return GetFlagName(this.flagType);
        }

        public override string getDescription()
        {
            return Game1.parseText(GetFlagDescription(this.flagType), Game1.smallFont, this.getDescriptionWidth());
        }

        public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
        {
            if (this.flagType is FlagType.Unknown)
            {
                Game1.showRedMessage("You need to identify the flag before placing it on walls!");
                return false;
            }

            return base.placementAction(location, x, y, who);
        }

        internal static string GetFlagName(FlagType flagType)
        {
            switch (flagType)
            {
                case FlagType.Parley:
                    return "Parley Flag";
                case FlagType.JollyRoger:
                    return "Jolly Roger";
                case FlagType.GamblersCrest:
                    return "The Gambler's Crest";
                case FlagType.MermaidsBlessing:
                    return "Mermaid's Blessing";
                case FlagType.PatronSaint:
                    return "The Patron Saint";
                case FlagType.SharksFin:
                    return "The Shark's Fin";
                case FlagType.Worldly:
                    return "Worldly Flag";
                default:
                    return "Ancient Flag";
            }
        }

        // TODO: Fill these in with description of their effects
        private string GetFlagDescription(FlagType flagType)
        {
            switch (flagType)
            {
                case FlagType.Parley:
                    return "Parley Flag";
                case FlagType.JollyRoger:
                    return "Jolly Roger";
                case FlagType.GamblersCrest:
                    return "The Gambler's Crest";
                case FlagType.MermaidsBlessing:
                    return "Mermaid's Blessing";
                case FlagType.PatronSaint:
                    return "The Patron Saint";
                case FlagType.SharksFin:
                    return "The Shark's Fin";
                case FlagType.Worldly:
                    return "Worldly Flag";
                default:
                    return "An ancient flag that faintly shimmers with magic.\n\nPerhaps Murphy would know more about it?";
            }
        }
    }
}
