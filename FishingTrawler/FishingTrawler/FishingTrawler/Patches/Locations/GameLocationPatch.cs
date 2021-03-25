﻿using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;
using FishingTrawler.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingTrawler.Patches.Locations
{
    public class GameLocationPatch : Patch
    {
        private readonly Type _gameLocation = typeof(GameLocation);

        internal GameLocationPatch(IMonitor monitor) : base(monitor)
        {

        }

        internal override void Apply(HarmonyInstance harmony)
        {

            harmony.Patch(AccessTools.Method(_gameLocation, nameof(GameLocation.RunLocationSpecificEventCommand), new[] { typeof(Event), typeof(string), typeof(bool), typeof(string[]) }), postfix: new HarmonyMethod(GetType(), nameof(RunLocationSpecificEventCommandPatch)));
            harmony.Patch(AccessTools.Method(_gameLocation, nameof(GameLocation.performTouchAction), new[] { typeof(string), typeof(Vector2) }), postfix: new HarmonyMethod(GetType(), nameof(PerformTouchActionPatch)));
            harmony.Patch(AccessTools.Method(_gameLocation, nameof(GameLocation.isActionableTile), new[] { typeof(int), typeof(int), typeof(Farmer) }), postfix: new HarmonyMethod(GetType(), nameof(IsActionableTilePatch)));
        }

        internal static void RunLocationSpecificEventCommandPatch(GameLocation __instance, ref bool __result, Event current_event, string command_string, bool first_run, params string[] args)
        {
            if (!(__instance is Beach))
            {
                return;
            }

            switch (command_string)
            {
                case "animate_boat_start":
                    ModEntry.trawlerObject._boatAnimating = true;
                    __result = true;
                    return;
                case "non_blocking_pause":
                    if (first_run)
                    {
                        int delay = 0;
                        if (args.Length < 0 || !int.TryParse(args[0], out delay))
                        {
                            delay = 0;
                        }
                        ModEntry.trawlerObject.nonBlockingPause = delay;
                        __result = false;
                        return;
                    }
                    ModEntry.trawlerObject.nonBlockingPause -= (int)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
                    if (ModEntry.trawlerObject.nonBlockingPause < 0)
                    {
                        ModEntry.trawlerObject.nonBlockingPause = 0;
                        __result = true;
                        return;
                    }
                    __result = false;
                    return;
                case "boat_depart":
                    if (first_run)
                    {
                        ModEntry.trawlerObject._boatDirection = 1;
                    }
                    if (ModEntry.trawlerObject._boatOffset >= 150)
                    {
                        __result = true;
                        return;
                    }
                    __result = false;
                    return;
                case "close_gate":
                    ModEntry.trawlerObject._closeGate = true;
                    __result = true;
                    return;
                case "despawn_murphy":
                    if (ModEntry.murphyNPC != null)
                    {
                        ModEntry.murphyNPC = null;
                    }
                    __result = true;
                    return;
            }
        }

        internal static void PerformTouchActionPatch(GameLocation __instance, string fullActionString, Vector2 playerStandingPosition)
        {
            if (Game1.eventUp || !(__instance is Beach))
            {
                return;
            }

            if (fullActionString == "FishingTrawler_AttemptBoard")
            {
                Game1.drawDialogueBox("You should speak to the captain before boarding his ship!");
            }
        }

        internal static void IsActionableTilePatch(GameLocation __instance, ref bool __result, int xTile, int yTile, Farmer who)
        {
            if (__result || !(__instance is Beach))
            {
                return;
            }

            string actionProperty = __instance.doesTileHaveProperty(xTile, yTile, "CustomAction", "Buildings");
            if (actionProperty != null && actionProperty == "TrawlerRewardStorage")
            {
                if (!Enumerable.Range(who.getTileX() - 1, 3).Contains(xTile) || !Enumerable.Range(who.getTileY() - 1, 3).Contains(yTile))
                {
                    Game1.mouseCursorTransparency = 0.5f;
                }

                __result = true;
            }

            if (ModEntry.murphyNPC != null && ModEntry.murphyNPC.getTileX() == xTile && ModEntry.murphyNPC.getTileY() == yTile)
            {
                if (!Utility.tileWithinRadiusOfPlayer(xTile, yTile, 1, who))
                {
                    Game1.mouseCursorTransparency = 0.5f;
                }

                Game1.isSpeechAtCurrentCursorTile = true;
                __result = true;
            }
        }
    }
}