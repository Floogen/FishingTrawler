using System;
using System.Reflection;
using FishingTrawler.Patches.Locations;
using Harmony;
using StardewModdingAPI;

namespace FishingTrawler
{
    public class ModEntry : Mod
    {
        internal static IMonitor monitor;
        internal static IModHelper modHelper;

        // ModData related
        internal static string offeringsStoredInWaterHutKey;

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor, helper and config
            monitor = Monitor;
            modHelper = helper;
        }
    }
}
