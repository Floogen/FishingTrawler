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
        internal static void DrawUI(SpriteBatch b, int fishingTripTimer, int amountOfFish, int floodLevel, int rippedNetsCount)
        {
            int languageOffset = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? 8 : (LocalizedContentManager.CurrentLanguageLatin ? 16 : 8));

            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            b.Draw(ModResources.uiTexture, new Vector2(16f, 16f) + new Vector2(-3f, -3f) * 4f, new Rectangle(0, 16, 3, 49), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f - 0.001f);
            b.Draw(ModResources.uiTexture, new Vector2(16f, 16f) + new Vector2(-1f, -3f) * 4f, new Rectangle(2, 16, 70, 49), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f - 0.001f);
            b.Draw(ModResources.uiTexture, new Vector2(16f, 16f) + new Vector2(256 + 4, -12f), new Rectangle(68, 16, 4, 59), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f - 0.001f);

            Game1.drawWithBorder($"Flooding: {floodLevel}%", Color.Black, floodLevel < 50 ? Color.White : floodLevel > 75 ? Color.Red : Color.Yellow, new Vector2(32f, 24f), 0f, 1f, 1f, tiny: false);
            Game1.drawWithBorder(string.Concat("Nets: ", rippedNetsCount < 1 ? "Working" : rippedNetsCount > 1 ? "Ripped" : "Ripping"), Color.Black, rippedNetsCount < 1 ? Color.White : rippedNetsCount > 1 ? Color.Red : Color.Yellow, new Vector2(32f, 76f), 0f, 1f, 1f, tiny: false);
            b.Draw(ModResources.uiTexture, new Vector2(28f, 130f), new Rectangle(0, 0, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
            Game1.drawWithBorder(string.Concat(amountOfFish), Color.Black, Color.White, new Vector2(76f, 125f + languageOffset), 0f, 1f, 1f, tiny: false);
            b.Draw(ModResources.uiTexture, new Vector2(136f, 125f), new Rectangle(16, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            Game1.drawWithBorder(Utility.getMinutesSecondsStringFromMilliseconds(fishingTripTimer), Color.Black, Color.White, new Vector2(190f, 125f + languageOffset), 0f, 1f, 1f, tiny: false);
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
        }
    }
}
