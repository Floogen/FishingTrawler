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
using StardewValley.Tools;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StardewValley.Objects.BedFurniture;

namespace FishingTrawler.Framework.Patches.Objects
{
    internal class FishingRodPatch : PatchTemplate
    {
        private readonly System.Type _object = typeof(FishingRod);

        public FishingRodPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal override void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, "doDoneFishing", new[] { typeof(bool) }), prefix: new HarmonyMethod(GetType(), nameof(DoDoneFishingPrefix)));
        }

        private static bool DoDoneFishingPrefix(FishingRod __instance, bool consumeBaitAndTackle)
        {
            if (__instance.attachments[1] != null && SeaborneTackle.IsValid(__instance.attachments[1]))
            {
                __instance.attachments[1].uses.Value = int.MinValue;
            }

            return true;
        }
    }
}
