using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingTrawler
{
    public static class ModResources
    {
        internal static string assetFolderPath;
        internal static Texture2D boatTexture;
        internal static Texture2D bucketTexture;
        internal static Texture2D uiTexture;

        internal static void SetUpAssets(IModHelper helper)
        {
            assetFolderPath = helper.Content.GetActualAssetKey("assets", ContentSource.ModFolder);

            // Load in textures
            boatTexture = helper.Content.Load<Texture2D>(Path.Combine(assetFolderPath, "Objects", "Trawler.png"));
            bucketTexture = helper.Content.Load<Texture2D>(Path.Combine(assetFolderPath, "Objects", "BailingBucket.png"));
            uiTexture = helper.Content.Load<Texture2D>(Path.Combine(assetFolderPath, "UI", "TrawlerUI.png"));
        }
    }
}
