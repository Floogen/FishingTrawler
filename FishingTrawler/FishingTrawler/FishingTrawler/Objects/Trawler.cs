using Microsoft.Xna.Framework;
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
        internal Farmer _farmerActor;
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

        internal void StartDeparture()
        {
            List<Farmer> farmersToDepart = _beach.farmers.ToList();

            xTile.Dimensions.Rectangle viewport = Game1.viewport;
            Vector2 player_position = Game1.player.Position;
            int player_direction = Game1.player.facingDirection;

            _boatEvent = _beach.findEventById(ModEntry.BOAT_DEPART_EVENT_ID); // TODO: Change the first four digits to the mod's Nexus ID
            _boatEvent.showWorldCharacters = false;
            _boatEvent.showGroundObjects = true;
            _boatEvent.ignoreObjectCollisions = false;

            Event boatEvent = this._boatEvent;
            boatEvent.onEventFinished = (Action)Delegate.Combine(boatEvent.onEventFinished, new Action(OnBoatEventEnd));
            _beach.currentEvent = this._boatEvent;
            _boatEvent.checkForNextCommand(_beach, Game1.currentGameTime);

            Game1.eventUp = true;
            Game1.viewport = viewport;
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

        internal static void WarpToCabinAtEnd()
        {
            Game1.warpFarmer("Custom_TrawlerCabin", 8, 5, false);
        }
    }
}
