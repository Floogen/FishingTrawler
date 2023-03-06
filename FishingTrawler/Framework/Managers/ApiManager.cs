using FishingTrawler.API.Interfaces;
using StardewModdingAPI;

namespace FishingTrawler.Framework.Managers
{
    public static class ApiManager
    {
        private static IMonitor monitor = FishingTrawler.monitor;
        private static IGenericModConfigMenuAPI genericModConfigMenuApi;
        private static IContentPatcherAPI contentPatcherApi;

        public static bool HookIntoGMCM(IModHelper helper)
        {
            genericModConfigMenuApi = helper.ModRegistry.GetApi<IGenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");

            if (genericModConfigMenuApi is null)
            {
                monitor.Log("Failed to hook into spacechase0.GenericModConfigMenu.", LogLevel.Error);
                return false;
            }

            monitor.Log("Successfully hooked into spacechase0.GenericModConfigMenu.", LogLevel.Debug);
            return true;
        }

        public static bool HookIntoContentPatcher(IModHelper helper)
        {
            contentPatcherApi = helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");

            if (contentPatcherApi is null)
            {
                monitor.Log("Failed to hook into Pathoschild.ContentPatcher.", LogLevel.Error);
                return false;
            }

            monitor.Log("Successfully hooked into Pathoschild.ContentPatcher.", LogLevel.Debug);
            return true;
        }

        public static IGenericModConfigMenuAPI GetGMCMInterface()
        {
            return genericModConfigMenuApi;
        }

        public static IContentPatcherAPI GetContentPatcherInterface()
        {
            return contentPatcherApi;
        }
    }
}
