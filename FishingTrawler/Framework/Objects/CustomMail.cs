using FishingTrawler.Framework.Utilities;
using StardewModdingAPI;
using StardewValley;

namespace FishingTrawler.Objects
{
    internal class CustomMail : IAssetEditor
    {
        internal CustomMail()
        {
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Data\\mail");
        }

        public void Edit<T>(IAssetData asset)
        {
            var data = asset.AsDictionary<string, string>().Data;

            // Intro letter
            data[ModDataKeys.MAIL_FLAG_MURPHY_WAS_INTRODUCED] = string.Format(FishingTrawler.i18n.Get("letter.meet_murphy"), Game1.MasterPlayer.modData[ModDataKeys.MURPHY_DAY_TO_APPEAR]);

            // Ginger Island letter
            data[ModDataKeys.MAIL_FLAG_MURPHY_FOUND_GINGER_ISLAND] = string.Format(FishingTrawler.i18n.Get("letter.island_murphy"), Game1.MasterPlayer.modData[ModDataKeys.MURPHY_DAY_TO_APPEAR]);
        }
    }
}
