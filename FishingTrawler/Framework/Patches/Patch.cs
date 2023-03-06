using HarmonyLib;
using StardewModdingAPI;

namespace FishingTrawler.Patches
{
    public abstract class Patch
    {
        internal static IMonitor Monitor;

        internal Patch(IMonitor monitor)
        {
            Monitor = monitor;
        }

        internal abstract void Apply(Harmony harmony);
    }
}
