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
        internal static string murphyTexturePath;
        internal static string murphyDialoguePath;

        internal static Texture2D murphyPortraitTexture;
        internal static Texture2D boatTexture;
        internal static Texture2D bucketTexture;
        internal static Texture2D uiTexture;

        internal static void SetUpAssets(IModHelper helper)
        {
            assetFolderPath = helper.Content.GetActualAssetKey("assets", ContentSource.ModFolder);
            murphyTexturePath = Path.Combine(assetFolderPath, "Characters", "Murphy.png");
            murphyDialoguePath = Path.Combine(assetFolderPath, "Characters", "Dialogue", "Murphy.json");

            // Load in textures
            murphyPortraitTexture = helper.Content.Load<Texture2D>(Path.Combine(assetFolderPath, "Characters", "MurphyPortrait.png"));
            boatTexture = helper.Content.Load<Texture2D>(Path.Combine(assetFolderPath, "Objects", "Trawler.png"));
            bucketTexture = helper.Content.Load<Texture2D>(Path.Combine(assetFolderPath, "Objects", "BailingBucket.png"));
            uiTexture = helper.Content.Load<Texture2D>(Path.Combine(assetFolderPath, "UI", "TrawlerUI.png"));
        }
    }
}
