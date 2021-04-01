using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingTrawler.Messages
{
    internal class DepartureMessage
    {
        public long MainDeckhand { get; set; }

        public DepartureMessage(long mainDeckhand)
        {
            MainDeckhand = mainDeckhand;
        }
    }
}
