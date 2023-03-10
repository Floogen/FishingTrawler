﻿using FishingTrawler.Framework.Utilities;
using FishingTrawler.GameLocations;
using FishingTrawler.Messages;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile.Dimensions;

namespace FishingTrawler.Framework.Managers
{
    public class EventManager
    {
        private IMonitor _monitor;

        // Intervals
        private uint _fishUpdateInterval = 300;
        private uint _floodUpdateInterval = 180;
        private uint _fuelUpdateInterval = 300;
        private uint _eventSecondInterval = 600;

        // Split screen data
        private readonly PerScreen<int> _fishingTripTimer = new PerScreen<int>();

        // Notificiation messages
        private KeyValuePair<string, int> MESSAGE_EVERYTHING_FAILING;
        private KeyValuePair<string, int> MESSAGE_LOSING_FISH;
        private KeyValuePair<string, int> MESSAGE_MAX_LEAKS;
        private KeyValuePair<string, int> MESSAGE_MULTI_PROBLEMS;
        private KeyValuePair<string, int> MESSAGE_ENGINE_PROBLEM;
        private KeyValuePair<string, int> MESSAGE_NET_PROBLEM;
        private KeyValuePair<string, int> MESSAGE_LEAK_PROBLEM;

        public EventManager(IMonitor monitor)
        {
            _monitor = monitor;

            // Set up notification messages
            MESSAGE_EVERYTHING_FAILING = new KeyValuePair<string, int>(FishingTrawler.i18n.Get("status_message.ship_falling_apart"), 10);
            MESSAGE_LOSING_FISH = new KeyValuePair<string, int>(FishingTrawler.i18n.Get("status_message.losing_fish"), 9);
            MESSAGE_MAX_LEAKS = new KeyValuePair<string, int>(FishingTrawler.i18n.Get("status_message.taking_on_water"), 8);
            MESSAGE_MULTI_PROBLEMS = new KeyValuePair<string, int>(FishingTrawler.i18n.Get("status_message.lots_of_problems"), 7);
            MESSAGE_ENGINE_PROBLEM = new KeyValuePair<string, int>(FishingTrawler.i18n.Get("status_message.engine_failing"), 7);
            MESSAGE_NET_PROBLEM = new KeyValuePair<string, int>(FishingTrawler.i18n.Get("status_message.nets_torn"), 6);
            MESSAGE_LEAK_PROBLEM = new KeyValuePair<string, int>(FishingTrawler.i18n.Get("status_message.leak"), 5);
        }

        internal void UpdateEvents(UpdateTickingEventArgs e, TrawlerCabin trawlerCabin, TrawlerSurface trawlerSurface, TrawlerHull trawlerHull)
        {
            // Every second, update the trip trimer
            if (e.IsOneSecond)
            {
                IncrementTripTimer(-1000);
            }

            // Every x seconds recalculate the amount of fish caught / lost
            if (e.IsMultipleOf(_fishUpdateInterval))
            {
                // TODO: Change UpdateFishCaught method to account for bonus fish at 50% > fuel, standard fish at 50% <= and no fish at 0%
                /* 
                 * Engine States:
                 * Fuel > 50% | Each working net gives one extra fish
                 * Fuel <= 50% | Each working net give standard fish count (no bonus)
                 * Fuel == 0% | Each working net gives 0 fish
                 * 
                 * Coal can be stacked up to three times by clicking the coal box three times
                 * Each coal gives 10% fuel, with a full stack giving a bonus 5%
                 */

                /* 
                 * Guidance Computer States:
                 * Discovering | Computer is looking for a new trail to pursue
                 * Awaiting Input | Computer is waiting for user to interact with it. Once interacted, the fishing trip will be extended by X seconds.
                 */
                trawlerSurface.UpdateFishCaught(trawlerHull.IsEngineFailing());
                FishingTrawler.SyncTrawler(SyncType.FishCaught, trawlerSurface.fishCaughtQuantity, FishingTrawler.GetFarmersOnTrawler());
            }

            // Every x seconds recalculate the flood level
            if (e.IsMultipleOf(_floodUpdateInterval))
            {
                // Update water level (from leaks) every second
                trawlerHull.RecalculateWaterLevel();
                FishingTrawler.SyncTrawler(SyncType.WaterLevel, trawlerHull.GetWaterLevel(), FishingTrawler.GetFarmersOnTrawler());
            }

            // Every x seconds recalculate the fuel level
            if (e.IsMultipleOf(_fuelUpdateInterval))
            {
                trawlerHull.AdjustFuelLevel(-10);
            }

            // Every random interval check for new event (leak, net tearing, etc.) on Trawler
            if (e.IsMultipleOf(_eventSecondInterval))
            {
                string message = string.Empty;

                // Check if the player gets lucky and skips getting an event, otherwise create the event(s)
                if (Game1.random.NextDouble() < 0.05)
                {
                    message = FishingTrawler.i18n.Get("status_message.sea_favors_us");
                }
                else
                {
                    message = CreateTrawlerEventsAndGetMessage(trawlerCabin, trawlerSurface, trawlerHull);
                }

                // Check for empty string 
                if (string.IsNullOrEmpty(message))
                {
                    message = FishingTrawler.i18n.Get("status_message.default");
                }
                FishingTrawler.notificationManager.SetNotification(message);

                _eventSecondInterval = (uint)Game1.random.Next(FishingTrawler.config.eventFrequencyLower, FishingTrawler.config.eventFrequencyUpper + 1) * 100;
            }

            // Update the track if needed
            if (FishingTrawler.themeSongUpdated)
            {
                FishingTrawler.themeSongUpdated = false;

                trawlerCabin.miniJukeboxTrack.Value = string.IsNullOrEmpty(FishingTrawler.trawlerThemeSong) ? null : FishingTrawler.trawlerThemeSong;
                trawlerHull.miniJukeboxTrack.Value = string.IsNullOrEmpty(FishingTrawler.trawlerThemeSong) ? null : FishingTrawler.trawlerThemeSong;
                trawlerSurface.miniJukeboxTrack.Value = string.IsNullOrEmpty(FishingTrawler.trawlerThemeSong) ? null : FishingTrawler.trawlerThemeSong;
            }
        }

        internal int GetTripTimer()
        {
            return _fishingTripTimer.Value;
        }

        internal void SetTripTimer(int milliseconds)
        {
            _fishingTripTimer.Value = milliseconds;
        }

        internal void IncrementTripTimer(int milliseconds)
        {
            _fishingTripTimer.Value += milliseconds;
        }

        private string CreateTrawlerEventsAndGetMessage(TrawlerCabin trawlerCabin, TrawlerSurface trawlerSurface, TrawlerHull trawlerHull)
        {
            int amountOfEvents = 0;
            for (int x = 0; x < 4; x++)
            {
                // Chance of skipping an event increases with each pass of this loop
                if (Game1.random.NextDouble() < 0.1 + x * 0.1f)
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
                if (!trawlerSurface.AreAllNetsRipped() && Game1.random.NextDouble() < 0.35)
                {
                    Location? tile = trawlerSurface.GetRandomWorkingNet();
                    if (tile is null)
                    {
                        continue;
                    }

                    trawlerSurface.AttemptCreateNetRip(tile.Value.X, tile.Value.Y);
                    FishingTrawler.BroadcastTrawlerEvent(EventType.NetTear, new Vector2(tile.Value.X, tile.Value.Y), false, FishingTrawler.GetFarmersOnTrawler());

                    possibleMessages.Add(trawlerSurface.AreAllNetsRipped() ? MESSAGE_LOSING_FISH : MESSAGE_NET_PROBLEM);

                    executedEvents++;
                    continue;
                }

                // Default hull breaking event
                if (!trawlerHull.AreAllHolesLeaking() && trawlerHull.areLeaksEnabled)
                {
                    if (trawlerHull.hasWeakHull)
                    {
                        foreach (Location tile in trawlerHull.GetAllLeakableLocations())
                        {
                            trawlerHull.AttemptCreateHullLeak(tile.X, tile.Y);
                            FishingTrawler.BroadcastTrawlerEvent(EventType.HullHole, new Vector2(tile.X, tile.Y), false, FishingTrawler.GetFarmersOnTrawler());
                        }
                    }
                    else
                    {
                        Location tile = trawlerHull.GetRandomPatchedHullHole();

                        trawlerHull.AttemptCreateHullLeak(tile.X, tile.Y);
                        FishingTrawler.BroadcastTrawlerEvent(EventType.HullHole, new Vector2(tile.X, tile.Y), false, FishingTrawler.GetFarmersOnTrawler());
                    }

                    possibleMessages.Add(trawlerHull.AreAllHolesLeaking() ? MESSAGE_MAX_LEAKS : MESSAGE_LEAK_PROBLEM);

                    executedEvents++;
                    continue;
                }
            }

            // Check if all possible events are activated
            if (trawlerSurface.AreAllNetsRipped() && trawlerHull.IsEngineFailing() && trawlerHull.AreAllHolesLeaking())
            {
                possibleMessages.Add(MESSAGE_EVERYTHING_FAILING);
            }

            // Add a generic message if there are lots of issues
            if (executedEvents > 1)
            {
                possibleMessages.Add(MESSAGE_MULTI_PROBLEMS);
            }

            // Select highest priority item (priority == default_priority_level * frequency)
            return amountOfEvents == 0 ? FishingTrawler.i18n.Get("status_message.yoba_be_praised") : possibleMessages.OrderByDescending(m => m.Value * possibleMessages.Count(p => p.Key == m.Key)).FirstOrDefault().Key;
        }

    }
}