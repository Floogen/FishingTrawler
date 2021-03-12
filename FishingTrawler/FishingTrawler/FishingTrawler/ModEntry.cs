using System;
using System.IO;
using System.Reflection;
using FishingTrawler.Patches.Locations;
using Harmony;
using StardewModdingAPI;
using StardewValley;

namespace FishingTrawler
{
    public class ModEntry : Mod
    {
        internal static IMonitor monitor;
        internal static IModHelper modHelper;

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor, helper and config
            monitor = Monitor;
            modHelper = helper;

            // Load in our assets
            ModResources.SetUpAssets(helper);

            // Load our Harmony patches
            try
            {
                var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

                // Apply our patches
                new BeachPatch(monitor).Apply(harmony);
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }
        }
    }
}
