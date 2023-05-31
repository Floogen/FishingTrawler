using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System.Collections.Generic;
using System.IO;

namespace FishingTrawler.Framework.Managers
{
    public class AssetManager
    {
        private IMonitor _monitor;
        private IModHelper _modHelper;

        internal string assetFolderPath;
        internal string murphyTexturePath;
        internal string murphyDialoguePath;

        #region Textures
        // Character textures
        public Texture2D MurphyTexture { get { return Textures[GetTextureKey(_murphyTexturePath)]; } }
        private string _murphyTexturePath { get { return Path.Combine(assetFolderPath, "Characters", "Murphy.png"); } }

        public Texture2D MurphyPortraitTexture { get { return Textures[GetTextureKey(_murphyPortraitTexturePath)]; } }
        private string _murphyPortraitTexturePath { get { return Path.Combine(assetFolderPath, "Characters", "MurphyPortrait.png"); } }

        // Object textures
        public Texture2D AncientFlagsTexture { get { return Textures[GetTextureKey(_ancientFlagsTexturePath)]; } }
        private string _ancientFlagsTexturePath { get { return Path.Combine(assetFolderPath, "Objects", "Flags.png"); } }

        public Texture2D AnglerRingTexture { get { return Textures[GetTextureKey(_anglerRingTexturePath)]; } }
        private string _anglerRingTexturePath { get { return Path.Combine(assetFolderPath, "Objects", "AnglerRing.png"); } }

        public Texture2D LostFishingCharmTexture { get { return Textures[GetTextureKey(_lostFishingCharmTexturePath)]; } }
        private string _lostFishingCharmTexturePath { get { return Path.Combine(assetFolderPath, "Objects", "LostFishingCharm.png"); } }

        public Texture2D BoatTexture { get { return Textures[GetTextureKey(_boatTexturePath)]; } }
        private string _boatTexturePath { get { return Path.Combine(assetFolderPath, "Objects", "Trawler.png"); } }

        public Texture2D OldBoatTexture { get { return Textures[GetTextureKey(_oldBoatTexturePath)]; } }
        private string _oldBoatTexturePath { get { return Path.Combine(assetFolderPath, "Objects", "TrawlerOld.png"); } }

        public Texture2D BucketTexture { get { return Textures[GetTextureKey(_bucketTexturePath)]; } }
        private string _bucketTexturePath { get { return Path.Combine(assetFolderPath, "Objects", "BailingBucket.png"); } }

        public Texture2D CoalClumpTexture { get { return Textures[GetTextureKey(_coalClumpTexturePath)]; } }
        private string _coalClumpTexturePath { get { return Path.Combine(assetFolderPath, "Objects", "CoalClump.png"); } }

        public Texture2D FishingTacklesTexture { get { return Textures[GetTextureKey(_fishingTackleTexturePath)]; } }
        private string _fishingTackleTexturePath { get { return Path.Combine(assetFolderPath, "Objects", "FishingTackles.png"); } }

        public Texture2D TridentTexture { get { return Textures[GetTextureKey(_tridentTexturePath)]; } }
        private string _tridentTexturePath { get { return Path.Combine(assetFolderPath, "Objects", "Trident.png"); } }

        // Etc.
        public Texture2D UITexture { get { return Textures[GetTextureKey(_uiTexturePath)]; } }
        private string _uiTexturePath { get { return Path.Combine(assetFolderPath, "UI", "TrawlerUI.png"); } }
        #endregion

        // Collective assets for external editing
        public Dictionary<string, Texture2D> Textures { get; set; }

        public AssetManager(IMonitor monitor, IModHelper modHelper)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            Textures = new Dictionary<string, Texture2D>();

            assetFolderPath = _modHelper.ModContent.GetInternalAssetName(Path.Combine("Framework", "Assets")).Name;

            murphyTexturePath = _murphyTexturePath;
            murphyDialoguePath = Path.Combine(assetFolderPath, "Characters", "Dialogue", "Murphy.json");

            // Load in the internal textures
            RegisterTexture(_murphyTexturePath);
            RegisterTexture(_murphyPortraitTexturePath);

            RegisterTexture(_boatTexturePath);
            RegisterTexture(_oldBoatTexturePath);
            RegisterTexture(_ancientFlagsTexturePath);
            RegisterTexture(_anglerRingTexturePath);
            RegisterTexture(_lostFishingCharmTexturePath);
            RegisterTexture(_bucketTexturePath);
            RegisterTexture(_coalClumpTexturePath);
            RegisterTexture(_fishingTackleTexturePath);
            RegisterTexture(_tridentTexturePath);

            RegisterTexture(_uiTexturePath);
        }

        internal string GetTextureKey(string textureFilePath)
        {
            return $"PeacefulEnd/FishingTrawler/Framework/Assets/{Path.GetFileNameWithoutExtension(textureFilePath)}";
        }

        internal void RegisterTexture(string textureFilePath)
        {
            Textures[GetTextureKey(textureFilePath)] = _modHelper.ModContent.Load<Texture2D>(textureFilePath);
        }
    }
}
