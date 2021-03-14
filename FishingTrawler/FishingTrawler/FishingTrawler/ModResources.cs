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
        internal static Texture2D boatTexture;

        internal static void SetUpAssets(IModHelper helper)
        {
            boatTexture = helper.Content.Load<Texture2D>(Path.Combine("assets", "Trawler.png"));
        }
    }
}
