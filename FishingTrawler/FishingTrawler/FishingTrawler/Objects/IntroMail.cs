using StardewModdingAPI;
using StardewValley;
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

            data["PeacefulEnd.FishingTrawler_WillyIntroducesMurphy"] = String.Format(ModEntry.i18n.Get("letter.meet_murphy"), Game1.MasterPlayer.modData[ModEntry.MURPHY_DAY_TO_APPEAR]);
        }
    }
}
