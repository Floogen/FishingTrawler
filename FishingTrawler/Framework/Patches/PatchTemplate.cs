using HarmonyLib;
using StardewModdingAPI;

namespace FishingTrawler.Patches
{
    internal abstract class PatchTemplate
    {
        internal static IMonitor _monitor;
        internal static IModHelper _helper;

        internal PatchTemplate(IMonitor modMonitor, IModHelper modHelper)
        {
            _monitor = modMonitor;
            _helper = modHelper;
        }

        internal abstract void Apply(Harmony harmony);
    }
}
