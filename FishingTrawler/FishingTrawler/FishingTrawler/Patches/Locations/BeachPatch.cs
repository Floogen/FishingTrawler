﻿using Harmony;
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
using xTile.Tiles;
using System.Reflection;

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
            harmony.Patch(AccessTools.Method(_beach, "resetLocalState", null), postfix: new HarmonyMethod(GetType(), nameof(ResetLocationStatePatch)));
            harmony.Patch(AccessTools.Method(_beach, nameof(Beach.checkAction), new[] { typeof(xTile.Dimensions.Location), typeof(xTile.Dimensions.Rectangle), typeof(Farmer) }), postfix: new HarmonyMethod(GetType(), nameof(CheckActionPatch)));
            harmony.Patch(AccessTools.Method(_beach, nameof(Beach.cleanupBeforePlayerExit), null), postfix: new HarmonyMethod(GetType(), nameof(CleanupBeforePlayerExitPatch)));
            harmony.Patch(AccessTools.Method(_beach, nameof(Beach.draw), new[] { typeof(SpriteBatch) }), postfix: new HarmonyMethod(GetType(), nameof(DrawPatch)));
            harmony.Patch(AccessTools.Method(_beach, nameof(Beach.UpdateWhenCurrentLocation), new[] { typeof(GameTime) }), postfix: new HarmonyMethod(GetType(), nameof(UpdateWhenCurrentLocationPatch)));
        }

        internal static void ResetLocationStatePatch(Beach __instance)
        {
            ModEntry.murphyNPC = new Murphy(new AnimatedSprite(ModResources.murphyTexturePath, 0, 16, 32), new Vector2(89f, 38.5f) * 64f, 2, "Murphy", ModResources.murphyPortraitTexture);
        }

        internal static void CheckActionPatch(Beach __instance, ref bool __result, xTile.Dimensions.Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        {
            if (__result)
            {
                return;
            }

            // Check to see if player is trying to talk to Murphy NPC
            if (ModEntry.murphyNPC != null && ModEntry.murphyNPC.getTileX() == tileLocation.X && ModEntry.murphyNPC.getTileY() == tileLocation.Y && ModEntry.murphyNPC.CurrentDialogue.Count == 0)
            {
                ModEntry.murphyNPC.DisplayDialogue(who);
            }

            // Check to see if player is trying to access Trawler's reward chest
            Tile tile = __instance.map.GetLayer("Buildings").PickTile(new xTile.Dimensions.Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);
            if (tile is null || !tile.Properties.ContainsKey("CustomAction"))
            {
                return;
            }

            switch (tile.Properties["CustomAction"].ToString())
            {
                case "TrawlerRewardStorage":
                    __result = true;

                    if (ModEntry.rewardChest.items.Count() == 0)
                    {
                        Game1.drawDialogueBox("The fishing crate is empty.");
                        break;
                    }

                    __instance.playSound("fishSlap");
                    ModEntry.rewardChest.ShowMenu();
                    break;
                default:
                    break;
            }
        }

        internal static void CleanupBeforePlayerExitPatch(Beach __instance)
        {
            ModEntry.trawlerObject.Reset();
            ModEntry.murphyNPC = null;
        }


        internal static void DrawPatch(Beach __instance, SpriteBatch b)
        {
            Texture2D boatTexture = ModResources.boatTexture;
            if (boatTexture != null)
            {
                b.Draw(boatTexture, Game1.GlobalToLocal(ModEntry.trawlerObject.GetTrawlerPosition()), new Rectangle(0, 16, 224, 160), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                if (ModEntry.trawlerObject._closeGate)
                {
                    b.Draw(boatTexture, Game1.GlobalToLocal(new Vector2(107f, 16f) * 4f + ModEntry.trawlerObject.GetTrawlerPosition()), new Rectangle(251, 32, 18, 15), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.07f);
                }
                else
                {
                    b.Draw(boatTexture, Game1.GlobalToLocal(new Vector2(106f, 7f) * 4f + ModEntry.trawlerObject.GetTrawlerPosition()), new Rectangle(282, 23, 4, 24), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.07f);
                }
            }

            // Draw the Murphy NPC
            if (ModEntry.murphyNPC != null)
            {
                ModEntry.murphyNPC.draw(b);
            }
        }

        internal static void UpdateWhenCurrentLocationPatch(Beach __instance, GameTime time)
        {
            // Update the Murphy NPC
            if (ModEntry.murphyNPC != null)
            {
                ModEntry.murphyNPC.update(time, __instance);
            }

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
                TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 1600, 64, 128), 200f, 9, 1, position, flicker: false, flipped: false, 1f, 0.025f, Color.Gray, 1f, 0.025f, 0f, 0f);
                sprite.acceleration = new Vector2(-0.25f, -0.15f);
                __instance.temporarySprites.Add(sprite);
                trawler._nextSmoke = 0.2f;
            }
        }
    }
}
