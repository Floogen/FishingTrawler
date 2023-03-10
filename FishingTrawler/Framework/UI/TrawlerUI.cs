using FishingTrawler.Framework.Managers;
using FishingTrawler.GameLocations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;

namespace FishingTrawler.UI
{
    internal static class TrawlerUI
    {
        internal static void DrawUI(SpriteBatch b, int fishingTripTimer, int amountOfFish, int floodLevel, bool isHullLeaking, int rippedNetsCount, int fuelLevel)
        {
            int languageOffset = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en ? 8 : LocalizedContentManager.CurrentLanguageLatin ? 16 : 8;

            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            b.Draw(FishingTrawler.assetManager.uiTexture, new Vector2(16f, 16f) + new Vector2(-3f, -3f) * 4f, new Rectangle(0, 16, 7, 57), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f - 0.001f);
            b.Draw(FishingTrawler.assetManager.uiTexture, new Vector2(16f, 16f) + new Vector2(-1f, -3f) * 4f, new Rectangle(2, 16, 74, 57), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f - 0.001f);
            b.Draw(FishingTrawler.assetManager.uiTexture, new Vector2(16f, 16f) + new Vector2(256 + 4, -12f), new Rectangle(68, 16, 8, 67), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f - 0.001f);

            Game1.drawWithBorder($"{FishingTrawler.i18n.Get("ui.flooding.name")}: {floodLevel}%", Color.Black, floodLevel > 75 ? Color.Red : isHullLeaking ? Color.Yellow : Color.White, new Vector2(32f, 24f), 0f, 1f, 1f, tiny: false);
            Game1.drawWithBorder(string.Concat(FishingTrawler.i18n.Get("ui.nets.name"), ": ", rippedNetsCount < 1 ? FishingTrawler.i18n.Get("ui.generic.working") : rippedNetsCount > 1 ? FishingTrawler.i18n.Get("ui.nets.ripped") : FishingTrawler.i18n.Get("ui.nets.ripping")), Color.Black, rippedNetsCount < 1 ? Color.White : rippedNetsCount > 1 ? Color.Red : Color.Yellow, new Vector2(32f, 76f), 0f, 1f, 1f, tiny: false);
            Game1.drawWithBorder($"{FishingTrawler.i18n.Get("ui.engine.name")}: {fuelLevel}%", Color.Black, fuelLevel < 30 ? Color.Red : fuelLevel < 50 ? Color.Yellow : Color.White, new Vector2(32f, 128f), 0f, 1f, 1f, tiny: false);
            b.Draw(FishingTrawler.assetManager.uiTexture, new Vector2(28f, 174f), new Rectangle(0, 0, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
            Game1.drawWithBorder(string.Concat(amountOfFish), Color.Black, Color.White, new Vector2(76f, 169f + languageOffset), 0f, 1f, 1f, tiny: false);
            b.Draw(FishingTrawler.assetManager.uiTexture, new Vector2(136f, 169f), new Rectangle(16, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            Game1.drawWithBorder(Utility.getMinutesSecondsStringFromMilliseconds(fishingTripTimer), Color.Black, Color.White, new Vector2(190f, 169f + languageOffset), 0f, 1f, 1f, tiny: false);
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
        }
    }
}
