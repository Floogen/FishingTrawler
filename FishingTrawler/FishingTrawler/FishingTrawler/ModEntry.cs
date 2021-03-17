using System;
using System.IO;
using System.Reflection;
using FishingTrawler.API;
using FishingTrawler.GameLocations;
using FishingTrawler.Patches.Locations;
using Harmony;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;

namespace FishingTrawler
{
    public class ModEntry : Mod
    {
        internal static IMonitor monitor;
        internal static IModHelper modHelper;

        private TrawlerHull trawlerHull;
        private TrawlerSurface trawlerSurface;

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

            // Hook into UpdateTicking
            helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking; ;

            // Hook into OneSecondUpdateTicking
            helper.Events.GameLoop.OneSecondUpdateTicking += this.OnOneSecondUpdateTicking;

            // Hook into GameLaunched event
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;

            // Hook into SaveLoaded
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;

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
                trawlerHull.Reset();
                trawlerSurface.Reset();

                return;
            }

            // Check if player just entered the trawler
            if (IsPlayerOnTrawler() && !IsValidTrawlerLocation(e.OldLocation))
            {
                // Give them a bailing bucket
                Game1.player.addItemToInventory(new MeleeWeapon(ApiManager.GetBailingBucketID()));
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
                if (Game1.player.currentLocation is TrawlerHull && trawlerHull.HasLeak())
                {
                    trawlerHull.playSoundPitched("wateringCan", Game1.random.Next(1, 5) * 100);
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
                trawlerHull.UpdateWaterLevel();
                trawlerSurface.UpdateFishCaught();
            }

            // Every 10 seconds check for new event (leak, net tearing, etc.) on Trawler
            if (e.IsMultipleOf(600))
            {
                // TODO: Base of Game1.random (10% probability?)
                trawlerHull.AttemptCreateHullLeak();
                trawlerSurface.AttemptCreateNetRip();
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

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // Add the surface location
            TrawlerSurface surfaceLocation = new TrawlerSurface(this.Helper.Content.GetActualAssetKey(Path.Combine("assets", "FishingTrawler.tmx"), ContentSource.ModFolder), TRAWLER_SURFACE_LOCATION_NAME) { IsOutdoors = true, IsFarm = false };
            Game1.locations.Add(surfaceLocation);

            // Add the hull location
            TrawlerHull hullLocation = new TrawlerHull(this.Helper.Content.GetActualAssetKey(Path.Combine("assets", "TrawlerHull.tmx"), ContentSource.ModFolder), TRAWLER_HULL_LOCATION_NAME) { IsOutdoors = false, IsFarm = false };
            Game1.locations.Add(hullLocation);

            // Verify our locations were added and establish our location variables
            trawlerHull = Game1.getLocationFromName(TRAWLER_HULL_LOCATION_NAME) as TrawlerHull;
            trawlerSurface = Game1.getLocationFromName(TRAWLER_SURFACE_LOCATION_NAME) as TrawlerSurface;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {

        }

        private bool IsPlayerOnTrawler()
        {
            return IsValidTrawlerLocation(Game1.player.currentLocation);
        }

        private bool IsValidTrawlerLocation(GameLocation location)
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
