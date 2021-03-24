using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace FishingTrawler.Objects
{
    internal static class TrawlerRewards
    {
        private static readonly int[] _forbiddenFish = new int[] { 159, 160, 163, 775, 682, 898, 899, 900, 901 };

        private static int[] GetEligibleFishIds(bool limitToOceanFish = true)
        {
            List<int> eligibleFishIds = new List<int>();

            // Iterate through any valid locations to find the fish eligible for rewarding (fish need to be in season and player must have minimum level for it)
            Dictionary<string, string> locationData = Game1.content.Load<Dictionary<string, string>>("Data\\Locations");
            foreach (GameLocation location in Game1.locations.Where(l => l.Name == (limitToOceanFish ? "Beach" : l.Name)))
            {
                if (!locationData.ContainsKey(location.Name))
                {
                    continue;
                }

                string[] rawFishData = locationData[location.Name].Split('/')[4 + Utility.getSeasonNumber(Game1.currentSeason)].Split(' ');
                Dictionary<int, string> rawFishDataWithLocation = new Dictionary<int, string>();
                if (rawFishData.Length > 1)
                {
                    for (int j = 0; j < rawFishData.Length; j += 2)
                    {
                        rawFishDataWithLocation[Convert.ToInt32(rawFishData[j])] = rawFishData[j + 1];
                    }
                }

                eligibleFishIds.AddRange(rawFishDataWithLocation.Keys);
            }

            Dictionary<int, string> fishData = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
            eligibleFishIds.AddRange(fishData.Where(f => f.Value.Split('/')[1] == "trap").Select(f => f.Key).Where(i => !_forbiddenFish.Contains(i)));

            return eligibleFishIds.ToArray();
        }

        internal static void CalculateAndPopulateReward(Chest rewardChest, int amountOfFish, Farmer who, int baseXpReduction = 5)
        {
            int[] keys = GetEligibleFishIds();
            Dictionary<int, string> fishData = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");

            int xpReward = 3;
            for (int x = 0; x < amountOfFish; x++)
            {
                bool caughtFish = false;

                Utility.Shuffle(Game1.random, keys);
                for (int i = 0; i < keys.Length; i++)
                {
                    string[] specificFishData = fishData[Convert.ToInt32(keys[i])].Split('/');

                    if (specificFishData[1] == "trap")
                    {
                        double chance = Convert.ToDouble(specificFishData[2]);
                        chance += (double)((float)who.FishingLevel / 50f);
                        chance /= 1.2f;  // Reduce chance of trap-based catches by 1.2

                        chance = Math.Min(chance, 0.89999997615814209);
                        if (Game1.random.NextDouble() <= chance)
                        {
                            caughtFish = true;
                            rewardChest.addItem(new Object(Convert.ToInt32(keys[i]), 1));
                            xpReward += 5; // Crab pot always give 5 XP per Vanilla
                            break;
                        }
                    }
                    else if (who.FishingLevel >= Convert.ToInt32(specificFishData[12]))
                    {
                        int difficulty = Convert.ToInt32(specificFishData[1]);
                        double chance = Convert.ToDouble(specificFishData[10]);
                        double dropOffAmount = Convert.ToDouble(specificFishData[11]) * chance;
                        chance -= (double)Math.Max(0, Convert.ToInt32(specificFishData[9]) - 5) * dropOffAmount;
                        chance += (double)((float)who.FishingLevel / 50f);

                        chance = Math.Min(chance, 0.89999997615814209);
                        if (Game1.random.NextDouble() <= chance)
                        {
                            caughtFish = true;
                            rewardChest.addItem(new Object(Convert.ToInt32(keys[i]), 1));
                            xpReward += 3 + (difficulty / 3);
                            break;
                        }
                    }
                }

                if (!caughtFish)
                {
                    rewardChest.addItem(new Object(Game1.random.Next(167, 173), 1));
                }
            }

            // Now give XP reward (give 5% of total caught XP)
            who.gainExperience(1, xpReward % (100 - baseXpReduction));
        }
    }
}
