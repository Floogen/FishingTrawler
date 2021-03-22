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
        }

        internal void StartDeparture()
        {
            xTile.Dimensions.Rectangle viewport = Game1.viewport;
            Vector2 player_position = Game1.player.Position;
            int player_direction = Game1.player.facingDirection;
            StringBuilder event_string = new StringBuilder();
            event_string.Append("/-1000 -1000/farmer 0 0 0/playMusic none/skippable");
            event_string.Append("/fade/warp farmer -100 -100/pause 1000/playSound furnace/locationSpecificCommand animate_boat_start/locationSpecificCommand non_blocking_pause 1000");
            event_string.Append("/locationSpecificCommand boat_depart/fade/viewport -5000 -5000/end position 87 35");
            //event_string.Append("/move Willy 0 -3 0/pause 500/locationSpecificCommand open_gate/viewport move 0 -1 1000/pause 500/move Willy 0 -2 3/move Willy -1 0 1/locationSpecificCommand path_player 6 5 2/move Willy 1 0 2/move Willy 0 1 2/pause 250/playSound clubhit/animate Willy false false 500 27/locationSpecificCommand retract_plank/jump Willy 4/pause 750/move Willy 0 -1 0/locationSpecificCommand close_gate/pause 200/move Willy 3 0 1/locationSpecificCommand offset_willy/move Willy 1 0 1");
            //event_string.Append("/locationSpecificCommand non_blocking_pause 1000/playerControl boatRide/playSound furnace/locationSpecificCommand animate_boat_start/locationSpecificCommand non_blocking_pause 1000/locationSpecificCommand boat_depart/locationSpecificCommand animate_boat_move/fade/viewport -5000 -5000/end tunnelDepart");
            _boatEvent = new Event(event_string.ToString(), 411203900, Game1.player) // TODO: Change the first four digits to the mod's Nexus ID
            {
                showWorldCharacters = true,
                showGroundObjects = true,
                ignoreObjectCollisions = false
            };
            event_string = null;
            Event boatEvent = this._boatEvent;
            boatEvent.onEventFinished = (Action)Delegate.Combine(boatEvent.onEventFinished, new Action(OnBoatEventEnd));
            _beach.currentEvent = this._boatEvent;
            _boatEvent.checkForNextCommand(_beach, Game1.currentGameTime);
            Game1.warpFarmer("Custom_TrawlerCabin", 8, 5, 0);
            Game1.eventUp = true;
            Game1.viewport = viewport;
            _farmerActor = _beach.currentEvent.getCharacterByName("farmer") as Farmer;
            _farmerActor.Position = player_position;
            _farmerActor.faceDirection(player_direction);
            //TODO: Use this for Murphy(_beach.currentEvent.getCharacterByName("Willy") as NPC).IsInvisible = false;
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

            return;
            // TODO: Implement this letter for details on rewards / flags?
            if (!Game1.player.hasOrWillReceiveMail("FishingTrawler_goneOnTrawlerTrip"))
            {
                Game1.addMailForTomorrow("FishingTrawler_goneOnTrawlerTrip");
            }
        }

    }
}
