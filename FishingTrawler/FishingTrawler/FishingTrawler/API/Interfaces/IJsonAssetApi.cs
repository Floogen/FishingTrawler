using System;
using System.Collections.Generic;

namespace FishingTrawler.API.Interfaces
{
    public interface IJsonAssetApi
    {
        void LoadAssets(string path);
        int GetWeaponId(string name);

        event EventHandler IdsAssigned;
    }
}
