using FishingTrawler.Framework.GameLocations;
using FishingTrawler.Patches;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace FishingTrawler.Framework.Patches.Characters
{
    internal class ScreenFadePatch : PatchTemplate
    {
        private readonly System.Type _object = typeof(ScreenFade);

        public ScreenFadePatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal override void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(ScreenFade), nameof(ScreenFade.UpdateFade), new[] { typeof(GameTime) }), prefix: new HarmonyMethod(GetType(), nameof(UpdateFadePrefix)));
        }

        private static bool UpdateFadePrefix(ScreenFade __instance, ref bool __result, GameTime time)
        {
            if (FishingTrawler.config.disableScreenFade is true && Game1.currentLocation is TrawlerLocation)
            {
                if (__instance.fadeIn)
                {
                    __instance.fadeToBlackAlpha = 2f;
                }
                else
                {
                    __instance.fadeToBlackAlpha = -1f;
                }

                return true;
            }

            return true;
        }
    }
}
