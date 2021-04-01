using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingTrawler.Messages
{
    public enum EventType
    {
        Unknown,
        HullHole,
        NetTear,
        EngineFailure,
        BailingWater
    }

    internal class TrawlerEventMessage
    {
        public EventType EventType { get; set; }
        public Vector2 Tile { get; set; }
        public bool IsRepairing { get; set; }

        public TrawlerEventMessage()
        {

        }

        public TrawlerEventMessage(EventType eventType, Vector2 tile, bool isRepairing = false)
        {
            EventType = eventType;
            Tile = tile;
            IsRepairing = isRepairing;
        }
    }
}
