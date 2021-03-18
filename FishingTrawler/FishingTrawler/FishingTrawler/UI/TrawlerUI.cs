using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingTrawler.UI
{
    internal static class TrawlerUI
    {
        internal static void DrawUI(SpriteBatch b, int fishingTripTimer, int amountOfFish, int floodLevel, bool isNetRipped)
        {
            int languageOffset = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? 8 : (LocalizedContentManager.CurrentLanguageLatin ? 16 : 8));
            Color floodingTextColor = Color.White;
            if (floodLevel > 50)
            {
                floodingTextColor = floodLevel > 75 ? Color.Red : Color.Yellow;
            }

            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            b.Draw(Game1.fadeToBlackRect, new Rectangle(16, 16, 320, 200), Color.Black * 0.75f);
            Game1.drawWithBorder($"Flooding: {floodLevel}%", Color.Black, floodingTextColor, new Vector2(32f, 24f), 0f, 1f, 1f, tiny: false);
            Game1.drawWithBorder(string.Concat("Nets: ", isNetRipped ? "Ripped" : "Good"), Color.Black, isNetRipped ? Color.Red : Color.White, new Vector2(32f, 76f), 0f, 1f, 1f, tiny: false);
            b.Draw(ModResources.uiTexture, new Vector2(32f, 140f), new Rectangle(0, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            Game1.drawWithBorder(string.Concat(amountOfFish), Color.Black, Color.White, new Vector2(96f, 145f + languageOffset), 0f, 1f, 1f, tiny: false);
            b.Draw(ModResources.uiTexture, new Vector2(166f, 145f), new Rectangle(16, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            Game1.drawWithBorder(Utility.getMinutesSecondsStringFromMilliseconds(fishingTripTimer), Color.Black, Color.White, new Vector2(230f, 145f + languageOffset), 0f, 1f, 1f, tiny: false);
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
        }
    }
}
