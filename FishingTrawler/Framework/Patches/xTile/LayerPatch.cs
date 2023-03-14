using FishingTrawler.Framework.Patches.SMAPI;
using FishingTrawler.GameLocations;
using FishingTrawler.Patches;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;
using xTile.Tiles;

namespace FishingTrawler.Framework.Patches.xTiles
{
    internal class LayerPatch : PatchTemplate
    {
        private readonly Type _object = typeof(Layer);

        internal LayerPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal override void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(Layer.Draw), new[] { typeof(IDisplayDevice), typeof(xTile.Dimensions.Rectangle), typeof(xTile.Dimensions.Location), typeof(bool), typeof(int) }), postfix: new HarmonyMethod(GetType(), nameof(DrawPostfix)));
        }

        private static void DrawPostfix(Layer __instance, IDisplayDevice displayDevice, xTile.Dimensions.Rectangle mapViewport, Location displayOffset, bool wrapAround, int pixelZoom)
        {
            if (__instance is null || String.IsNullOrEmpty(__instance.Id))
            {
                return;
            }

            // Handle Fishing Trawler specific maps
            if (Game1.currentLocation is TrawlerHull trawlerHull)
            {
                if (__instance.Id.Equals("Back", StringComparison.OrdinalIgnoreCase) is true)
                {
                    var floodLayer = trawlerHull.Map.GetLayer("FloodWater");
                    if (floodLayer.Properties.TryGetValue("@Opacity", out var opacityProperty) && float.TryParse(opacityProperty, out float opacityValue))
                    {
                        DisplayDevicePatch.Opacity = opacityValue;
                    }
                    floodLayer.Draw(displayDevice, mapViewport, displayOffset, wrapAround, pixelZoom);

                    DisplayDevicePatch.Opacity = null;

                    var splashLayer = trawlerHull.Map.GetLayer("WaterSplash");
                    splashLayer.Draw(displayDevice, mapViewport, displayOffset, wrapAround, pixelZoom);
                }
                else if (__instance.Id.Equals("Buildings", StringComparison.OrdinalIgnoreCase) is true)
                {
                    var floodItems = trawlerHull.Map.GetLayer("FloodItems");
                    if (floodItems.Properties.TryGetValue("@Opacity", out var opacityProperty) && float.TryParse(opacityProperty, out float opacityValue))
                    {
                        DisplayDevicePatch.Opacity = opacityValue;
                    }
                    floodItems.Draw(displayDevice, mapViewport, displayOffset, wrapAround, pixelZoom);

                    DisplayDevicePatch.Opacity = null;
                }
            }
            else if (Game1.currentLocation is TrawlerSurface trawlerSurface)
            {
                if (__instance.Id.Equals("Back", StringComparison.OrdinalIgnoreCase) is true)
                {
                    trawlerSurface.Map.GetLayer("WaterFlow").Draw(displayDevice, mapViewport, displayOffset, wrapAround, pixelZoom);
                }
                if (__instance.Id.Equals("AlwaysFront", StringComparison.OrdinalIgnoreCase) is true)
                {
                    trawlerSurface.Map.GetLayer("Flags").Draw(displayDevice, mapViewport, displayOffset, wrapAround, pixelZoom);
                }
            }
        }
    }
}
