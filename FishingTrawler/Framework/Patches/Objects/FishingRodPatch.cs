using FishingTrawler.Framework.Objects.Items.Rewards;
using FishingTrawler.Patches;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley.Tools;

namespace FishingTrawler.Framework.Patches.Objects
{
    internal class FishingRodPatch : PatchTemplate
    {
        private readonly System.Type _object = typeof(FishingRod);

        public FishingRodPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal override void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, "doDoneFishing", new[] { typeof(bool) }), prefix: new HarmonyMethod(GetType(), nameof(DoDoneFishingPrefix)));
        }

        private static bool DoDoneFishingPrefix(FishingRod __instance, bool consumeBaitAndTackle)
        {
            if (__instance.attachments[1] != null && SeaborneTackle.IsValid(__instance.attachments[1]))
            {
                __instance.attachments[1].uses.Value = int.MinValue;
            }

            return true;
        }
    }
}
