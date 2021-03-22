using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;
using FishingTrawler.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingTrawler.Patches.Locations
{
    public class BeachPatch : Patch
    {
        private readonly Type _beach = typeof(Beach);

        internal BeachPatch(IMonitor monitor) : base(monitor)
        {

        }

        internal override void Apply(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Method(_beach, nameof(Beach.cleanupBeforePlayerExit), null), postfix: new HarmonyMethod(GetType(), nameof(CleanupBeforePlayerExitPatch)));
            harmony.Patch(AccessTools.Method(_beach, nameof(Beach.draw), new[] { typeof(SpriteBatch) }), postfix: new HarmonyMethod(GetType(), nameof(DrawPatch)));
            harmony.Patch(AccessTools.Method(_beach, nameof(Beach.UpdateWhenCurrentLocation), new[] { typeof(GameTime) }), postfix: new HarmonyMethod(GetType(), nameof(UpdateWhenCurrentLocationPatch)));
        }

        internal static void CleanupBeforePlayerExitPatch(Beach __instance)
        {
            ModEntry.trawlerObject.Reset();
        }


        internal static void DrawPatch(Beach __instance, SpriteBatch b)
        {
            Texture2D boatTexture = ModResources.boatTexture;
            if (boatTexture is null)
            {
                return;
            }

            b.Draw(boatTexture, Game1.GlobalToLocal(ModEntry.trawlerObject.GetTrawlerPosition()), new Rectangle(0, 16, 224, 160), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 720f / 10000f);
            // Overlay for glass port
            //b.Draw(boatTexture, Game1.GlobalToLocal(Game1.viewport, Trawler.GetTrawlerPosition() + new Vector2(8f, 0f) * 4f), new Rectangle(0, 160, 128, 96), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (720f + 408f) / 10000f);
        }

        internal static void UpdateWhenCurrentLocationPatch(Beach __instance, GameTime time)
        {
            Trawler trawler = ModEntry.trawlerObject;
            if (trawler is null)
            {
                return;
            }

            if (trawler._boatDirection != 0)
            {
                trawler._boatOffset += trawler._boatDirection;
                if (__instance.currentEvent != null)
                {
                    foreach (NPC actor in __instance.currentEvent.actors)
                    {
                        actor.shouldShadowBeOffset = true;
                        actor.drawOffset.X = trawler._boatOffset;
                    }
                    foreach (Farmer farmerActor in __instance.currentEvent.farmerActors)
                    {
                        farmerActor.shouldShadowBeOffset = true;
                        farmerActor.drawOffset.X = trawler._boatOffset;
                    }
                }
            }


            Microsoft.Xna.Framework.Rectangle back_rectangle = new Microsoft.Xna.Framework.Rectangle(24, 188, 16, 220);
            back_rectangle.X += (int)trawler.GetTrawlerPosition().X;
            back_rectangle.Y += (int)trawler.GetTrawlerPosition().Y;
            if ((float)trawler._boatDirection != 0f)
            {
                if (trawler._nextBubble > 0f)
                {
                    trawler._nextBubble -= (float)time.ElapsedGameTime.TotalSeconds;
                }
                else
                {
                    Vector2 position2 = Utility.getRandomPositionInThisRectangle(back_rectangle, Game1.random);
                    TemporaryAnimatedSprite sprite2 = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 50f, 9, 1, position2, flicker: false, flipped: false, 0f, 0.025f, Color.White, 1f, 0f, 0f, 0f);
                    sprite2.acceleration = new Vector2(-0.25f * (float)Math.Sign(trawler._boatDirection), 0f);
                    __instance.temporarySprites.Add(sprite2);
                    trawler._nextBubble = 0.01f;
                }
                if (trawler._nextSlosh > 0f)
                {
                    trawler._nextSlosh -= (float)time.ElapsedGameTime.TotalSeconds;
                }
                else
                {
                    Game1.playSound("waterSlosh");
                    trawler._nextSlosh = 0.5f;
                }
            }
            if (trawler._boatAnimating)
            {
                if (trawler._nextSmoke > 0f)
                {
                    trawler._nextSmoke -= (float)time.ElapsedGameTime.TotalSeconds;
                    return;
                }
                Vector2 position = new Vector2(158f, -32f) * 4f + trawler.GetTrawlerPosition();
                TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 1600, 64, 128), 200f, 9, 1, position, flicker: false, flipped: false, 1f, 0.025f, Color.White, 1f, 0.025f, 0f, 0f);
                sprite.acceleration = new Vector2(-0.25f, -0.15f);
                __instance.temporarySprites.Add(sprite);
                trawler._nextSmoke = 0.2f;
            }
        }

    }
}
