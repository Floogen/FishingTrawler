using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingTrawler.Framework.Interfaces
{
    public interface IDynamicReflectionsAPI
    {
        public bool IsDrawAnyReflection();
        public bool IsDrawingWaterReflection();
        public bool IsDrawingPuddleReflection();
        public bool IsDrawingMirrorReflection();

        public bool IsFilteringWater();
        public bool IsFilteringPuddles();
        public bool IsFilteringMirrors();
        public bool IsFilteringStars();
    }
}
