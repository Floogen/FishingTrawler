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
        }

        internal static void CleanupBeforePlayerExitPatch(Beach __instance)
        {
            Trawler.Reset();
        }


        internal static void DrawPatch(Beach __instance, SpriteBatch b)
        {
            Texture2D boatTexture = ModResources.boatTexture;
            if (boatTexture is null)
            {
                return;
            }

            b.Draw(boatTexture, Game1.GlobalToLocal(Trawler.GetTrawlerPosition()), new Rectangle(0, 16, 224, 160), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 720f / 10000f);
            // Overlay for glass port
            //b.Draw(boatTexture, Game1.GlobalToLocal(Game1.viewport, Trawler.GetTrawlerPosition() + new Vector2(8f, 0f) * 4f), new Rectangle(0, 160, 128, 96), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (720f + 408f) / 10000f);
        }
    }
}
