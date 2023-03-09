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
        internal static void DrawUI(SpriteBatch b, int fishingTripTimer, int amountOfFish, int floodLevel, bool isHullLeaking, int rippedNetsCount, int leakingPipes)
        {
            int languageOffset = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en ? 8 : LocalizedContentManager.CurrentLanguageLatin ? 16 : 8;

            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            b.Draw(FishingTrawler.assetManager.uiTexture, new Vector2(16f, 16f) + new Vector2(-3f, -3f) * 4f, new Rectangle(0, 16, 7, 57), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f - 0.001f);
            b.Draw(FishingTrawler.assetManager.uiTexture, new Vector2(16f, 16f) + new Vector2(-1f, -3f) * 4f, new Rectangle(2, 16, 74, 57), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f - 0.001f);
            b.Draw(FishingTrawler.assetManager.uiTexture, new Vector2(16f, 16f) + new Vector2(256 + 4, -12f), new Rectangle(68, 16, 8, 67), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f - 0.001f);

            Game1.drawWithBorder($"{FishingTrawler.i18n.Get("ui.flooding.name")}: {floodLevel}%", Color.Black, floodLevel > 75 ? Color.Red : isHullLeaking ? Color.Yellow : Color.White, new Vector2(32f, 24f), 0f, 1f, 1f, tiny: false);
            Game1.drawWithBorder(string.Concat(FishingTrawler.i18n.Get("ui.nets.name"), ": ", rippedNetsCount < 1 ? FishingTrawler.i18n.Get("ui.generic.working") : rippedNetsCount > 1 ? FishingTrawler.i18n.Get("ui.nets.ripped") : FishingTrawler.i18n.Get("ui.nets.ripping")), Color.Black, rippedNetsCount < 1 ? Color.White : rippedNetsCount > 1 ? Color.Red : Color.Yellow, new Vector2(32f, 76f), 0f, 1f, 1f, tiny: false);
            Game1.drawWithBorder(string.Concat(FishingTrawler.i18n.Get("ui.engine.name"), ": ", leakingPipes > 0 ? FishingTrawler.i18n.Get("ui.engine.failing") : FishingTrawler.i18n.Get("ui.generic.working")), Color.Black, leakingPipes > 0 ? Color.Red : Color.White, new Vector2(32f, 128f), 0f, 1f, 1f, tiny: false);
            b.Draw(FishingTrawler.assetManager.uiTexture, new Vector2(28f, 174f), new Rectangle(0, 0, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
            Game1.drawWithBorder(string.Concat(amountOfFish), Color.Black, Color.White, new Vector2(76f, 169f + languageOffset), 0f, 1f, 1f, tiny: false);
            b.Draw(FishingTrawler.assetManager.uiTexture, new Vector2(136f, 169f), new Rectangle(16, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            Game1.drawWithBorder(Utility.getMinutesSecondsStringFromMilliseconds(fishingTripTimer), Color.Black, Color.White, new Vector2(190f, 169f + languageOffset), 0f, 1f, 1f, tiny: false);
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
        }

        internal static void DrawNotification(SpriteBatch b, GameLocation location, string s, float alpha = 1f)
        {
            FishingTrawler.modHelper.Reflection.GetMethod(typeof(SpriteText), "setUpCharacterMap").Invoke();

            int x = 0;
            int y = 0;

            int color = 8;
            float layerDepth = 0.88f;
            int width = SpriteText.getWidthOfString(s) * 2;

            Vector2 position;
            switch (location)
            {
                case TrawlerSurface trawlerSurface:
                    x = 41;
                    y = 23;
                    position = Game1.GlobalToLocal(new Vector2(41.5f, 23.7f) * 64f);
                    break;
                case TrawlerHull trawlerHull:
                    x = 14;
                    y = 1;
                    position = Game1.GlobalToLocal(new Vector2(1.59f, 0.58f) * 64f);
                    break;
                case TrawlerCabin trawlerCabin:
                    x = 8;
                    y = 0;
                    position = Game1.GlobalToLocal(new Vector2(8.5f, 0f) * 64f);
                    break;
                default:
                    position = new Vector2(0f, 0f);
                    break;
            }

            if (SpriteText.fontPixelZoom < 4f && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.ko)
            {
                y += (int)((4f - SpriteText.fontPixelZoom) * 4f);
            }
            int accumulatedHorizontalSpaceBetweenCharacters = 0;

            int text_width = SpriteText.getWidthOfString(s);
            Vector2 speech_position = position;
            if (Game1.currentLocation != null && Game1.currentLocation.map != null && Game1.currentLocation.map.Layers[0] != null)
            {
                int left_edge = -Game1.viewport.X + 28;
                int right_edge = -Game1.viewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64 - 28;

                speech_position.X += text_width / 2;

                speech_position.X = Utility.Clamp(speech_position.X, position.X, position.X + text_width - 24f);
            }

            position *= 1f / Game1.options.zoomLevel;
            b.Draw(Game1.mouseCursors, position + new Vector2(-7f, -3f) * 4f, new Rectangle(324, 299, 7, 17), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
            b.Draw(Game1.mouseCursors, position + new Vector2(0f, -3f) * 4f, new Rectangle(331, 299, 1, 17), Color.White * alpha, 0f, Vector2.Zero, new Vector2(SpriteText.getWidthOfString(s), 4f), SpriteEffects.None, layerDepth - 0.001f);
            b.Draw(Game1.mouseCursors, position + new Vector2(text_width, -12f), new Rectangle(332, 299, 7, 17), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
            b.Draw(Game1.mouseCursors, position + new Vector2(0f, 52f), new Rectangle(341, 308, 6, 5), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.0001f);

            x = (int)position.X;
            position.Y += (4f - SpriteText.fontPixelZoom) * 4f;

            if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko)
            {
                position.Y -= 8f;
            }
            s = s.Replace(Environment.NewLine, "");
            if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.th)
            {
                position.Y -= (4f - SpriteText.fontPixelZoom) * 4f;
            }
            s = s.Replace('♡', '<');
            for (int i = 0; i < Math.Min(s.Length, 9999); i++)
            {
                if (LocalizedContentManager.CurrentLanguageLatin || FishingTrawler.modHelper.Reflection.GetMethod(typeof(SpriteText), "IsSpecialCharacter").Invoke<bool>(s[i]) || SpriteText.forceEnglishFont)
                {
                    float tempzoom = SpriteText.fontPixelZoom;
                    if (FishingTrawler.modHelper.Reflection.GetMethod(typeof(SpriteText), "IsSpecialCharacter").Invoke<bool>(s[i]) || SpriteText.forceEnglishFont)
                    {
                        SpriteText.fontPixelZoom = 3f;
                    }
                    if (s[i] == '^')
                    {
                        position.Y += 18f * SpriteText.fontPixelZoom;
                        position.X = x;
                        accumulatedHorizontalSpaceBetweenCharacters = 0;
                        SpriteText.fontPixelZoom = tempzoom;
                        continue;
                    }
                    accumulatedHorizontalSpaceBetweenCharacters = (int)(0f * SpriteText.fontPixelZoom);
                    bool upper = char.IsUpper(s[i]) || s[i] == 'ß';
                    Vector2 spriteFontOffset = new Vector2(0f, -1 + (upper ? -3 : 0));
                    if (s[i] == 'Ç')
                    {
                        spriteFontOffset.Y += 2f;
                    }
                    if (SpriteText.positionOfNextSpace(s, i, (int)position.X - x, accumulatedHorizontalSpaceBetweenCharacters) >= width)
                    {
                        position.Y += 18f * SpriteText.fontPixelZoom;
                        accumulatedHorizontalSpaceBetweenCharacters = 0;
                        position.X = x;
                        if (s[i] == ' ')
                        {
                            SpriteText.fontPixelZoom = tempzoom;
                            continue;
                        }
                    }
                    b.Draw(color != -1 ? SpriteText.coloredTexture : SpriteText.spriteTexture, position + spriteFontOffset * SpriteText.fontPixelZoom, FishingTrawler.modHelper.Reflection.GetMethod(typeof(SpriteText), "getSourceRectForChar").Invoke<Rectangle>(s[i], false), (FishingTrawler.modHelper.Reflection.GetMethod(typeof(SpriteText), "IsSpecialCharacter").Invoke<bool>(s[i]) ? Color.White : SpriteText.getColorFromIndex(color)) * alpha, 0f, Vector2.Zero, SpriteText.fontPixelZoom, SpriteEffects.None, layerDepth);
                    if (i < s.Length - 1)
                    {
                        position.X += 8f * SpriteText.fontPixelZoom + accumulatedHorizontalSpaceBetweenCharacters + SpriteText.getWidthOffsetForChar(s[i + 1]) * SpriteText.fontPixelZoom;
                    }
                    if (s[i] != '^')
                    {
                        position.X += SpriteText.getWidthOffsetForChar(s[i]) * SpriteText.fontPixelZoom;
                    }
                    SpriteText.fontPixelZoom = tempzoom;
                    continue;
                }
                /*
                                 if (s[i] == '^')
                                {
                                    position.Y += (float)(ModEntry.modHelper.Reflection.GetField<FontFile>(typeof(SpriteText), "FontFile").GetValue().Common.LineHeight + 2) * SpriteText.fontPixelZoom;
                                    position.X = x;
                                    accumulatedHorizontalSpaceBetweenCharacters = 0;
                                    continue;
                                }
                                if (i > 0 && ModEntry.modHelper.Reflection.GetMethod(typeof(SpriteText), "IsSpecialCharacter").Invoke<bool>(s[i - 1]))
                                {
                                    position.X += 24f;
                                }
                                if (ModEntry.modHelper.Reflection.GetField<Dictionary<char, FontChar>>(typeof(SpriteText), "_characterMap").GetValue().TryGetValue(s[i], out var fc))
                                {
                                    Rectangle sourcerect = new Rectangle(fc.X, fc.Y, fc.Width, fc.Height);
                                    Texture2D _texture = ModEntry.modHelper.Reflection.GetField<List<Texture2D>>(typeof(SpriteText), "fontPages").GetValue()[fc.Page];
                                    if (SpriteText.positionOfNextSpace(s, i, (int)position.X, accumulatedHorizontalSpaceBetweenCharacters) >= x + width - 4)
                                    {
                                        position.Y += (float)(ModEntry.modHelper.Reflection.GetField<FontFile>(typeof(SpriteText), "FontFile").GetValue().Common.LineHeight + 2) * SpriteText.fontPixelZoom;
                                        accumulatedHorizontalSpaceBetweenCharacters = 0;
                                        position.X = x;
                                    }
                                    Vector2 position2 = new Vector2(position.X + (float)fc.XOffset * SpriteText.fontPixelZoom, position.Y + (float)fc.YOffset * SpriteText.fontPixelZoom);
                                    if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko)
                                    {
                                        position2.Y -= 8f;
                                    }
                                    if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru)
                                    {
                                        Vector2 offset = new Vector2(-1f, 1f) * SpriteText.fontPixelZoom;
                                        b.Draw(_texture, position2 + offset, sourcerect, SpriteText.getColorFromIndex(color) * alpha * SpriteText.shadowAlpha, 0f, Vector2.Zero, SpriteText.fontPixelZoom, SpriteEffects.None, layerDepth);
                                        b.Draw(_texture, position2 + new Vector2(0f, offset.Y), sourcerect, SpriteText.getColorFromIndex(color) * alpha * SpriteText.shadowAlpha, 0f, Vector2.Zero, SpriteText.fontPixelZoom, SpriteEffects.None, layerDepth);
                                        b.Draw(_texture, position2 + new Vector2(offset.X, 0f), sourcerect, SpriteText.getColorFromIndex(color) * alpha * SpriteText.shadowAlpha, 0f, Vector2.Zero, SpriteText.fontPixelZoom, SpriteEffects.None, layerDepth);
                                    }
                                    b.Draw(_texture, position2, sourcerect, SpriteText.getColorFromIndex(color) * alpha, 0f, Vector2.Zero, SpriteText.fontPixelZoom, SpriteEffects.None, layerDepth);
                                    position.X += (float)fc.XAdvance * SpriteText.fontPixelZoom;
                                }
                */
            }
        }
    }
}
