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
