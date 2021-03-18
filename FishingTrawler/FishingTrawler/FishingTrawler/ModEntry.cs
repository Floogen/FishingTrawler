using System;
using System.IO;
using System.Linq;
using System.Reflection;
using FishingTrawler.API;
using FishingTrawler.GameLocations;
using FishingTrawler.Objects.Tools;
using FishingTrawler.Patches.Locations;
using Harmony;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;

namespace FishingTrawler
{
    public class ModEntry : Mod
    {
        internal static IMonitor monitor;
        internal static IModHelper modHelper;
        internal static string bailingBucketKey;

        private TrawlerHull _trawlerHull;
        private TrawlerSurface _trawlerSurface;
        private string _trawlerItemsPath = Path.Combine("assets", "TrawlerItems");

        private const string TRAWLER_SURFACE_LOCATION_NAME = "Custom_FishingTrawler";
        private const string TRAWLER_HULL_LOCATION_NAME = "Custom_TrawlerHull";

        // API related
        //IContentPatcherAPI contentPatcherApi;

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor, helper and config
            monitor = Monitor;
            modHelper = helper;

            // Load in our assets
            bailingBucketKey = $"{ModManifest.UniqueID}/bailing-bucket";
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

            // Hook into GameLoops related events
            helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking; ;
            helper.Events.GameLoop.OneSecondUpdateTicking += this.OnOneSecondUpdateTicking;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.Saving += this.OnSaving;

            // Hook into Player related events
            helper.Events.Player.Warped += this.OnWarped;

            // Hook into MouseClicked
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            // Check if player just left the trawler
            if (!IsPlayerOnTrawler() && IsValidTrawlerLocation(e.OldLocation))
            {
                // Reset the trawler
                _trawlerHull.Reset();
                _trawlerSurface.Reset();

                // Take away any bailing buckets
                foreach (BailingBucket bucket in Game1.player.Items.Where(i => i != null && i is BailingBucket))
                {
                    Game1.player.removeItemFromInventory(bucket);
                }

                return;
            }

            // Check if player just entered the trawler
            if (IsPlayerOnTrawler() && !IsValidTrawlerLocation(e.OldLocation))
            {
                // Give them a bailing bucket
                if (!Game1.player.items.Any(i => i is BailingBucket))
                {
                    Game1.player.addItemToInventory(new BailingBucket());
                }

                return;
            }
        }

        private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady || !IsPlayerOnTrawler())
            {
                return;
            }

            // Every quarter of a second play leaking sound, if there is a leak
            if (e.IsMultipleOf(15))
            {
                if (Game1.player.currentLocation is TrawlerHull && _trawlerHull.HasLeak())
                {
                    _trawlerHull.playSoundPitched("wateringCan", Game1.random.Next(1, 5) * 100);
                }
            }
        }

        private void OnOneSecondUpdateTicking(object sender, OneSecondUpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady || !IsPlayerOnTrawler())
            {
                return;
            }

            // Every 5 seconds recalculate the water level (from leaks), amount of fish caught
            // TODO: (when player bails the water level will update outside this timer)
            if (e.IsMultipleOf(300))
            {
                // TODO: Base of Game1.random (10% probability?)
                _trawlerHull.RecaculateWaterLevel();
                _trawlerSurface.UpdateFishCaught();
            }

            // Every 10 seconds check for new event (leak, net tearing, etc.) on Trawler
            if (e.IsMultipleOf(600))
            {
                // TODO: Base of Game1.random (10% probability?)
                _trawlerHull.AttemptCreateHullLeak();
                _trawlerSurface.AttemptCreateNetRip();
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!e.IsDown(SButton.MouseRight) || !Context.IsWorldReady)
            {
                return;
            }

            if (Game1.player.currentLocation.NameOrUniqueName == TRAWLER_HULL_LOCATION_NAME)
            {
                TrawlerHull hullLocation = Game1.player.currentLocation as TrawlerHull;
                hullLocation.AttemptPlugLeak((int)e.Cursor.Tile.X, (int)e.Cursor.Tile.Y, Game1.player);
            }
            else if (Game1.player.currentLocation.NameOrUniqueName == TRAWLER_SURFACE_LOCATION_NAME)
            {
                TrawlerSurface surfaceLocation = Game1.player.currentLocation as TrawlerSurface;

                // Attempt two checks, in case the user clicks above the rope
                surfaceLocation.AttemptFixNet((int)e.Cursor.Tile.X, (int)e.Cursor.Tile.Y, Game1.player);
                surfaceLocation.AttemptFixNet((int)e.Cursor.Tile.X, (int)e.Cursor.Tile.Y + 1, Game1.player);
            }
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // Add the surface location
            TrawlerSurface surfaceLocation = new TrawlerSurface(Path.Combine(ModResources.assetFolderPath, "Maps", "FishingTrawler.tmx"), TRAWLER_SURFACE_LOCATION_NAME) { IsOutdoors = true, IsFarm = false };
            Game1.locations.Add(surfaceLocation);

            // Add the hull location
            TrawlerHull hullLocation = new TrawlerHull(Path.Combine(ModResources.assetFolderPath, "Maps", "TrawlerHull.tmx"), TRAWLER_HULL_LOCATION_NAME) { IsOutdoors = false, IsFarm = false };
            Game1.locations.Add(hullLocation);

            // Verify our locations were added and establish our location variables
            _trawlerHull = Game1.getLocationFromName(TRAWLER_HULL_LOCATION_NAME) as TrawlerHull;
            _trawlerSurface = Game1.getLocationFromName(TRAWLER_SURFACE_LOCATION_NAME) as TrawlerSurface;

            // Note: This shouldn't be necessary, as the player shouldn't normally be able to take the BailingBucket outside the Trawler
            // However, in the situations it does happen this will prevent crashes

            // For every player, add the BailingBucket Tool (if they had previously)
            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                foreach (MilkPail pail in farmer.Items.Where(i => i != null && i.modData.ContainsKey(bailingBucketKey)))
                {
                    farmer.removeItemFromInventory(pail);
                    farmer.addItemToInventory(new BailingBucket() { modData = pail.modData });
                }
            }

            // Check every location for a chest and then re-add any previous BailingBuckets
            foreach (GameLocation location in Game1.locations)
            {
                ConvertStoredBailingBuckets(location, true);

                if (location is BuildableGameLocation)
                {
                    foreach (Building building in (location as BuildableGameLocation).buildings)
                    {
                        GameLocation indoorLocation = building.indoors.Value;
                        if (indoorLocation is null)
                        {
                            continue;
                        }

                        ConvertStoredBailingBuckets(indoorLocation, true);
                    }
                }
            }
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            // Offload the custom locations
            Game1.locations.Remove(_trawlerHull);
            Game1.locations.Remove(_trawlerSurface);

            // Note: This shouldn't be necessary, as the player shouldn't normally be able to take the BailingBucket outside the Trawler
            // However, in the situations it does happen this will prevent crashes

            // For every player, replace any BailingBucket with MilkPail
            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                foreach (BailingBucket bucket in farmer.Items.Where(i => i != null && i is BailingBucket))
                {
                    farmer.removeItemFromInventory(bucket);
                    farmer.addItemToInventory(new MilkPail() { modData = bucket.modData });
                }
            }

            // Check every location for a chest and then replace any BailingBucket with MilkPail
            foreach (GameLocation location in Game1.locations)
            {
                ConvertStoredBailingBuckets(location);

                if (location is BuildableGameLocation)
                {
                    foreach (Building building in (location as BuildableGameLocation).buildings)
                    {
                        GameLocation indoorLocation = building.indoors.Value;
                        if (indoorLocation is null)
                        {
                            continue;
                        }

                        ConvertStoredBailingBuckets(indoorLocation);
                    }
                }
            }
        }

        private void ConvertStoredBailingBuckets(GameLocation location, bool inverse = false)
        {
            foreach (Chest chest in location.Objects.Pairs.Where(o => o.Value != null && o.Value is Chest).Select(o => o.Value))
            {
                if (inverse)
                {
                    foreach (MilkPail pail in chest.items.Where(i => i != null && i.modData.ContainsKey(bailingBucketKey)).ToList())
                    {
                        chest.items.Remove(pail);
                        chest.items.Add(new BailingBucket() { modData = pail.modData });
                    }
                }
                else
                {
                    foreach (BailingBucket bucket in chest.items.Where(i => i != null && i is BailingBucket).ToList())
                    {
                        chest.items.Remove(bucket);
                        chest.items.Add(new MilkPail() { modData = bucket.modData });
                    }
                }
            }
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Check if spacechase0's JsonAssets is in the current mod list
            if (Helper.ModRegistry.IsLoaded("spacechase0.JsonAssets"))
            {
                Monitor.Log("Attempting to hook into spacechase0.JsonAssets.", LogLevel.Debug);
                ApiManager.HookIntoJsonAssets(Helper);

                // Add the bailing bucket asset (weapon) and rewards
                ApiManager.GetJsonAssetInterface().LoadAssets(Path.Combine(Helper.DirectoryPath, _trawlerItemsPath));
            }
        }

        internal static bool IsPlayerOnTrawler()
        {
            return IsValidTrawlerLocation(Game1.player.currentLocation);
        }

        private static bool IsValidTrawlerLocation(GameLocation location)
        {
            switch (location)
            {
                case TrawlerSurface surface:
                case TrawlerHull hull:
                    return true;
                default:
                    return false;
            }
        }
    }
}
