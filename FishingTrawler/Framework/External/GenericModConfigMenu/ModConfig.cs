namespace FishingTrawler.Framework.External.GenericModConfigMenu
{
    public class ModConfig
    {
        public int minimumFishingLevel = 3;
        public float fishPerNet = 1f;
        public int engineFishBonus = 2;
        public int eventFrequencyLower = 1;
        public int eventFrequencyUpper = 5;
        public string dayOfWeekChoice = "Wednesday";
        public string dayOfWeekChoiceIsland = "Saturday";
        internal static string[] murphyDayToAppear = new string[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
    }
}
