using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FishingTrawler.API;
using FishingTrawler.GameLocations;
using FishingTrawler.Messages;
using FishingTrawler.Objects;
using FishingTrawler.Objects.Rewards;
using FishingTrawler.Objects.Tools;
using FishingTrawler.Patches.Locations;
using FishingTrawler.UI;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;
using xTile.Dimensions;

namespace FishingTrawler
{
    public class ModEntry : Mod
    {
        internal static IMonitor monitor;
        internal static IModHelper modHelper;
        internal static IManifest manifest;
        internal static Multiplayer multiplayer;
        internal static string trawlerThemeSong;
        internal static bool themeSongUpdated;
        internal static Farmer mainDeckhand;
        internal static int numberOfDeckhands;

        // Trawler beach map related
        internal static Murphy murphyNPC;
        internal static Trawler trawlerObject;
        internal static Chest rewardChest;

        // Trawler map / texture related
        private readonly PerScreen<int> fishingTripTimer = new PerScreen<int>();
        private readonly PerScreen<TrawlerHull> _trawlerHull = new PerScreen<TrawlerHull>();
        private readonly PerScreen<TrawlerSurface> _trawlerSurface = new PerScreen<TrawlerSurface>();
        private readonly PerScreen<TrawlerCabin> _trawlerCabin = new PerScreen<TrawlerCabin>();
        private readonly PerScreen<TrawlerRewards> _trawlerRewards = new PerScreen<TrawlerRewards>();
        private PerScreen<bool> _isTripEnding = new PerScreen<bool>();
        private string _trawlerItemsPath = Path.Combine("assets", "TrawlerItems");

        // Location names
        private const string TRAWLER_SURFACE_LOCATION_NAME = "Custom_FishingTrawler";
        private const string TRAWLER_HULL_LOCATION_NAME = "Custom_TrawlerHull";
        private const string TRAWLER_CABIN_LOCATION_NAME = "Custom_TrawlerCabin";

        // Day to appear settings
        internal const int BOAT_DEPART_EVENT_ID = 411203900;
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
            manifest = ModManifest;
            multiplayer = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

            // Load in our assets
            ModResources.SetUpAssets(helper);

            // Initialize the timer for fishing trip
            fishingTripTimer.Value = 0;

            // Set up our notification on the trawler
            _eventSecondInterval = 600;
            _isTripEnding.Value = false;
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
            helper.Events.GameLoop.DayEnding += this.OnDayEnding;

            // Hook into Display related events
            helper.Events.Display.RenderingHud += this.OnRenderingHud;
            helper.Events.Display.RenderedHud += this.OnRenderedHud;

            // Hook into Player related events
            helper.Events.Player.Warped += this.OnWarped;

            // Hook into MouseClicked
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;

            // Hook into Multiplayer related
            helper.Events.Multiplayer.PeerConnected += this.OnPeerConnected;
            helper.Events.Multiplayer.ModMessageReceived += this.OnModMessageReceived;
        }

        private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID != this.ModManifest.UniqueID)
            {
                return;
            }

            switch (e.Type)
            {
                case nameof(DepartureMessage):
                    trawlerObject.TriggerDepartureEvent();
                    break;
                case nameof(TrawlerEventMessage):
                    TrawlerEventMessage message = e.ReadAs<TrawlerEventMessage>();
                    UpdateLocalTrawlerMap(message.EventType, message.Tile, message.IsRepairing);
                    break;
            }
        }

        private void OnPeerConnected(object sender, PeerConnectedEventArgs e)
        {
            if (!Context.IsMainPlayer)
            {
                // Set Farmer moddata used for this mod
                EstablishPlayerData();
            }
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

            TrawlerUI.DrawUI(e.SpriteBatch, fishingTripTimer.Value, _trawlerSurface.Value.fishCaughtQuantity, _trawlerHull.Value.waterLevel, _trawlerHull.Value.HasLeak(), _trawlerSurface.Value.GetRippedNetsCount(), _trawlerCabin.Value.GetLeakingPipesCount());
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            // Check if player just left the trawler
            if (!IsPlayerOnTrawler() && IsValidTrawlerLocation(e.OldLocation))
            {
                if (IsMainDeckhand())
                {
                    // Set the theme to null
                    SetTrawlerTheme(null);

                    numberOfDeckhands = 0;
                    mainDeckhand = null;
                }

                // Take away any bailing buckets
                foreach (BailingBucket bucket in Game1.player.Items.Where(i => i != null && i is BailingBucket))
                {
                    Game1.player.removeItemFromInventory(bucket);
                }

                // Reset the trawler
                _trawlerHull.Value.Reset();
                _trawlerSurface.Value.Reset();
                _trawlerCabin.Value.Reset();

                // Finish trip ending logic
                _isTripEnding.Value = false;

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

                // Clear any previous reward data, set the head deckhand (which determines fishing level for reward calc)
                _trawlerRewards.Value.Reset(Game1.player);

                // Set flag data
                _trawlerSurface.Value.SetFlagTexture(GetHoistedFlag());

                // Start the timer (2.5 minute default)
                fishingTripTimer.Value = 30000; //150000
                _trawlerSurface.Value.fishCaughtQuantity = 100;

                // Apply flag benefits
                switch (GetHoistedFlag())
                {
                    case FlagType.Parley:
                        // Disable all leaks, but reduce fish catch chance by 25% during reward calculations (e.g. more chance of junk / lower quality fish)
                        _trawlerHull.Value.areLeaksEnabled = false;
                        _trawlerRewards.Value.fishCatchChanceOffset = 0.25f;
                        break;
                    case FlagType.JollyRoger:
                        // Quadruples net output 
                        _trawlerSurface.Value.fishCaughtMultiplier = 4;
                        _trawlerHull.Value.hasWeakHull = true;
                        break;
                    case FlagType.GamblersCrest:
                        // 50% of doubling chest, 25% of getting nothing
                        _trawlerRewards.Value.isGambling = true;
                        break;
                    case FlagType.MermaidsBlessing:
                        // 10% of fish getting consumed, but gives random fishing chest reward
                        _trawlerRewards.Value.hasMermaidsBlessing = true;
                        break;
                    case FlagType.PatronSaint:
                        // 25% of fish getting consumed, but gives full XP
                        _trawlerRewards.Value.hasPatronSaint = true;
                        break;
                    case FlagType.SharksFin:
                        // Adds one extra minute to timer, allowing for more fish haul
                        fishingTripTimer.Value += 60000;
                        break;
                    case FlagType.Worldly:
                        // Allows catching of non-ocean fish
                        _trawlerRewards.Value.hasWorldly = true;
                        break;
                    default:
                        // Do nothing
                        break;
                }


                return;
            }
        }

        private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady || !IsPlayerOnTrawler() || _isTripEnding.Value)
            {
                return;
            }

            if (Game1.activeClickableMenu != null && !Context.IsMultiplayer)
            {
                // Allow pausing in singleplayer via menu
                return;
            }

            if (Context.IsMainPlayer)
            {
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
            }

            // Every quarter of a second play leaking sound, if there is a leak
            if (e.IsMultipleOf(15))
            {
                if (Game1.player.currentLocation is TrawlerHull && _trawlerHull.Value.HasLeak())
                {
                    Game1.playSoundPitched("wateringCan", Game1.random.Next(1, 5) * 100);
                }
            }

            if (e.IsMultipleOf(150))
            {
                if (!String.IsNullOrEmpty(_activeNotification))
                {
                    _isNotificationFading = true;
                }

                // Update water level (from leaks) every second
                _trawlerHull.Value.RecaculateWaterLevel();

                if (_trawlerHull.Value.waterLevel == 100)
                {
                    // Reduce fishCaughtQuantity due to failed trip
                    _trawlerSurface.Value.fishCaughtQuantity /= 4;
                }
            }

            if (_trawlerHull.Value.waterLevel == 100)
            {
                Monitor.Log($"Ending trip due to flooding for: {Game1.player.Name}", LogLevel.Warn);

                // Set the status as failed
                Game1.player.modData[MURPHY_WAS_TRIP_SUCCESSFUL_KEY] = "false";
                Game1.player.modData[MURPHY_SAILED_TODAY_KEY] = "true";

                // End trip due to flooding
                Game1.player.currentLocation.playSound("fishEscape");
                Game1.player.CanMove = false;
                Game1.addHUDMessage(new HUDMessage("The ship has taken on too much water! Murphy quickly returns to port before it can sink.", null));

                EndTrip();
            }
        }

        private void OnOneSecondUpdateTicking(object sender, OneSecondUpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady || !IsPlayerOnTrawler() || _isTripEnding.Value)
            {
                return;
            }

            if (Game1.activeClickableMenu != null && !Context.IsMultiplayer)
            {
                // Allow pausing in singleplayer via menu
                return;
            }

            // Iterate the fishing trip timer
            if (fishingTripTimer.Value > 0f)
            {
                fishingTripTimer.Value -= 1000;
            }

            // Update the track if needed
            if (themeSongUpdated)
            {
                themeSongUpdated = false;

                _trawlerCabin.Value.miniJukeboxTrack.Value = String.IsNullOrEmpty(trawlerThemeSong) ? null : trawlerThemeSong;
                _trawlerHull.Value.miniJukeboxTrack.Value = String.IsNullOrEmpty(trawlerThemeSong) ? null : trawlerThemeSong;
                _trawlerSurface.Value.miniJukeboxTrack.Value = String.IsNullOrEmpty(trawlerThemeSong) ? null : trawlerThemeSong;
            }

            // Every 5 seconds recalculate the amount of fish caught / lost
            if (e.IsMultipleOf(300))
            {
                _trawlerSurface.Value.UpdateFishCaught(_trawlerCabin.Value.AreAllPipesLeaking());
            }

            if (IsMainDeckhand())
            {
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
                        // TODO: Sync area changes via broadcasts
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

                    _eventSecondInterval = (uint)Game1.random.Next(1, 5) * 100;
                }
            }

            if (fishingTripTimer.Value <= 0f)
            {
                // Set the status as successful
                Game1.player.modData[MURPHY_WAS_TRIP_SUCCESSFUL_KEY] = "true";
                Game1.player.modData[MURPHY_SAILED_TODAY_KEY] = "true";

                // End trip due to timer finishing
                Game1.player.currentLocation.playSound("trainWhistle");
                Game1.player.CanMove = false;

                Game1.addHUDMessage(new HUDMessage("The trip was a success! Murphy starts heading back to port.", null));

                EndTrip();
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if ((!e.IsDown(SButton.MouseRight) && !e.IsDown(Buttons.A.ToSButton())) || !Context.IsWorldReady || Game1.activeClickableMenu != null)
            {
                return;
            }

            if (e.IsDown(Buttons.A.ToSButton()))
            {
                if (Game1.player.currentLocation.NameOrUniqueName == TRAWLER_HULL_LOCATION_NAME)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        _trawlerHull.Value.AttemptPlugLeak((int)Game1.player.getTileX(), (int)Game1.player.getTileY() - y, Game1.player);
                        BroadcastTrawlerEvent(EventType.HullHole, new Vector2((int)Game1.player.getTileX(), (int)Game1.player.getTileY() - y), true, GetFarmersOnTrawler());
                    }
                }
                else if (Game1.player.currentLocation.NameOrUniqueName == TRAWLER_SURFACE_LOCATION_NAME)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        _trawlerSurface.Value.AttemptFixNet((int)Game1.player.getTileX(), (int)Game1.player.getTileY() - y, Game1.player);
                        BroadcastTrawlerEvent(EventType.NetTear, new Vector2((int)Game1.player.getTileX(), (int)Game1.player.getTileY() - y), true, GetFarmersOnTrawler());
                    }
                }
                else if (Game1.player.currentLocation.NameOrUniqueName == TRAWLER_CABIN_LOCATION_NAME)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        _trawlerCabin.Value.AttemptPlugLeak((int)Game1.player.getTileX(), (int)Game1.player.getTileY() - y, Game1.player);
                        BroadcastTrawlerEvent(EventType.EngineFailure, new Vector2((int)Game1.player.getTileX(), (int)Game1.player.getTileY() - y), true, GetFarmersOnTrawler());
                    }
                }
            }
            else
            {
                if (Game1.player.currentLocation.NameOrUniqueName == TRAWLER_HULL_LOCATION_NAME)
                {
                    _trawlerHull.Value.AttemptPlugLeak((int)e.Cursor.Tile.X, (int)e.Cursor.Tile.Y, Game1.player);
                    BroadcastTrawlerEvent(EventType.HullHole, new Vector2((int)e.Cursor.Tile.X, (int)e.Cursor.Tile.Y), true, GetFarmersOnTrawler());
                }
                else if (Game1.player.currentLocation.NameOrUniqueName == TRAWLER_SURFACE_LOCATION_NAME)
                {
                    // Attempt two checks, in case the user clicks above the rope
                    _trawlerSurface.Value.AttemptFixNet((int)e.Cursor.Tile.X, (int)e.Cursor.Tile.Y, Game1.player);
                    _trawlerSurface.Value.AttemptFixNet((int)e.Cursor.Tile.X, (int)e.Cursor.Tile.Y + 1, Game1.player);

                    BroadcastTrawlerEvent(EventType.NetTear, new Vector2((int)e.Cursor.Tile.X, (int)e.Cursor.Tile.Y), true, GetFarmersOnTrawler());
                    BroadcastTrawlerEvent(EventType.NetTear, new Vector2((int)e.Cursor.Tile.X, (int)e.Cursor.Tile.Y + 1), true, GetFarmersOnTrawler());
                }
                else if (Game1.player.currentLocation.NameOrUniqueName == TRAWLER_CABIN_LOCATION_NAME)
                {
                    _trawlerCabin.Value.AttemptPlugLeak((int)e.Cursor.Tile.X, (int)e.Cursor.Tile.Y, Game1.player);

                    BroadcastTrawlerEvent(EventType.EngineFailure, new Vector2((int)e.Cursor.Tile.X, (int)e.Cursor.Tile.Y), true, GetFarmersOnTrawler());
                }
            }
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // May hook into Content Patcher's API for tokens
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            Beach beach = Game1.getLocationFromName("Beach") as Beach;

            // Set Farmer moddata used for this mod
            EstablishPlayerData();

            if (Context.IsMainPlayer)
            {
                // Must be a Wednesday, the player's fishing level >= 3 and the bridge must be fixed on the beach
                if (!Game1.MasterPlayer.mailReceived.Contains("PeacefulEnd.FishingTrawler_WillyIntroducesMurphy") && Game1.MasterPlayer.FishingLevel >= 3 && beach.bridgeFixed && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == DAY_TO_APPEAR_TOWN)
                {
                    Helper.Content.AssetEditors.Add(new IntroMail());
                    Game1.MasterPlayer.mailbox.Add("PeacefulEnd.FishingTrawler_WillyIntroducesMurphy");
                }

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

                // Create the trawler object for the beach
                // TODO: See how non-split screen multiplayer is handled
                trawlerObject = new Trawler(beach);

                // Reset ownership of boat, deckhands
                mainDeckhand = null;
                numberOfDeckhands = 0;
            }

            // Create the TrawlerReward class
            _trawlerRewards.Value = new TrawlerRewards(rewardChest);

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
            _trawlerHull.Value = Game1.getLocationFromName(TRAWLER_HULL_LOCATION_NAME) as TrawlerHull;
            _trawlerSurface.Value = Game1.getLocationFromName(TRAWLER_SURFACE_LOCATION_NAME) as TrawlerSurface;
            _trawlerCabin.Value = Game1.getLocationFromName(TRAWLER_CABIN_LOCATION_NAME) as TrawlerCabin;
        }

        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            // Offload the custom locations
            Game1.locations.Remove(_trawlerHull.Value);
            Game1.locations.Remove(_trawlerSurface.Value);
            Game1.locations.Remove(_trawlerCabin.Value);
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
                if (!_trawlerSurface.Value.AreAllNetsRipped() && Game1.random.NextDouble() < 0.35)
                {
                    Location tile = _trawlerSurface.Value.GetRandomWorkingNet();

                    _trawlerSurface.Value.AttemptCreateNetRip(tile.X, tile.Y);
                    BroadcastTrawlerEvent(EventType.NetTear, new Vector2(tile.X, tile.Y), false, GetFarmersOnTrawler());

                    possibleMessages.Add(_trawlerSurface.Value.AreAllNetsRipped() && _trawlerCabin.Value.AreAllPipesLeaking() ? MESSAGE_LOSING_FISH : MESSAGE_NET_PROBLEM);

                    executedEvents++;
                    continue;
                }

                if (!_trawlerCabin.Value.AreAllPipesLeaking() && Game1.random.NextDouble() < 0.25)
                {
                    Location tile = _trawlerCabin.Value.GetRandomWorkingPipe();

                    _trawlerCabin.Value.AttemptCreatePipeLeak(tile.X, tile.Y);
                    BroadcastTrawlerEvent(EventType.EngineFailure, new Vector2(tile.X, tile.Y), false, GetFarmersOnTrawler());

                    possibleMessages.Add(_trawlerSurface.Value.AreAllNetsRipped() && _trawlerCabin.Value.AreAllPipesLeaking() ? MESSAGE_LOSING_FISH : MESSAGE_ENGINE_PROBLEM);

                    executedEvents++;
                    continue;
                }

                // Default hull breaking event
                if (!_trawlerHull.Value.AreAllHolesLeaking() && _trawlerHull.Value.areLeaksEnabled)
                {
                    if (_trawlerHull.Value.hasWeakHull)
                    {
                        foreach (Location tile in _trawlerHull.Value.GetAllLeakableLocations())
                        {
                            _trawlerHull.Value.AttemptCreateHullLeak(tile.X, tile.Y);
                            BroadcastTrawlerEvent(EventType.HullHole, new Vector2(tile.X, tile.Y), false, GetFarmersOnTrawler());
                        }
                    }
                    else
                    {
                        Location tile = _trawlerHull.Value.GetRandomPatchedHullHole();

                        _trawlerHull.Value.AttemptCreateHullLeak(tile.X, tile.Y);
                        BroadcastTrawlerEvent(EventType.HullHole, new Vector2(tile.X, tile.Y), false, GetFarmersOnTrawler());
                    }

                    possibleMessages.Add(_trawlerHull.Value.AreAllHolesLeaking() ? MESSAGE_MAX_LEAKS : MESSAGE_LEAK_PROBLEM);

                    executedEvents++;
                    continue;
                }
            }

            // Check if all possible events are activated
            if (_trawlerSurface.Value.AreAllNetsRipped() && _trawlerCabin.Value.AreAllPipesLeaking() && _trawlerHull.Value.AreAllHolesLeaking())
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

        internal static void AlertPlayersOfDeparture(List<Farmer> farmersToAlert)
        {
            if (Context.IsMultiplayer)
            {
                modHelper.Multiplayer.SendMessage(new DepartureMessage(), nameof(DepartureMessage), new[] { manifest.UniqueID }, farmersToAlert.Select(f => f.UniqueMultiplayerID).ToArray());
            }
        }

        internal static void BroadcastTrawlerEvent(EventType eventType, Vector2 locationOfEvent, bool isRepairing, List<Farmer> farmersToAlert)
        {
            if (Context.IsMultiplayer)
            {
                modHelper.Multiplayer.SendMessage(new TrawlerEventMessage(eventType, locationOfEvent, isRepairing), nameof(TrawlerEventMessage), new[] { manifest.UniqueID }, farmersToAlert.Select(f => f.UniqueMultiplayerID).ToArray());
            }
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
            if (Game1.MasterPlayer.mailReceived.Contains("PeacefulEnd.FishingTrawler_WillyIntroducesMurphy") && location is Beach && !Game1.isStartingToGetDarkOut() && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == DAY_TO_APPEAR_TOWN)
            {
                return true;
            }

            return false;
        }

        internal static bool IsMainDeckhand()
        {
            return mainDeckhand != null && mainDeckhand == Game1.player ? true : false;
        }

        internal static FlagType GetHoistedFlag()
        {
            Farmer flagOwner = mainDeckhand != null ? mainDeckhand : Game1.player;
            return Enum.TryParse(flagOwner.modData[HOISTED_FLAG_KEY], out FlagType flagType) ? flagType : FlagType.Unknown;
        }

        internal static void SetHoistedFlag(FlagType flagType)
        {
            Game1.player.modData[HOISTED_FLAG_KEY] = flagType.ToString();
        }

        private void EstablishPlayerData()
        {
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
        }

        internal List<Farmer> GetFarmersOnTrawler()
        {
            return Game1.getAllFarmers().Where(f => IsValidTrawlerLocation(f.currentLocation)).ToList();
        }

        internal void UpdateLocalTrawlerMap(EventType eventType, Vector2 tile, bool isRepairing)
        {
            bool result = false;
            switch (eventType)
            {
                case EventType.HullHole:
                    result = isRepairing ? _trawlerHull.Value.AttemptPlugLeak((int)tile.X, (int)tile.Y, Game1.player, true) : _trawlerHull.Value.AttemptCreateHullLeak((int)tile.X, (int)tile.Y);
                    break;
                case EventType.EngineFailure:
                    result = isRepairing ? _trawlerCabin.Value.AttemptPlugLeak((int)tile.X, (int)tile.Y, Game1.player, true) : _trawlerCabin.Value.AttemptCreatePipeLeak((int)tile.X, (int)tile.Y);
                    break;
                case EventType.NetTear:
                    result = isRepairing ? _trawlerSurface.Value.AttemptFixNet((int)tile.X, (int)tile.Y, Game1.player, true) : _trawlerSurface.Value.AttemptCreateNetRip((int)tile.X, (int)tile.Y);
                    break;
                default:
                    monitor.Log($"A trawler event tried to sync, but its EventType was not handled: {eventType}", LogLevel.Debug);
                    break;
            }
        }

        internal void EndTrip()
        {
            // Give the player(s) their rewards, if they left the trawler as expected (warping out early does not give any rewards)
            _trawlerRewards.Value.CalculateAndPopulateReward(_trawlerSurface.Value.fishCaughtQuantity);

            DelayedAction.warpAfterDelay("Beach", new Point(86, 38), 2500);

            _isTripEnding.Value = true;
        }
    }
}
