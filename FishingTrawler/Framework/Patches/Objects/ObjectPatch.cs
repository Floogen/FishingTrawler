using FishingTrawler.Framework.Objects.Items.Resources;
using FishingTrawler.Framework.Objects.Items.Rewards;
using FishingTrawler.Framework.Objects.Items.Tools;
using FishingTrawler.Framework.Utilities;
using FishingTrawler.Patches;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StardewValley.Objects.BedFurniture;

namespace FishingTrawler.Framework.Patches.Objects
{
    internal class ObjectPatch : PatchTemplate
    {
        private readonly System.Type _object = typeof(Object);

        public ObjectPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal override void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, "get_DisplayName", null), postfix: new HarmonyMethod(GetType(), nameof(GetNamePostfix)));
            harmony.Patch(AccessTools.Method(_object, "getDescription", null), postfix: new HarmonyMethod(GetType(), nameof(GetDescriptionPostfix)));

            harmony.Patch(AccessTools.Method(_object, nameof(Object.drawInMenu), new[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) }), prefix: new HarmonyMethod(GetType(), nameof(DrawInMenuPrefix)));
        }

        private static void GetNamePostfix(Object __instance, ref string __result)
        {
            if (CoalClump.IsValid(__instance))
            {
                switch (CoalClump.GetSize(__instance))
                {
                    default:
                    case 1:
                        __result = _helper.Translation.Get("item.coal_clump.name.small");
                        return;
                    case 2:
                        __result = _helper.Translation.Get("item.coal_clump.name.medium");
                        return;
                    case 3:
                        __result = _helper.Translation.Get("item.coal_clump.name.large");
                        return;
                }
            }
        }

        private static void GetDescriptionPostfix(Object __instance, ref string __result)
        {
            if (CoalClump.IsValid(__instance))
            {
                switch (CoalClump.GetSize(__instance))
                {
                    default:
                    case 1:
                        __result = _helper.Translation.Get("item.coal_clump.description.small");
                        return;
                    case 2:
                        __result = _helper.Translation.Get("item.coal_clump.description.medium");
                        return;
                    case 3:
                        __result = _helper.Translation.Get("item.coal_clump.description.large");
                        return;
                }
            }
        }

        private static bool DrawInMenuPrefix(Object __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            if (CoalClump.IsValid(__instance))
            {
                var xOffset = 0;
                switch (CoalClump.GetSize(__instance))
                {
                    default:
                    case 1:
                        xOffset = 0;
                        break;
                    case 2:
                        xOffset = 16;
                        break;
                    case 3:
                        xOffset = 32;
                        break;
                }
                spriteBatch.Draw(FishingTrawler.assetManager.coalClumpTexture, location + new Vector2(32f, 32f) * scaleSize, new Rectangle(xOffset, 0, 16, 16), color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, scaleSize * 3f, SpriteEffects.None, layerDepth);

                return false;
            }

            return true;
        }
    }
}
