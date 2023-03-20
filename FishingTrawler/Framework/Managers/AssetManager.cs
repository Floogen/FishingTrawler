using FishingTrawler.Framework.Objects.Items.Rewards;
using FishingTrawler.Framework.Utilities;
using FishingTrawler.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using System.IO;
using System.Linq;
using System.Threading;

namespace FishingTrawler.Framework.Managers
{
    public class AssetManager
    {
        private IMonitor _monitor;
        private IModHelper _modHelper;

        internal string assetFolderPath;
        internal string murphyTexturePath;
        internal string murphyDialoguePath;

        // Character textures
        internal Texture2D murphyPortraitTexture;

        // Object textures
        internal Texture2D ancientFlagsTexture;
        internal Texture2D anglerRingTexture;
        internal Texture2D lostFishingCharmTexture;
        internal Texture2D boatTexture;
        internal Texture2D bucketTexture;
        internal Texture2D coalClumpTexture;
        internal Texture2D fishingTacklesTexture;
        internal Texture2D tridentTexture;

        // Etc.
        internal Texture2D uiTexture;

        public AssetManager(IMonitor monitor, IModHelper modHelper)
        {
            _monitor = monitor;
            _modHelper = modHelper;

            assetFolderPath = _modHelper.ModContent.GetInternalAssetName(Path.Combine("Framework", "Assets")).Name;
            murphyTexturePath = Path.Combine(assetFolderPath, "Characters", "Murphy.png");
            murphyDialoguePath = Path.Combine(assetFolderPath, "Characters", "Dialogue", "Murphy.json");

            // Load in textures
            murphyPortraitTexture = _modHelper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "Characters", "MurphyPortrait.png"));

            boatTexture = _modHelper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "Objects", "Trawler.png"));
            ancientFlagsTexture = _modHelper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "Objects", "Flags.png"));
            anglerRingTexture = _modHelper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "Objects", "AnglerRing.png"));
            lostFishingCharmTexture = _modHelper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "Objects", "LostFishingCharm.png"));
            bucketTexture = _modHelper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "Objects", "BailingBucket.png"));
            coalClumpTexture = _modHelper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "Objects", "CoalClump.png"));
            fishingTacklesTexture = _modHelper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "Objects", "FishingTackles.png"));
            tridentTexture = _modHelper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "Objects", "Trident.png"));

            uiTexture = _modHelper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI", "TrawlerUI.png"));
        }
    }
}
