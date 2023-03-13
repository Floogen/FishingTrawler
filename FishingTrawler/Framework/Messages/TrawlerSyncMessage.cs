namespace FishingTrawler.Messages
{
    public enum SyncType
    {
        Unknown,
        WaterLevel,
        FishCaught,
        Fuel,
        RestartGPS,
        TripTimer
    }

    internal class TrawlerSyncMessage
    {
        public SyncType SyncType { get; set; }
        public int Quantity { get; set; }

        public TrawlerSyncMessage()
        {

        }

        public TrawlerSyncMessage(SyncType syncType, int waterLevel)
        {
            SyncType = syncType;
            Quantity = waterLevel;
        }
    }
}
