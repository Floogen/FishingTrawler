using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingTrawler.Messages
{
    internal class TrawlerNotificationMessage
    {
        public string Notification { get; set; }

        public TrawlerNotificationMessage()
        {

        }

        public TrawlerNotificationMessage(string notification)
        {
            Notification = notification;
        }
    }
}
