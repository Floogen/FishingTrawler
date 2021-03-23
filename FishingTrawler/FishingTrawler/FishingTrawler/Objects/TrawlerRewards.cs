using StardewValley;
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
            int[] eligibleFishIds = { };

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

                eligibleFishIds.Union(rawFishDataWithLocation.Keys);
            }

            Dictionary<int, string> fishData = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
            return eligibleFishIds.Union(fishData.Where(f => f.Value.Split('/')[1] == "trap").Select(f => f.Key).Where(i => !_forbiddenFish.Contains(i))).ToArray();
        }

        private static List<Object> CalculateReward(int[] keys, int amountOfFish, Farmer who)
        {
            Dictionary<int, string> fishData = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");

            List<Object> rewards = new List<Object>();
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

                        chance = Math.Min(chance, 0.89999997615814209);
                        if (Game1.random.NextDouble() <= chance)
                        {
                            caughtFish = true;
                            rewards.Add(new Object(Convert.ToInt32(keys[i]), 1));
                            break;
                        }
                    }
                    else if (who.FishingLevel >= Convert.ToInt32(specificFishData[12]))
                    {
                        double chance = Convert.ToDouble(specificFishData[10]);
                        double dropOffAmount = Convert.ToDouble(specificFishData[11]) * chance;
                        chance -= (double)Math.Max(0, Convert.ToInt32(specificFishData[9]) - 5) * dropOffAmount;
                        chance += (double)((float)who.FishingLevel / 50f);

                        chance = Math.Min(chance, 0.89999997615814209);
                        if (Game1.random.NextDouble() <= chance)
                        {
                            caughtFish = true;
                            rewards.Add(new Object(Convert.ToInt32(keys[i]), 1));
                            break;
                        }
                    }
                }

                if (!caughtFish)
                {
                    rewards.Add(new Object(Game1.random.Next(167, 173), 1));
                }
            }

            return rewards;
        }

        internal static void PopulatePlayerRewards(int amountOfFish, Farmer who)
        {
            ModEntry.rewardChest.items.AddRange(CalculateReward(GetEligibleFishIds(), amountOfFish, who));
        }
    }
}
