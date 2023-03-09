using FishingTrawler.Framework.Interfaces;
using StardewModdingAPI;
using System.Threading;

namespace FishingTrawler.Framework.Managers
{
    public class ApiManager
    {
        private IMonitor _monitor;
        private IGenericModConfigMenuAPI genericModConfigMenuApi;
        private IContentPatcherAPI contentPatcherApi;

        public ApiManager(IMonitor monitor)
        {
            _monitor = monitor;
        }

        internal bool HookIntoGMCM(IModHelper helper)
        {
            genericModConfigMenuApi = helper.ModRegistry.GetApi<IGenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");

            if (genericModConfigMenuApi is null)
            {
                _monitor.Log("Failed to hook into spacechase0.GenericModConfigMenu.", LogLevel.Error);
                return false;
            }

            _monitor.Log("Successfully hooked into spacechase0.GenericModConfigMenu.", LogLevel.Debug);
            return true;
        }

        internal bool HookIntoContentPatcher(IModHelper helper)
        {
            contentPatcherApi = helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");

            if (contentPatcherApi is null)
            {
                _monitor.Log("Failed to hook into Pathoschild.ContentPatcher.", LogLevel.Error);
                return false;
            }

            _monitor.Log("Successfully hooked into Pathoschild.ContentPatcher.", LogLevel.Debug);
            return true;
        }

        internal IGenericModConfigMenuAPI GetGMCMInterface()
        {
            return genericModConfigMenuApi;
        }

        internal IContentPatcherAPI GetContentPatcherInterface()
        {
            return contentPatcherApi;
        }
    }
}
