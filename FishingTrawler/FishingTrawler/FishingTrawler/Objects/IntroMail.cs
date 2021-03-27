using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingTrawler.Objects
{
    internal class IntroMail : IAssetEditor
    {
        internal IntroMail()
        {
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Data\\mail");
        }

        public void Edit<T>(IAssetData asset)
        {
            var data = asset.AsDictionary<string, string>().Data;

            data["PeacefulEnd.FishingTrawler_WillyIntroducesMurphy"] = "Ahoy there!^ ^My old gambling buddy Murphy recently stopped by. He's looking for some deckhands willing to work on his fishing trawler. I brought your name up, as I've seen you fishing around town, and he seems eager to meet you.^ ^-Willy ^ ^P.S. Murphy also mentioned he would be on the far right docks (across the bridge) on Wednesdays and that he would leave before nightfall.";
        }
    }
}
