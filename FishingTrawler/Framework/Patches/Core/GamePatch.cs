using FishingTrawler.Framework.Objects.Items.Tools;
using FishingTrawler.Framework.Patches.SMAPI;
using FishingTrawler.GameLocations;
using FishingTrawler.Patches;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;
using xTile.Tiles;

namespace FishingTrawler.Framework.Patches.xTiles
{
    internal class GamePatch : PatchTemplate
    {
        private readonly Type _object = typeof(Game1);

        internal GamePatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal override void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(Game1.drawTool), new[] { typeof(Farmer), typeof(int) }), prefix: new HarmonyMethod(GetType(), nameof(DrawToolPrefix)));
        }

        private static bool DrawToolPrefix(Game1 __instance, Farmer f, int currentToolIndex)
        {
            if (Trident.IsValid(f.CurrentTool))
            {
                Trident.Draw(Game1.spriteBatch, f);

                return false;
            }

            return true;
        }
    }
}
