using System;
using System.IO;
using System.Reflection;
using FishingTrawler.GameLocations;
using FishingTrawler.Patches.Locations;
using Harmony;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace FishingTrawler
{
    public class ModEntry : Mod
    {
        internal static IMonitor monitor;
        internal static IModHelper modHelper;

        // API related
        //IContentPatcherAPI contentPatcherApi;

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

            // Hook into GameLaunched event
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;

            // Hook into SaveLoaded
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // Add the surface location
            TrawlerSurface surfaceLocation = new TrawlerSurface(this.Helper.Content.GetActualAssetKey(Path.Combine("assets", "FishingTrawler.tmx"), ContentSource.ModFolder), "Custom_FishingTrawler") { IsOutdoors = true, IsFarm = false };
            Game1.locations.Add(surfaceLocation);

            // Add the hull location
            TrawlerHull hullLocation = new TrawlerHull(this.Helper.Content.GetActualAssetKey(Path.Combine("assets", "TrawlerHull.tmx"), ContentSource.ModFolder), "Custom_TrawlerHull") { IsOutdoors = false, IsFarm = false };
            Game1.locations.Add(hullLocation);
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {

        }
    }
}
