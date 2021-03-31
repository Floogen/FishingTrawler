using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingTrawler.Objects
{
    internal class Trawler
    {
        internal Vector2 _boatPosition;
        internal int _boatDirection;
        internal int _boatOffset;
        internal Event _boatEvent;
        internal int nonBlockingPause;
        internal float _nextBubble;
        internal float _nextSlosh;
        internal float _nextSmoke;
        internal bool _boatAnimating;
        internal bool _closeGate;
        internal readonly GameLocation _beach;

        public Trawler()
        {

        }

        public Trawler(GameLocation beachLocation)
        {
            _beach = beachLocation;
            this._boatPosition = new Vector2(80f, 41f) * 64f;
        }

        internal Vector2 GetTrawlerPosition()
        {
            // Enable this line to test moving to the right:
            //_boatOffset++;
            return _boatPosition + new Vector2(_boatOffset, 0f);
        }

        internal void Reset()
        {
            this._nextSmoke = 0f;
            this._nextBubble = 0f;
            this._boatAnimating = false;
            this._boatPosition = new Vector2(80f, 41f) * 64f;
            this._boatOffset = 0;
            this._boatDirection = 0;
            this._closeGate = false;
        }

        internal void TriggerDepartureEvent()
        {
            string id = _beach.currentEvent is null ? "Empty" : _beach.currentEvent.id.ToString();
            ModEntry.monitor.Log($"Starting event for {Game1.player.Name}: {_beach.currentEvent is null} | {id}", LogLevel.Trace);

            if (Context.IsMultiplayer)
            {
                // Force close menu
                if (Game1.player.hasMenuOpen)
                {
                    Game1.activeClickableMenu = null;
                }

                Game1.player.locationBeforeForcedEvent.Value = "Custom_TrawlerCabin";
                Farmer farmerActor = (Game1.player.NetFields.Root as NetRoot<Farmer>).Clone().Value;

                Action performForcedEvent = delegate
                {
                    Game1.warpingForForcedRemoteEvent = true;
                    Game1.player.completelyStopAnimatingOrDoingAction();

                    farmerActor.currentLocation = _beach;
                    farmerActor.completelyStopAnimatingOrDoingAction();
                    farmerActor.UsingTool = false;
                    farmerActor.items.Clear();
                    farmerActor.hidden.Value = false;
                    Event @event = Game1.currentLocation.findEventById(ModEntry.BOAT_DEPART_EVENT_ID, farmerActor);
                    @event.showWorldCharacters = false;
                    @event.showGroundObjects = true;
                    @event.ignoreObjectCollisions = false;
                    Game1.currentLocation.startEvent(@event);
                    Game1.warpingForForcedRemoteEvent = false;
                    string value = Game1.player.locationBeforeForcedEvent.Value;
                    Game1.player.locationBeforeForcedEvent.Value = null;
                    @event.setExitLocation("Custom_TrawlerCabin", 8, 5);
                    Game1.player.locationBeforeForcedEvent.Value = value;
                    Game1.player.orientationBeforeEvent = 0;
                };
                Game1.remoteEventQueue.Add(performForcedEvent);

                return;
            }

            _boatEvent = _beach.findEventById(ModEntry.BOAT_DEPART_EVENT_ID, Game1.player); // TODO: Change the first four digits to the mod's Nexus ID
            _boatEvent.showWorldCharacters = false;
            _boatEvent.showGroundObjects = true;
            _boatEvent.ignoreObjectCollisions = false;
            _boatEvent.setExitLocation("Custom_TrawlerCabin", 8, 5);
            Game1.player.locationBeforeForcedEvent.Value = "Custom_TrawlerCabin";

            Event boatEvent = this._boatEvent;
            boatEvent.onEventFinished = (Action)Delegate.Combine(boatEvent.onEventFinished, new Action(OnBoatEventEnd));
            _beach.currentEvent = this._boatEvent;
            _boatEvent.checkForNextCommand(_beach, Game1.currentGameTime);

            Game1.eventUp = true;
        }

        internal void StartDeparture()
        {
            List<Farmer> farmersToDepart = GetFarmersToDepart();

            ModEntry.claimedBoat = true;
            ModEntry.numberOfDeckhands = farmersToDepart.Count();
            ModEntry.monitor.Log($"There are {farmersToDepart.Count()} farm hands departing!", LogLevel.Trace);

            TriggerDepartureEvent();

            if (Context.IsMultiplayer)
            {
                // Send out trigger event to relevant players
                ModEntry.AlertPlayersOfDeparture(farmersToDepart);
            }
        }

        internal void OnBoatEventEnd()
        {
            if (this._boatEvent == null)
            {
                return;
            }
            foreach (NPC actor in this._boatEvent.actors)
            {
                actor.shouldShadowBeOffset = false;
                actor.drawOffset.X = 0f;
            }
            foreach (Farmer farmerActor in this._boatEvent.farmerActors)
            {
                farmerActor.shouldShadowBeOffset = false;
                farmerActor.drawOffset.X = 0f;
            }
            this.Reset();
            this._boatEvent = null;
        }

        internal List<Farmer> GetFarmersToDepart(bool excludeThisPlayer = false)
        {
            Rectangle zoneOfDeparture = new Rectangle(82, 26, 10, 16);
            return _beach.farmers.Where(f => zoneOfDeparture.Contains(f.getTileX(), f.getTileY()) && !excludeThisPlayer || (excludeThisPlayer && f != Game1.player)).ToList();
        }
    }
}
