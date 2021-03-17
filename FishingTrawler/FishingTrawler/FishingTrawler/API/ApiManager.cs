using FishingTrawler.API.Interfaces;
using StardewModdingAPI;

namespace FishingTrawler.API
{
    public static class ApiManager
    {
        private static IJsonAssetApi jsonAssetApi;

        public static void HookIntoJsonAssets(IModHelper helper)
        {
            jsonAssetApi = helper.ModRegistry.GetApi<IJsonAssetApi>("spacechase0.JsonAssets");

            if (jsonAssetApi is null)
            {
                ModEntry.monitor.Log("Failed to hook into spacechase0.JsonAssets.", LogLevel.Error);
                return;
            }

            ModEntry.monitor.Log("Successfully hooked into spacechase0.JsonAssets.", LogLevel.Debug);
        }

        public static IJsonAssetApi GetJsonAssetInterface()
        {
            return jsonAssetApi;
        }

        public static int GetBailingBucketID()
        {
            return jsonAssetApi.GetWeaponId("Bailing Bucket");
        }
    }
}
