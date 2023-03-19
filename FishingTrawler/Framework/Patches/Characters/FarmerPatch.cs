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

namespace FishingTrawler.Framework.Patches.Characters
{
    internal class FarmerPatch : PatchTemplate
    {
        private readonly System.Type _object = typeof(Farmer);

        public FarmerPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal override void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, "get_ActiveObject", null), postfix: new HarmonyMethod(GetType(), nameof(IsCarringPostfix)));
        }

        private static void IsCarringPostfix(Farmer __instance, ref Object __result)
        {
            if (CoalClump.IsValid(__result) || SeaborneTackle.IsValid(__result))
            {
                __result = null;
            }
        }
    }
}
