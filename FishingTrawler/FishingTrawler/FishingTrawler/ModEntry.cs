using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FishingTrawler.API;
using FishingTrawler.GameLocations;
using FishingTrawler.Objects.Tools;
using FishingTrawler.Patches.Locations;
using FishingTrawler.UI;
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
        internal static int fishingTripTimer;
        internal static string trawlerThemeSong;
        internal static bool themeSongUpdated;

        // Trawler map / texture related
        private TrawlerHull _trawlerHull;
        private TrawlerSurface _trawlerSurface;
        private TrawlerCabin _trawlerCabin;
        private string _trawlerItemsPath = Path.Combine("assets", "TrawlerItems");

        // Location names
        private const string TRAWLER_SURFACE_LOCATION_NAME = "Custom_FishingTrawler";
        private const string TRAWLER_HULL_LOCATION_NAME = "Custom_TrawlerHull";
        private const string TRAWLER_CABIN_LOCATION_NAME = "Custom_TrawlerCabin";

        // Notificiation messages
        private readonly KeyValuePair<string, int> MESSAGE_EVERYTHING_FAILING = new KeyValuePair<string, int>("This ship is falling apart!", 10);
        private readonly KeyValuePair<string, int> MESSAGE_LOSING_FISH = new KeyValuePair<string, int>("We're losing fish!", 9);
        private readonly KeyValuePair<string, int> MESSAGE_MAX_LEAKS = new KeyValuePair<string, int>("We're taking on water!", 8);
        private readonly KeyValuePair<string, int> MESSAGE_MULTI_PROBLEMS = new KeyValuePair<string, int>("We've got lots of problems!", 7);
        private readonly KeyValuePair<string, int> MESSAGE_NET_PROBLEM = new KeyValuePair<string, int>("The nets are torn!", 6);
        private readonly KeyValuePair<string, int> MESSAGE_ENGINE_PROBLEM = new KeyValuePair<string, int>("The engine is failing!", 6);
        private readonly KeyValuePair<string, int> MESSAGE_LEAK_PROBLEM = new KeyValuePair<string, int>("We've got a leak!", 5);

        // Notification related
        private bool _isNotificationFading;
        private float _notificationAlpha;
        private string _activeNotification;

        // API related
        //IContentPatcherAPI contentPatcherApi;

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor and helper
            monitor = Monitor;
            modHelper = helper;

            // Load in our assets
            bailingBucketKey = $"{ModManifest.UniqueID}/bailing-bucket";
            ModResources.SetUpAssets(helper);

            // Initialize the timer for fishing trip
            fishingTripTimer = 0;

            // Set up our notification on the trawler
            _activeNotification = String.Empty;
            _notificationAlpha = 1f;
            _isNotificationFading = false;

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

            // Hook into Display related events
            helper.Events.Display.RenderingHud += this.OnRenderingHud;
            helper.Events.Display.RenderedHud += this.OnRenderedHud;

            // Hook into Player related events
            helper.Events.Player.Warped += this.OnWarped;

            // Hook into MouseClicked
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        private void OnRenderingHud(object sender, RenderingHudEventArgs e)
        {
            if (!IsPlayerOnTrawler())
            {
                return;
            }

            if (!String.IsNullOrEmpty(_activeNotification))
            {
                TrawlerUI.DrawNotification(e.SpriteBatch, Game1.player.currentLocation, _activeNotification, _notificationAlpha);
            }
        }

        private void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            if (!IsPlayerOnTrawler())
            {
                return;
            }

            TrawlerUI.DrawUI(e.SpriteBatch, fishingTripTimer, _trawlerSurface.fishCaughtQuantity, _trawlerHull.waterLevel, _trawlerHull.HasLeak(), _trawlerSurface.GetRippedNetsCount(), _trawlerCabin.GetLeakingPipesCount());
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

                // Set the theme to null
                SetTrawlerTheme(null);

                return;
            }

            // Check if player just entered the trawler
            if (IsPlayerOnTrawler() && !IsValidTrawlerLocation(e.OldLocation))
            {
                // Set the default track
                Game1.changeMusicTrack("fieldofficeTentMusic");

                // Give them a bailing bucket
                if (!Game1.player.items.Any(i => i is BailingBucket))
                {
                    Game1.player.addItemToInventory(new BailingBucket());
                }

                // Start the timer (2.5 minute default)
                fishingTripTimer = 150000;

                return;
            }
        }

        private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady || !IsPlayerOnTrawler() || Game1.activeClickableMenu != null)
            {
                return;
            }

            if (_isNotificationFading)
            {
                _notificationAlpha -= 0.1f;
            }

            if (_notificationAlpha < 0f)
            {
                _activeNotification = String.Empty;
                _isNotificationFading = false;
                _notificationAlpha = 1f;
            }

            // Every quarter of a second play leaking sound, if there is a leak
            if (e.IsMultipleOf(15))
            {
                if (Game1.player.currentLocation is TrawlerHull && _trawlerHull.HasLeak())
                {
                    _trawlerHull.playSoundPitched("wateringCan", Game1.random.Next(1, 5) * 100);
                }
            }

            if (e.IsMultipleOf(150))
            {
                if (!String.IsNullOrEmpty(_activeNotification))
                {
                    _isNotificationFading = true;
                }

                // Update water level (from leaks) every second
                _trawlerHull.RecaculateWaterLevel();

                // Every 5 seconds recalculate the amount of fish caught / lost
                _trawlerSurface.UpdateFishCaught(_trawlerCabin.AreAllPipesLeaking());
            }
        }

        private void OnOneSecondUpdateTicking(object sender, OneSecondUpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady || !IsPlayerOnTrawler() || Game1.activeClickableMenu != null)
            {
                return;
            }

            // Iterate the fishing trip timer
            if (fishingTripTimer > 0f)
            {
                fishingTripTimer -= 1000;
            }

            // Update the track if needed
            if (themeSongUpdated)
            {
                themeSongUpdated = false;

                _trawlerHull.miniJukeboxTrack.Value = String.IsNullOrEmpty(trawlerThemeSong) ? null : trawlerThemeSong;
                _trawlerSurface.miniJukeboxTrack.Value = String.IsNullOrEmpty(trawlerThemeSong) ? null : trawlerThemeSong;
            }

            // Every 10 seconds check for new event (leak, net tearing, etc.) on Trawler
            if (e.IsMultipleOf(600))
            {
                string message = String.Empty;

                // Check if the player gets lucky and skips getting an event, otherwise create the event(s)
                if (Game1.random.NextDouble() < 0.05)
                {
                    message = "The sea favors us today!";
                }
                else
                {
                    message = CreateTrawlerEventsAndGetMessage();
                }

                // Check for empty string 
                if (String.IsNullOrEmpty(message))
                {
                    message = "Ah the smell of the sea...";
                }

                if (_activeNotification != message)
                {
                    _activeNotification = message;
                }
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!e.IsDown(SButton.MouseRight) || !Context.IsWorldReady || Game1.activeClickableMenu != null)
            {
                return;
            }

            if (Game1.player.currentLocation.NameOrUniqueName == TRAWLER_HULL_LOCATION_NAME)
            {
                _trawlerHull.AttemptPlugLeak((int)e.Cursor.Tile.X, (int)e.Cursor.Tile.Y, Game1.player);
            }
            else if (Game1.player.currentLocation.NameOrUniqueName == TRAWLER_SURFACE_LOCATION_NAME)
            {
                // Attempt two checks, in case the user clicks above the rope
                _trawlerSurface.AttemptFixNet((int)e.Cursor.Tile.X, (int)e.Cursor.Tile.Y, Game1.player);
                _trawlerSurface.AttemptFixNet((int)e.Cursor.Tile.X, (int)e.Cursor.Tile.Y + 1, Game1.player);
            }
            else if (Game1.player.currentLocation.NameOrUniqueName == TRAWLER_CABIN_LOCATION_NAME)
            {
                _trawlerCabin.AttemptPlugLeak((int)e.Cursor.Tile.X, (int)e.Cursor.Tile.Y, Game1.player);
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

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // Add the surface location
            TrawlerSurface surfaceLocation = new TrawlerSurface(Path.Combine(ModResources.assetFolderPath, "Maps", "FishingTrawler.tmx"), TRAWLER_SURFACE_LOCATION_NAME) { IsOutdoors = true, IsFarm = false };
            Game1.locations.Add(surfaceLocation);

            // Add the hull location
            TrawlerHull hullLocation = new TrawlerHull(Path.Combine(ModResources.assetFolderPath, "Maps", "TrawlerHull.tmx"), TRAWLER_HULL_LOCATION_NAME) { IsOutdoors = false, IsFarm = false };
            Game1.locations.Add(hullLocation);

            // Add the cabin location
            TrawlerCabin cabinLocation = new TrawlerCabin(Path.Combine(ModResources.assetFolderPath, "Maps", "TrawlerCabin.tmx"), TRAWLER_CABIN_LOCATION_NAME) { IsOutdoors = false, IsFarm = false };
            Game1.locations.Add(cabinLocation);

            // Verify our locations were added and establish our location variables
            _trawlerHull = Game1.getLocationFromName(TRAWLER_HULL_LOCATION_NAME) as TrawlerHull;
            _trawlerSurface = Game1.getLocationFromName(TRAWLER_SURFACE_LOCATION_NAME) as TrawlerSurface;
            _trawlerCabin = Game1.getLocationFromName(TRAWLER_CABIN_LOCATION_NAME) as TrawlerCabin;

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
            Game1.locations.Remove(_trawlerCabin);

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

        private string CreateTrawlerEventsAndGetMessage()
        {
            int amountOfEvents = 0;
            for (int x = 0; x < 4; x++)
            {
                // Chance of skipping an event increases with each pass of this loop
                if (Game1.random.NextDouble() < 0.1 + (x * 0.1f))
                {
                    // Skip event
                    continue;
                }

                amountOfEvents++;
            }

            int executedEvents = 0;
            List<KeyValuePair<string, int>> possibleMessages = new List<KeyValuePair<string, int>>();
            for (int x = 0; x < amountOfEvents; x++)
            {
                if (!_trawlerSurface.AreAllNetsRipped() && Game1.random.NextDouble() < 0.35)
                {
                    _trawlerSurface.AttemptCreateNetRip();
                    possibleMessages.Add(_trawlerSurface.AreAllNetsRipped() && _trawlerCabin.AreAllPipesLeaking() ? MESSAGE_LOSING_FISH : MESSAGE_NET_PROBLEM);

                    executedEvents++;
                    continue;
                }

                if (!_trawlerCabin.AreAllPipesLeaking() && Game1.random.NextDouble() < 0.25)
                {
                    _trawlerCabin.AttemptCreatePipeLeak();
                    possibleMessages.Add(_trawlerSurface.AreAllNetsRipped() && _trawlerCabin.AreAllPipesLeaking() ? MESSAGE_LOSING_FISH : MESSAGE_ENGINE_PROBLEM);

                    executedEvents++;
                    continue;
                }

                // Default hull breaking event
                if (!_trawlerHull.AreAllHolesLeaking())
                {
                    _trawlerHull.AttemptCreateHullLeak();
                    possibleMessages.Add(_trawlerHull.AreAllHolesLeaking() ? MESSAGE_MAX_LEAKS : MESSAGE_LEAK_PROBLEM);

                    executedEvents++;
                    continue;
                }
            }

            // Check if all possible events are activated
            if (_trawlerSurface.AreAllNetsRipped() && _trawlerCabin.AreAllPipesLeaking() && _trawlerHull.AreAllHolesLeaking())
            {
                possibleMessages.Add(MESSAGE_EVERYTHING_FAILING);
            }

            // Add a generic message if there are lots of issues
            if (executedEvents > 1)
            {
                possibleMessages.Add(MESSAGE_MULTI_PROBLEMS);
            }

            // Select highest priority item (priority == default_priority_level * frequency)
            return amountOfEvents == 0 ? "Yoba be praised!" : possibleMessages.OrderByDescending(m => m.Value * possibleMessages.Count(p => p.Key == m.Key)).FirstOrDefault().Key;
        }

        internal static void SetTrawlerTheme(string songName)
        {
            trawlerThemeSong = songName;
            themeSongUpdated = true;
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
                case TrawlerCabin cabin:
                    return true;
                default:
                    return false;
            }
        }
    }
}
