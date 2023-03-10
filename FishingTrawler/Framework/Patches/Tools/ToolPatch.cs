using FishingTrawler.Framework.Objects.Items.Tools;
using FishingTrawler.Framework.Utilities;
using FishingTrawler.Patches;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingTrawler.Framework.Patches.Tools
{
    internal class ToolPatch : PatchTemplate
    {
        private readonly Type _object = typeof(Tool);

        public ToolPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal override void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, "get_DisplayName", null), postfix: new HarmonyMethod(GetType(), nameof(GetNamePostfix)));
            harmony.Patch(AccessTools.Method(_object, "get_description", null), postfix: new HarmonyMethod(GetType(), nameof(GetDescriptionPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Item), nameof(Item.canBeTrashed), null), postfix: new HarmonyMethod(GetType(), nameof(CanBeTrashedPostfix)));

            harmony.Patch(AccessTools.Method(_object, nameof(Tool.drawInMenu), new[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) }), prefix: new HarmonyMethod(GetType(), nameof(DrawInMenuPrefix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Tool.beginUsing), new[] { typeof(GameLocation), typeof(int), typeof(int), typeof(Farmer) }), prefix: new HarmonyMethod(GetType(), nameof(BeginUsingPrefix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Tool.tickUpdate), new[] { typeof(GameTime), typeof(Farmer) }), prefix: new HarmonyMethod(GetType(), nameof(TickUpdatePrefix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Tool.DoFunction), new[] { typeof(GameLocation), typeof(int), typeof(int), typeof(int), typeof(Farmer) }), prefix: new HarmonyMethod(GetType(), nameof(DoFunctionPrefix)));
        }

        private static void GetNamePostfix(Tool __instance, ref string __result)
        {
            if (__instance.modData.ContainsKey(ModDataKeys.BAILING_BUCKET_KEY))
            {
                __result = _helper.Translation.Get("item.bailing_bucket.name");
                return;
            }
        }

        private static void GetDescriptionPostfix(Tool __instance, ref string __result)
        {
            if (__instance.modData.ContainsKey(ModDataKeys.BAILING_BUCKET_KEY) && new BailingBucket(__instance) is BailingBucket bucket && bucket.IsValid)
            {
                __result = bucket.ContainsWater ? _helper.Translation.Get("item.bailing_bucket.description_full") : _helper.Translation.Get("item.bailing_bucket.description_empty");
                return;
            }
        }

        private static void CanBeTrashedPostfix(Tool __instance, ref bool __result)
        {
            if (__instance.modData.ContainsKey(ModDataKeys.BAILING_BUCKET_KEY))
            {
                __result = FishingTrawler.IsPlayerOnTrawler() is false;
                return;
            }
        }

        private static bool DrawInMenuPrefix(Tool __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            if (__instance.modData.ContainsKey(ModDataKeys.BAILING_BUCKET_KEY) && new BailingBucket(__instance) is BailingBucket bucket && bucket.IsValid)
            {
                int spriteOffset = bucket.ContainsWater ? 16 : 0;
                spriteBatch.Draw(FishingTrawler.assetManager.bucketTexture, location + new Vector2(32f, 32f), new Rectangle(spriteOffset, 0, 16, 16), color * transparency, 0f, new Vector2(8f, 8f), 4f * (scaleSize + bucket.Scale), SpriteEffects.None, layerDepth);

                return false;
            }

            return true;
        }

        private static bool BeginUsingPrefix(Tool __instance, ref bool __result, GameLocation location, int x, int y, Farmer who)
        {
            if (__instance.modData.ContainsKey(ModDataKeys.BAILING_BUCKET_KEY) && who == Game1.player && new BailingBucket(__instance) is BailingBucket bucket && bucket.IsValid)
            {
                __result = true;
                return bucket.Use(location, x, y, who);
            }

            return true;
        }

        private static bool TickUpdatePrefix(Tool __instance, ref Farmer ___lastUser, GameTime time, Farmer who)
        {
            if (__instance.modData.ContainsKey(ModDataKeys.BAILING_BUCKET_KEY) && who == Game1.player && new BailingBucket(__instance) is BailingBucket bucket && bucket.IsValid)
            {
                if (bucket.Scale > 0f)
                {
                    bucket.Scale -= 0.01f;
                    bucket.SaveData();
                }

                return false;
            }

            return true;
        }

        private static bool DoFunctionPrefix(Tool __instance, ref Farmer ___lastUser, GameLocation location, int x, int y, int power, Farmer who)
        {
            if (__instance.modData.ContainsKey(ModDataKeys.BAILING_BUCKET_KEY) && who == Game1.player && new BailingBucket(__instance) is BailingBucket bucket && bucket.IsValid)
            {
                return false;
            }

            return true;
        }
    }
}
