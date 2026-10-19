namespace FishingTrawler.Framework.External.GenericModConfigMenu
{
    public class ModConfig
    {
        public bool allowMultipleTrips = false;
        public float minigameDuration = 2.5f;
        public int minimumFishingLevel = 3;
        public bool disableScreenFade = false;
        public bool useOldTrawlerSprite = false;
        public float fishPerNet = 1f;
        public int engineFishBonus = 1;
        public int hullEventFrequencyLower = 1;
        public int hullEventFrequencyUpper = 5;
        public int netEventFrequencyLower = 3;
        public int netEventFrequencyUpper = 8;
        public string dayOfWeekBeachStart = "Tuesday";
        public string dayOfWeekBeachEnd = "Thursday";
        public string dayOfWeekIslandStart = "Saturday";
        public string dayOfWeekIslandEnd = "Sunday";
        internal static string[] murphyDayToAppear = new string[] {"Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"};
    }
}
