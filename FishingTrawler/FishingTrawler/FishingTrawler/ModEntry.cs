using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FishingTrawler.API;
using FishingTrawler.GameLocations;
using FishingTrawler.Objects;
using FishingTrawler.Objects.Rewards;
using FishingTrawler.Objects.Tools;
using FishingTrawler.Patches.Locations;
using FishingTrawler.UI;
using Harmony;
using Microsoft.Xna.Framework;
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
        internal static int fishingTripTimer;
        internal static string trawlerThemeSong;
        internal static bool themeSongUpdated;

        // FlagType
        private static FlagType _hoistedFlag;

        // Trawler beach map related
        internal static Murphy murphyNPC;
        internal static Trawler trawlerObject;
        internal static Chest rewardChest;

        // Trawler map / texture related
        private TrawlerHull _trawlerHull;
        private TrawlerSurface _trawlerSurface;
        private TrawlerCabin _trawlerCabin;
        private string _trawlerItemsPath = Path.Combine("assets", "TrawlerItems");

        // Location names
        private const string TRAWLER_SURFACE_LOCATION_NAME = "Custom_FishingTrawler";
        private const string TRAWLER_HULL_LOCATION_NAME = "Custom_TrawlerHull";
        private const string TRAWLER_CABIN_LOCATION_NAME = "Custom_TrawlerCabin";

        // Day to appear settings
        private const string DAY_TO_APPEAR_TOWN = "Wed";
        private const string DAY_TO_APPEAR_ISLAND = "Sun";

        // Mod data related
        private const string REWARD_CHEST_DATA_KEY = "PeacefulEnd.FishingTrawler_RewardChest";
        internal const string MURPHY_WAS_GREETED_TODAY_KEY = "PeacefulEnd.FishingTrawler_MurphyGreeted";
        internal const string MURPHY_SAILED_TODAY_KEY = "PeacefulEnd.FishingTrawler_MurphySailedToday";
        internal const string MURPHY_WAS_TRIP_SUCCESSFUL_KEY = "PeacefulEnd.FishingTrawler_MurphyTripSuccessful";
        internal const string MURPHY_FINISHED_TALKING_KEY = "PeacefulEnd.FishingTrawler_MurphyFinishedTalking";
        internal const string MURPHY_HAS_SEEN_FLAG_KEY = "PeacefulEnd.FishingTrawler_MurphyHasSeenFlag";

        internal const string BAILING_BUCKET_KEY = "PeacefulEnd.FishingTrawler_BailingBucket";
        internal const string ANCIENT_FLAG_KEY = "PeacefulEnd.FishingTrawler_AncientFlag";

        internal const string HOISTED_FLAG_KEY = "PeacefulEnd.FishingTrawler_HoistedFlag";

        // Notificiation messages
        private readonly KeyValuePair<string, int> MESSAGE_EVERYTHING_FAILING = new KeyValuePair<string, int>("This ship is falling apart!", 10);
        private readonly KeyValuePair<string, int> MESSAGE_LOSING_FISH = new KeyValuePair<string, int>("We're losing fish!", 9);
        private readonly KeyValuePair<string, int> MESSAGE_MAX_LEAKS = new KeyValuePair<string, int>("We're taking on water!", 8);
        private readonly KeyValuePair<string, int> MESSAGE_MULTI_PROBLEMS = new KeyValuePair<string, int>("We've got lots of problems!", 7);
        private readonly KeyValuePair<string, int> MESSAGE_ENGINE_PROBLEM = new KeyValuePair<string, int>("The engine is failing!", 7);
        private readonly KeyValuePair<string, int> MESSAGE_NET_PROBLEM = new KeyValuePair<string, int>("The nets are torn!", 6);
        private readonly KeyValuePair<string, int> MESSAGE_LEAK_PROBLEM = new KeyValuePair<string, int>("We've got a leak!", 5);

        // Notification related
        private uint _eventSecondInterval;
        private bool _isTripEnding;
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
            ModResources.SetUpAssets(helper);

            // Initialize the timer for fishing trip
            fishingTripTimer = 0;

            // Set up our notification on the trawler
            _eventSecondInterval = 600;
            _isTripEnding = false;
            _activeNotification = String.Empty;
            _notificationAlpha = 1f;
            _isNotificationFading = false;

            // Load our Harmony patches
            try
            {
                var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

                // Apply our patches
                new BeachPatch(monitor).Apply(harmony);
                new GameLocationPatch(monitor).Apply(harmony);
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
                // Give the player their rewards, if they left the trawler as expected (warping out early does not give any rewards)
                if (_isTripEnding)
                {
                    TrawlerRewards.CalculateAndPopulateReward(rewardChest, _trawlerSurface.fishCaughtQuantity, e.Player);
                }

                // Reset the trawler
                _trawlerHull.Reset();
                _trawlerSurface.Reset();
                _trawlerCabin.Reset();

                // Take away any bailing buckets
                foreach (BailingBucket bucket in Game1.player.Items.Where(i => i != null && i is BailingBucket))
                {
                    Game1.player.removeItemFromInventory(bucket);
                }

                // Set the theme to null
                SetTrawlerTheme(null);

                // Finish trip ending logic
                _isTripEnding = false;

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
                    Game1.addHUDMessage(new HUDMessage("A bailing bucket has been added to your inventory.", null));
                }

                // Set flag data
                _trawlerSurface.SetFlagTexture(_hoistedFlag);

                // Start the timer (2.5 minute default)
                fishingTripTimer = 150000;

                return;
            }
        }

        private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady || !IsPlayerOnTrawler() || Game1.activeClickableMenu != null || _isTripEnding)
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

                if (_trawlerHull.waterLevel == 100)
                {
                    // Set the status as failed
                    Game1.player.modData[MURPHY_WAS_TRIP_SUCCESSFUL_KEY] = "false";
                    Game1.player.modData[MURPHY_SAILED_TODAY_KEY] = "true";

                    // End trip due to flooding
                    Game1.player.currentLocation.playSound("fishEscape");
                    Game1.player.CanMove = false;
                    Game1.addHUDMessage(new HUDMessage("The ship has taken on too much water! Murphy quickly returns to port before it can sink.", null));
                    DelayedAction.warpAfterDelay("Beach", new Point(86, 38), 2500);

                    // Reduce fishCaughtQuantity due to failed trip
                    _trawlerSurface.fishCaughtQuantity /= 4;

                    _isTripEnding = true;
                    return;
                }
            }
        }

        private void OnOneSecondUpdateTicking(object sender, OneSecondUpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady || !IsPlayerOnTrawler() || Game1.activeClickableMenu != null || _isTripEnding)
            {
                return;
            }

            // Iterate the fishing trip timer
            if (fishingTripTimer > 0f)
            {
                fishingTripTimer -= 1000;
            }
            else
            {
                // Set the status as successful
                Game1.player.modData[MURPHY_WAS_TRIP_SUCCESSFUL_KEY] = "true";
                Game1.player.modData[MURPHY_SAILED_TODAY_KEY] = "true";

                // End trip due to timer finishing
                Game1.player.currentLocation.playSound("trainWhistle");
                Game1.player.CanMove = false;
                Game1.addHUDMessage(new HUDMessage("The trip was a success! Murphy starts heading back to port.", null));
                DelayedAction.warpAfterDelay("Beach", new Point(86, 38), 2000);

                _isTripEnding = true;
                return;
            }

            // Update the track if needed
            if (themeSongUpdated)
            {
                themeSongUpdated = false;

                _trawlerCabin.miniJukeboxTrack.Value = String.IsNullOrEmpty(trawlerThemeSong) ? null : trawlerThemeSong;
                _trawlerHull.miniJukeboxTrack.Value = String.IsNullOrEmpty(trawlerThemeSong) ? null : trawlerThemeSong;
                _trawlerSurface.miniJukeboxTrack.Value = String.IsNullOrEmpty(trawlerThemeSong) ? null : trawlerThemeSong;
            }

            // Every 5 seconds recalculate the amount of fish caught / lost
            if (e.IsMultipleOf(300))
            {
                _trawlerSurface.UpdateFishCaught(_trawlerCabin.AreAllPipesLeaking());
            }

            // Every random interval check for new event (leak, net tearing, etc.) on Trawler
            if (e.IsMultipleOf(_eventSecondInterval))
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

                _eventSecondInterval = (uint)Game1.random.Next(2, 6) * 100;
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
            Beach beach = Game1.getLocationFromName("Beach") as Beach;

            // Must be a Wednesday, the player's fishing level >= 3 and the bridge must be fixed on the beach
            if (!Game1.player.mailReceived.Contains("PeacefulEnd.FishingTrawler_WillyIntroducesMurphy") && Game1.MasterPlayer.FishingLevel >= 3 && beach.bridgeFixed && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == DAY_TO_APPEAR_TOWN)
            {
                Helper.Content.AssetEditors.Add(new IntroMail());
                Game1.MasterPlayer.mailbox.Add("PeacefulEnd.FishingTrawler_WillyIntroducesMurphy");
            }

            // Create the trawler object for the beach
            trawlerObject = new Trawler(beach);

            // Set the reward chest
            Vector2 rewardChestPosition = new Vector2(-100, -100);
            Farm farm = Game1.getLocationFromName("Farm") as Farm;
            rewardChest = farm.objects.Values.FirstOrDefault(o => o.modData.ContainsKey(REWARD_CHEST_DATA_KEY)) as Chest;
            if (rewardChest is null)
            {
                Monitor.Log($"Creating reward chest {rewardChestPosition}", LogLevel.Trace);
                rewardChest = new Chest(true, rewardChestPosition) { Name = "Trawler Rewards" };
                rewardChest.modData.Add(REWARD_CHEST_DATA_KEY, "true");

                farm.setObject(rewardChestPosition, rewardChest);
            }

            // Set Farmer moddata used for this mod
            if (!Game1.player.modData.ContainsKey(HOISTED_FLAG_KEY))
            {
                Game1.player.modData.Add(HOISTED_FLAG_KEY, FlagType.Unknown.ToString());
            }
            else
            {
                SetHoistedFlag(Enum.TryParse(Game1.player.modData[HOISTED_FLAG_KEY], out FlagType flagType) ? flagType : FlagType.Unknown);
            }

            if (!Game1.player.modData.ContainsKey(MURPHY_WAS_GREETED_TODAY_KEY))
            {
                Game1.player.modData.Add(MURPHY_WAS_GREETED_TODAY_KEY, "false");
            }
            else if (Game1.player.modData[MURPHY_WAS_GREETED_TODAY_KEY].ToLower() == "true")
            {
                Game1.player.modData[MURPHY_WAS_GREETED_TODAY_KEY] = "false";
            }

            if (!Game1.player.modData.ContainsKey(MURPHY_SAILED_TODAY_KEY))
            {
                Game1.player.modData.Add(MURPHY_SAILED_TODAY_KEY, "false");
                Game1.player.modData.Add(MURPHY_WAS_TRIP_SUCCESSFUL_KEY, "false");
                Game1.player.modData.Add(MURPHY_FINISHED_TALKING_KEY, "false");
            }
            else if (Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == DAY_TO_APPEAR_TOWN || Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == DAY_TO_APPEAR_ISLAND)
            {
                Game1.player.modData[MURPHY_SAILED_TODAY_KEY] = "false";
                Game1.player.modData[MURPHY_WAS_TRIP_SUCCESSFUL_KEY] = "false";
                Game1.player.modData[MURPHY_FINISHED_TALKING_KEY] = "false";
            }

            // One time event, do not renew
            if (!Game1.player.modData.ContainsKey(MURPHY_HAS_SEEN_FLAG_KEY))
            {
                Game1.player.modData.Add(MURPHY_HAS_SEEN_FLAG_KEY, "false");
            }

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
            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                ModResources.ConvertInventoryBaseItemsToCustom(farmer);
            }

            // Check every location for a chest and then re-add any previous BailingBuckets
            foreach (GameLocation location in Game1.locations)
            {
                ModResources.ConvertBaseItemsToCustom(location);

                if (location is BuildableGameLocation)
                {
                    foreach (Building building in (location as BuildableGameLocation).buildings)
                    {
                        GameLocation indoorLocation = building.indoors.Value;
                        if (indoorLocation is null)
                        {
                            continue;
                        }

                        ModResources.ConvertBaseItemsToCustom(indoorLocation);
                    }
                }
            }
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            // Save the current hoisted flag
            Game1.player.modData[HOISTED_FLAG_KEY] = _hoistedFlag.ToString();

            // Offload the custom locations
            Game1.locations.Remove(_trawlerHull);
            Game1.locations.Remove(_trawlerSurface);
            Game1.locations.Remove(_trawlerCabin);

            // Note: This shouldn't be necessary, as the player shouldn't normally be able to take the BailingBucket outside the Trawler
            // However, in the situations it does happen this will prevent crashes

            // For every player, replace any BailingBucket with MilkPail
            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                ModResources.ConvertInventoryCustomItemsToBase(farmer);
            }

            // Check every location for a chest and then replace any BailingBucket with MilkPail
            foreach (GameLocation location in Game1.locations)
            {
                ModResources.ConvertCustomItemsToBase(location);

                if (location is BuildableGameLocation)
                {
                    foreach (Building building in (location as BuildableGameLocation).buildings)
                    {
                        GameLocation indoorLocation = building.indoors.Value;
                        if (indoorLocation is null)
                        {
                            continue;
                        }

                        ModResources.ConvertCustomItemsToBase(indoorLocation);
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

        internal static bool ShouldMurphyAppear(GameLocation location)
        {
            if (Game1.player.mailReceived.Contains("PeacefulEnd.FishingTrawler_WillyIntroducesMurphy") && location is Beach && !Game1.isStartingToGetDarkOut() && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == DAY_TO_APPEAR_TOWN)
            {
                return true;
            }

            return false;
        }

        internal static FlagType GetHoistedFlag()
        {
            return _hoistedFlag;
        }

        internal static void SetHoistedFlag(FlagType flagType)
        {
            // Updating the player's modData for which flag is hoisted
            _hoistedFlag = flagType;

            // TODO: Apply flag benefits

        }
    }
}
